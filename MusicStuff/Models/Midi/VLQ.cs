using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicStuff.Models.Midi
{
    public class VLQ
    {
        private readonly List<byte> _bytes = new List<byte>();
        public IReadOnlyList<byte> Bytes => _bytes.AsReadOnly();
        public readonly uint Value;

        public VLQ(BinaryReader reader)
        {
            uint result = 0;
            for (var shift = 0; shift < 5 * 7; shift += 7)
            {
                var next = reader.ReadByte();
                _bytes.Add(next);
                result <<= 7;
                result |= ((uint)(next & 0x7F));
                if ((next & 0x80) == 0) break;
            }

            Value = result;
        }

        public VLQ(uint value, IEnumerable<byte> bytes)
        {
            Value = value;
            _bytes = bytes.ToList();
        }
    }

    public static class ExtensionMethods
    {
        public static VLQ ReadVLQ(this BinaryReader reader)
        {
            return new VLQ(reader);
        }
    }
}