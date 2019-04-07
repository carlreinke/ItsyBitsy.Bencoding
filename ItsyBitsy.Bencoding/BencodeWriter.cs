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
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A writer that provides generation of bencoded data.
    /// </summary>
    public sealed class BencodeWriter : IBencodeWriter
    {
        private const int _int32MaxDigits = 10;

        private const int _int64MaxDigits = 19;

        private readonly Stack<PreviousKeyRange> _previousKeyStack;

        private byte[] _buffer;

        private int _index = 0;

        private BencodeSpanWriter.State _state = BencodeSpanWriter.State.Initial;

        private BitStack _scopeStack;

        private int _previousKeyIndex = -1;

        private int _previousKeyLength = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeWriter"/> class.
        /// </summary>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     misordering and duplication as they are being written.</param>
        public BencodeWriter(bool skipValidation = false)
        {
            _buffer = Array.Empty<byte>();
            if (!skipValidation)
                _previousKeyStack = new Stack<PreviousKeyRange>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeWriter"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the writer.</param>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     misordering and duplication as they are being written.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.
        ///     </exception>
        public BencodeWriter(int capacity, bool skipValidation = false)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new byte[capacity];
            if (!skipValidation)
                _previousKeyStack = new Stack<PreviousKeyRange>();
        }

        /// <summary>
        /// Gets the current length of the encoded data.
        /// </summary>
        public int Length => _index;

        internal int Capacity => _buffer.Length;

        /// <summary>
        /// Removes all encoded data from the writer.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _index);
            ArrayPools.Bytes.Return(_buffer);

            _previousKeyStack?.Clear();
            _buffer = Array.Empty<byte>();
            _index = 0;
            _state = BencodeSpanWriter.State.Initial;
            _scopeStack.Clear();
            _previousKeyIndex = -1;
            _previousKeyLength = 0;
        }

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows an
        ///     integer to be written.</exception>
        public void WriteInteger(long value)
        {
            _state = BencodeSpanWriter.GetStateAfterValue(_state);

            try
            {
                // 'i', '-', digits, 'e'
                EnsureWriteCapacity(1 + (1 + _int64MaxDigits) + 1);

                var span = _buffer.AsSpan();

                span[_index] = (byte)'i';
                _index += 1;

                if (!Utf8Formatter.TryFormat(value, span.Slice(_index), out int bytesWritten))
                {
                    Debug.Assert(false);  // This should be unreachable.
                    throw new InvalidOperationException("Unexpected behavior.");
                }
                _index += bytesWritten;

                span[_index] = (byte)'e';
                _index += 1;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     string to be written.</exception>
        public void WriteString(ReadOnlySpan<byte> value)
        {
            _state = BencodeSpanWriter.GetStateAfterValue(_state);

            try
            {
                // digits, ':', body
                EnsureWriteCapacity(_int32MaxDigits + 1 + value.Length);

                var span = _buffer.AsSpan();

                if (!Utf8Formatter.TryFormat(value.Length, span.Slice(_index), out int bytesWritten))
                {
                    Debug.Assert(false);  // This should be unreachable.
                    throw new InvalidOperationException("Unexpected behavior.");
                }
                _index += bytesWritten;

                span[_index] = (byte)':';
                _index += 1;

                value.CopyTo(span.Slice(_index));
                _index += value.Length;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list head to be written.</exception>
        public void WriteListHead()
        {
            _state = BencodeSpanWriter.EnterListScope(_state, ref _scopeStack);

            try
            {
                EnsureWriteCapacity(1);

                _buffer[_index] = (byte)'l';
                _index += 1;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list tail to be written.</exception>
        public void WriteListTail()
        {
            _state = BencodeSpanWriter.ExitListScope(_state, ref _scopeStack);

            try
            {
                EnsureWriteCapacity(1);

                _buffer[_index] = (byte)'e';
                _index += 1;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary head to be written.</exception>
        public void WriteDictionaryHead()
        {
            _state = BencodeSpanWriter.EnterDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKeyStack.Push(new PreviousKeyRange(_previousKeyIndex, _previousKeyLength));
                _previousKeyIndex = -1;
                _previousKeyLength = 0;
            }

            try
            {
                EnsureWriteCapacity(1);

                _buffer[_index] = (byte)'d';
                _index += 1;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary tail to be written.</exception>
        public void WriteDictionaryTail()
        {
            _state = BencodeSpanWriter.ExitDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                Debug.Assert(_previousKeyStack.Count > 0);

                var previousKey = _previousKeyStack.Pop();
                _previousKeyIndex = previousKey.Index;
                _previousKeyLength = previousKey.Length;
            }

            try
            {
                EnsureWriteCapacity(1);

                _buffer[_index] = (byte)'e';
                _index += 1;
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a key.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer was constructed with validation
        ///     enabled and the key being written is misordered or duplicated.</exception>
        public void WriteKey(ReadOnlySpan<byte> key)
        {
            _state = BencodeSpanWriter.GetStateAfterKey(_state);

            // Ensure that keys are ordered and unique.
            if (_previousKeyIndex != -1)
            {
                var previousKeySpan = _buffer.AsSpan(_previousKeyIndex, _previousKeyLength);

                if (!IsLess(previousKeySpan, key))
                    throw new InvalidOperationException("Keys must be ordered and unique.");
            }

            try
            {
                // digits, ':', body
                EnsureWriteCapacity(_int32MaxDigits + 1 + key.Length);

                var span = _buffer.AsSpan();

                if (!Utf8Formatter.TryFormat(key.Length, span.Slice(_index), out int bytesWritten))
                {
                    Debug.Assert(false);  // This should be unreachable.
                    throw new InvalidOperationException("Unexpected behavior.");
                }
                _index += bytesWritten;

                span[_index] = (byte)':';
                _index += 1;

                int keyIndex = _index;

                key.CopyTo(span.Slice(_index));
                _index += key.Length;

                if (_previousKeyStack != null)
                {
                    _previousKeyIndex = keyIndex;
                    _previousKeyLength = key.Length;
                }
            }
            catch
            {
                _state = BencodeSpanWriter.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Returns a buffer containing the encoded data.
        /// </summary>
        /// <returns>A buffer containing the encoded data.</returns>
        /// <exception cref="InvalidOperationException">The value being written is incomplete.
        ///     </exception>
        public byte[] Encode()
        {
            if (_state != BencodeSpanWriter.State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            return _buffer.AsSpan(0, _index).ToArray();
        }

        /// <summary>
        /// Writes the encoded data into a buffer.
        /// </summary>
        /// <param name="destination">The buffer that the encoded data will be written into.</param>
        /// <exception cref="InvalidOperationException">The value being written is incomplete.
        ///     </exception>
        /// <exception cref="ArgumentException">The length of the encoded data is greater than the
        ///     length of the buffer.</exception>
        public void EncodeTo(Span<byte> destination)
        {
            if (_state != BencodeSpanWriter.State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            _buffer.AsSpan(0, _index).CopyTo(destination);
        }

        /// <summary>
        /// Attempts to write the encoded data into a buffer.
        /// </summary>
        /// <param name="destination">The buffer that the encoded data will be written into.</param>
        /// <returns><see langword="false"/> if the length of the encoded data is greater than the
        ///     length of the buffer; otherwise, <see langword="true"/>.</returns>
        /// <exception cref="InvalidOperationException">The value being written is incomplete.
        ///     </exception>
        public bool TryEncodeTo(Span<byte> destination)
        {
            if (_state != BencodeSpanWriter.State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            return _buffer.AsSpan(0, _index).TryCopyTo(destination);
        }

        /// <summary>
        /// Transfers ownership of the encoded data buffers and clears the writer.
        /// </summary>
        /// <returns>The encoded data buffers.</returns>
        /// <exception cref="InvalidOperationException">The value being written is incomplete.
        ///     </exception>
        public ReadOnlyMemory<byte> TransferEncoded()
        {
            if (_state != BencodeSpanWriter.State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            var buffer = _buffer.AsMemory(0, _index);
            _buffer = Array.Empty<byte>();
            _index = 0;
            Clear();
            return buffer;
        }

        private static bool IsLess(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            for (int i = 0; i < y.Length; ++i)
            {
                // Check if `x` ends first.
                if (i == x.Length)
                    return true;

                // Compare first position with differing data.
                if (x[i] != y[i])
                    return x[i] < y[i];
            }

            // Otherwise `y` ends first or both are equal.
            return false;
        }

        private void EnsureWriteCapacity(int pendingCount)
        {
            if (pendingCount < 0)
                throw new OverflowException();

            if (_buffer.Length - _index < pendingCount)
            {
                const int blockSize = 1024;
                int blocks = checked(_index + pendingCount + (blockSize - 1)) / blockSize;
                byte[] newBuffer = ArrayPools.Bytes.Rent(blockSize * blocks);

                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _index);
                Array.Clear(_buffer, 0, _index);
                ArrayPools.Bytes.Return(_buffer);

                _buffer = newBuffer;
            }
        }

        private struct PreviousKeyRange
        {
            public readonly int Index;

            public readonly int Length;

            public PreviousKeyRange(int index, int length)
            {
                Index = index;
                Length = length;
            }
        }
    }
}
