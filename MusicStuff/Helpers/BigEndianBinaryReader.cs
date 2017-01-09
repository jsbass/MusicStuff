using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MidiParser.Helpers
{
    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        private byte[] ReadLittleIndian(int count)
        {
            var bytes = ReadBytes(count);
            Array.Reverse(bytes);
            return bytes;
        }

        public override short ReadInt16()
        {
            var bytes = ReadLittleIndian(2);
            return BitConverter.ToInt16(bytes, 0);
        }

        public override int ReadInt32()
        {
            var bytes = ReadLittleIndian(4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public override long ReadInt64()
        {
            var bytes = ReadLittleIndian(8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public override ushort ReadUInt16()
        {
            var bytes = ReadLittleIndian(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public override uint ReadUInt32()
        {
            var bytes = ReadLittleIndian(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public override ulong ReadUInt64()
        {
            var bytes = ReadLittleIndian(8);
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}