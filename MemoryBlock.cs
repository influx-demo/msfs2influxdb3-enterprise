using System;
using FSUIPC;

namespace MSFS2MQTTBridge
{
    public class MemoryBlock
    {
        public readonly int _offset;
        private readonly int _size;
        private byte[] _data;
        private readonly Offset<byte[]> _blockOffset;

        public MemoryBlock(int offset, int size)
        {
            _offset = offset;
            _size = size;
            _data = new byte[size];
            _blockOffset = new Offset<byte[]>(offset, size);
        }

        public bool Read()
        {
            try
            {
                // Use the existing offset - Process() without arguments processes all registered offsets
                FSUIPCConnection.Process();
                _data = _blockOffset.Value;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error reading memory block at offset 0x{_offset:X4} with size {_size}: {ex.Message}"
                );
                return false;
            }
        }

        // Generic value extraction methods
        public long GetInt64(int relativeOffset) => BitConverter.ToInt64(_data, relativeOffset);

        public int GetInt32(int relativeOffset) => BitConverter.ToInt32(_data, relativeOffset);

        public uint GetUInt32(int relativeOffset) => BitConverter.ToUInt32(_data, relativeOffset);

        public short GetInt16(int relativeOffset) => BitConverter.ToInt16(_data, relativeOffset);

        public ushort GetUInt16(int relativeOffset) => BitConverter.ToUInt16(_data, relativeOffset);

        public byte GetByte(int relativeOffset) => _data[relativeOffset];

        public double GetDouble(int relativeOffset) => BitConverter.ToDouble(_data, relativeOffset);
    }
}
