//
// Copyright (C) 2019  Carl Reinke
//
// This file is part of ItsyBitsy.Bencoding.
//
// This program is free software; you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this program;
// if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
// 02110-1301, USA.
//
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static ItsyBitsy.Bencoding.BencodeSpanWriter;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A writer that provides generation of bencoded data.
    /// </summary>
    /// <seealso cref="BencodeSpanWriter"/>
    public sealed class BencodeWriter
    {
        private const int _intMaxDigits = 10;

        private const int _longMaxDigits = 19;

        private readonly IBufferWriter<byte> _buffer;

        private readonly Stack<PreviousKey>? _previousKeyStack;

        private Memory<byte> _memory;

        private int _bufferedLength;

        private State _state;

        private BitStack _scopeStack;

        private PreviousKey _previousKey;

        /// <summary>
        /// Initializes a new <see cref="BencodeWriter"/> instance using the specified destination
        /// buffer.
        /// </summary>
        /// <param name="destination">The destination to write the encoded data into.</param>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     mis-ordering and duplication as they are being written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is
        ///     <see langword="null"/>.</exception>
        public BencodeWriter(IBufferWriter<byte> destination, bool skipValidation = false)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            _buffer = destination;
            _memory = destination.GetMemory();
            _bufferedLength = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
            _previousKey = PreviousKey.None;
            _previousKeyStack = skipValidation ? null : new Stack<PreviousKey>();
        }

        /// <summary>
        /// Gets the number of bytes that have been written but not yet flushed to the underlying
        /// <see cref="IBufferWriter{T}"/>.
        /// </summary>
        public int BufferedLength => _bufferedLength;

        /// <summary>
        /// Gets a <see cref="BencodeSpanWriter"/> that continues from the current state of this
        /// writer.
        /// </summary>
        /// <returns>A <see cref="BencodeSpanWriter"/> that continues from the current state of this
        ///     writer.</returns>
        /// <remarks>
        /// Do not operate on this writer again until after calling
        /// <see cref="BencodeSpanWriter.Dispose"/> on the <see cref="BencodeSpanWriter"/>.
        /// </remarks>
        public BencodeSpanWriter CreateSpanWriter()
        {
            Flush();

            var spanWriter = new BencodeSpanWriter(this, _buffer, _state, _scopeStack, _previousKey, _previousKeyStack);

            _state = State.Error;

            return spanWriter;
        }

        /// <summary>
        /// Advances the underlying <see cref="IBufferWriter{T}"/>.
        /// </summary>
        /// <param name="final">Indicates whether the value should be complete.</param>
        /// <exception cref="InvalidOperationException"><paramref name="final"/> is
        ///     <see langword="true"/> and the value is incomplete.</exception>
        public void Flush(bool final = true)
        {
            if (final && _state != State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            Flush();
        }

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows an
        ///     integer to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the integer.</exception>
        public void WriteInteger(long value)
        {
            _state = GetStateAfterValue(_state);

            try
            {
                Span<byte> numberBuffer = stackalloc byte[1 + _longMaxDigits];
                if (!Utf8Formatter.TryFormat(value, numberBuffer, out int numberLength))
                    throw new UnreachableException();

                EnsureCapacity(1 + numberLength + 1);

                var span = _memory.Span;
                span[0] = (byte)'i';
                span = span.Slice(1);
                numberBuffer.Slice(0, numberLength).CopyTo(span);
                span = span.Slice(numberLength);
                span[0] = (byte)'e';

                Advance(1 + numberLength + 1);

            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     string to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the string.</exception>
        public void WriteString(ReadOnlySpan<byte> value)
        {
            _state = GetStateAfterValue(_state);

            WriteStringInternal(value);
        }

        /// <summary>
        /// Writes the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list head to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the list head.</exception>
        public void WriteListHead()
        {
            _state = EnterListScope(_state, ref _scopeStack);

            try
            {
                EnsureCapacity(1);

                _memory.Span[0] = (byte)'l';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list tail to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the list tail.</exception>
        public void WriteListTail()
        {
            _state = ExitListScope(_state, ref _scopeStack);

            try
            {
                EnsureCapacity(1);

                _memory.Span[0] = (byte)'e';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary head to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the dictionary head.</exception>
        public void WriteDictionaryHead()
        {
            _state = EnterDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKeyStack.Push(_previousKey);
                _previousKey = PreviousKey.None;
            }

            try
            {
                EnsureCapacity(1);

                _memory.Span[0] = (byte)'d';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary tail to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the dictionary tail.</exception>
        public void WriteDictionaryTail()
        {
            _state = ExitDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKey = _previousKeyStack.Pop();
            }

            try
            {
                EnsureCapacity(1);

                _memory.Span[0] = (byte)'e';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a key.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the key.</exception>
        /// <exception cref="InvalidOperationException">The writer was constructed with validation
        ///     enabled and the key being written is mis-ordered or duplicated.</exception>
        public void WriteKey(ReadOnlySpan<byte> key)
        {
            _state = GetStateAfterKey(_state);

            if (_previousKey.HasValue && !IsLess(_previousKey.Span, key))
                throw new InvalidOperationException("Keys must be ordered and unique.");

            if (_previousKeyStack != null)
                _previousKey.CopyFrom(key);

            WriteStringInternal(key);
        }

        internal void RestoreState(State state, BitStack scopeStack, PreviousKey previousKey)
        {
            _state = state;
            _scopeStack = scopeStack;
            _previousKey = previousKey;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int pendingLength)
        {
            if (_memory.Length < pendingLength)
                FlushAndEnsureCapacity(pendingLength);
        }

        private void FlushAndEnsureCapacity(int pendingLength)
        {
            if (_bufferedLength > 0)
            {
                _buffer.Advance(_bufferedLength);
                _bufferedLength = 0;
            }
            _memory = _buffer.GetMemory(pendingLength);

            if (_memory.Length < pendingLength)
                throw new InvalidOperationException("Reached the end of the destination buffer while attempting to write.");
        }

        private void FlushAndRequestCapacity(int pendingLength)
        {
            if (_bufferedLength > 0)
            {
                _buffer.Advance(_bufferedLength);
                _bufferedLength = 0;
            }
            _memory = _buffer.GetMemory(pendingLength);

            if (_memory.Length == 0)
                throw new InvalidOperationException("Reached the end of the destination buffer while attempting to write.");
        }

        private void Flush()
        {
            if (_bufferedLength > 0)
            {
                _buffer.Advance(_bufferedLength);
                _bufferedLength = 0;
            }
            _memory = Memory<byte>.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Advance(int length)
        {
            _memory = _memory.Slice(length);
            _bufferedLength += length;
        }

        private void WriteStringInternal(ReadOnlySpan<byte> value)
        {
            try
            {
                Span<byte> numberBuffer = stackalloc byte[_intMaxDigits];
                if (!Utf8Formatter.TryFormat(value.Length, numberBuffer, out int numberLength))
                    throw new UnreachableException();

                EnsureCapacity(numberLength + 1);

                var span = _memory.Span;
                numberBuffer.Slice(0, numberLength).CopyTo(span);
                span = span.Slice(numberLength);
                span[0] = (byte)':';

                Advance(numberLength + 1);

                WriteIncrementally(value);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteIncrementally(ReadOnlySpan<byte> value)
        {
            for (; ; )
            {
                Span<byte> span = _memory.Span;
                int length = span.Length;

                if (length >= value.Length)
                {
                    value.CopyTo(span);
                    Advance(value.Length);
                    return;
                }

                value.Slice(0, length).CopyTo(span);
                Advance(length);

                value = value.Slice(length);

                FlushAndRequestCapacity(0);
            }
        }
    }
}
