using System;
using System.Buffers;
using System.Text;

namespace CSharp.Net.Util
{
    public ref struct Utf8ValueBuilder
    {
        private byte[] _buffer;
        private Span<byte> _span;
        private int _offset;
        public int Length => _offset;

        public Utf8ValueBuilder(int initialSize)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialSize);
            _span = _buffer;
            _offset = 0;
        }

        /// <summary>
        /// 写入单字节
        /// </summary>
        /// <param name="value"></param>
        public void Append(byte value)
        {
            EnsureCapacity(1);
            _span[_offset++] = value;
        }

        /// <summary>
        /// 写入 char（ASCII 快速路径）
        /// </summary>
        /// <param name="ch"></param>
        public void Append(char ch)
        {
            if (ch <= 0x7F)
            {
                EnsureCapacity(1);
                _span[_offset++] = (byte)ch;
            }
            else
            {
                Append(ch.ToString());
            }
        }

        // 写入字符串（UTF8 编码）
        public void Append(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            int byteCount = Encoding.UTF8.GetByteCount(text);
            EnsureCapacity(byteCount);
#if NET5_0_OR_GREATER
            _offset += Encoding.UTF8.GetBytes(text, _span.Slice(_offset));
#else
            _offset += Encoding.UTF8.GetBytes(text, 0, text.Length, _buffer, _offset);
#endif
        }

        public void Append(DateTime dateTime)
        {
            EnsureCapacity(23);
            _offset = DateTimeHelper.WriteDateTime(dateTime, _buffer, _offset);
        }
        /// <summary>
        /// 毫秒级
        /// </summary>
        /// <param name="dt"></param>
        public void AppendUnixMilliseconds(DateTime dt)
        {
            long ms = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
            AppendInt64(ms);
        }
        /// <summary>
        /// 秒级
        /// </summary>
        /// <param name="dt"></param>
        public void AppendUnixSeconds(DateTime dt)
        {
            long seconds = new DateTimeOffset(dt).ToUnixTimeSeconds();

            AppendInt64(seconds);
        }

        public void Append(int value)
        {
            AppendInt64(value);
        }
        public void Append(long value)
        {
            AppendInt64(value);
        }
        /// <summary>
        /// Guid
        /// </summary>
        /// <param name="value"></param>
        public void Append(Guid value)
        {
            EnsureCapacity(36);
#if NET5_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[16];
            value.TryWriteBytes(bytes);
#else
            byte[] bytes = value.ToByteArray();
#endif

            WriteHex(bytes[3]);
            WriteHex(bytes[2]);
            WriteHex(bytes[1]);
            WriteHex(bytes[0]);
            _span[_offset++] = (byte)'-';

            WriteHex(bytes[5]);
            WriteHex(bytes[4]);
            _span[_offset++] = (byte)'-';

            WriteHex(bytes[7]);
            WriteHex(bytes[6]);
            _span[_offset++] = (byte)'-';

            WriteHex(bytes[8]);
            WriteHex(bytes[9]);
            _span[_offset++] = (byte)'-';

            WriteHex(bytes[10]);
            WriteHex(bytes[11]);
            WriteHex(bytes[12]);
            WriteHex(bytes[13]);
            WriteHex(bytes[14]);
            WriteHex(bytes[15]);
        }


        /// <summary>
        /// 写入 ReadOnlySpan<char>
        /// </summary>
        /// <param name="text"></param>
        public void Append(ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
                return;
#if NET5_0_OR_GREATER

            int byteCount = Encoding.UTF8.GetByteCount(text);
            EnsureCapacity(byteCount);
            _offset += Encoding.UTF8.GetBytes(text, _span.Slice(_offset));
#else
            var s = text.ToString();
            int byteCount = Encoding.UTF8.GetByteCount(s);
            EnsureCapacity(byteCount);
            _offset += Encoding.UTF8.GetBytes(s, 0, s.Length, _buffer, _offset);
#endif

        }

        /// <summary>
        /// 写入 Span<byte>（已编码数据）
        /// </summary>
        /// <param name="bytes"></param>
        public void AppendBytes(ReadOnlySpan<byte> bytes)
        {
            EnsureCapacity(bytes.Length);
            bytes.CopyTo(_span.Slice(_offset));
            _offset += bytes.Length;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            int maxBytes = Encoding.UTF8.GetMaxByteCount(text.Length);

            EnsureCapacity(maxBytes);

#if NET5_0_OR_GREATER
            _offset += Encoding.UTF8.GetBytes(
                text.AsSpan(),
                _span[_offset..]);
#else
            string s = text.ToString();
            _offset += Encoding.UTF8.GetBytes(s, 0, s.Length, _buffer, _offset);
#endif
        }

        private static ReadOnlySpan<byte> Hex => "0123456789abcdef"u8;
        private void WriteHex(byte b)
        {
            _span[_offset++] = Hex[b >> 4];
            _span[_offset++] = Hex[b & 0xF];
        }

        private void AppendInt64(long value)
        {
            // 预留最大长度：-9223372036854775808 → 20
            EnsureCapacity(20);

            if (value == 0)
            {
                _span[_offset++] = (byte)'0';
                return;
            }

            if (value < 0)
            {
                _span[_offset++] = (byte)'-';
                value = -value;
            }

            // 反向写入
            int start = _offset;

            while (value > 0)
            {
                long div = value / 10;
                int digit = (int)(value - div * 10);
                _span[_offset++] = (byte)('0' + digit);
                value = div;
            }

            // 反转
            _span.Slice(start, _offset - start).Reverse();
        }

        private void Write2(int value)
        {
            _span[_offset++] = (byte)('0' + (value / 10));
            _span[_offset++] = (byte)('0' + (value % 10));
        }

        private void Write3(int value)
        {
            _span[_offset++] = (byte)('0' + (value / 100));
            _span[_offset++] = (byte)('0' + ((value / 10) % 10));
            _span[_offset++] = (byte)('0' + (value % 10));
        }

        private void Write4(int value)
        {
            _span[_offset++] = (byte)('0' + (value / 1000));
            _span[_offset++] = (byte)('0' + ((value / 100) % 10));
            _span[_offset++] = (byte)('0' + ((value / 10) % 10));
            _span[_offset++] = (byte)('0' + (value % 10));
        }

        // 扩容
        private void EnsureCapacity(int additionalCapacity)
        {
            if (_span.Length - _offset >= additionalCapacity)
                return;

            Grow(additionalCapacity);
        }
        /// <summary>
        /// 扩容
        /// </summary>
        /// <param name="additionalCapacity"></param>
        private void Grow(int additionalCapacity)
        {
            int newSize = Math.Max(
                _offset + additionalCapacity,
                _span.Length * 2);

            byte[] newArray =
                ArrayPool<byte>.Shared.Rent(newSize);

            _span.Slice(0, _offset).CopyTo(newArray);
            // _span[.._offset].CopyTo(newArray);

            if (_buffer != null)
                ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = newArray;
            _span = newArray;
        }

        /// <summary>
        /// 导出结果（拷贝安全版）
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] result = new byte[_offset];
            _span.Slice(0, _offset).CopyTo(result);
            return result;
        }

        /// <summary>
        /// 导出 Span（零拷贝）
        /// </summary>
        /// <returns></returns>
        public ReadOnlySpan<byte> AsSpan()
            => _span.Slice(0, _offset);//_span[.._offset];

        /// <summary>
        /// 释放池内存
        /// </summary>
        public void Dispose()
        {
            var arr = _buffer;
            if (arr != null)
            {
                _buffer = null;
                ArrayPool<byte>.Shared.Return(arr);
                //_buffer = null;
            }
        }
    }
}
