using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace MusicStuff.Models.Midi
{
    public class Chunk
    {
        [JsonProperty("trackType")]
        public Type ChunkType { get; set; }

        [JsonProperty("length")]
        public uint Length { get; set; }

        [JsonProperty("data")]
        public ChunkData Data { get; set; }

        public enum Type : byte
        {
            Header = 0,
            Track = 1
        }

        public Chunk(BinaryReader reader)
        {
            var typeString = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Length = reader.ReadUInt32();
            switch (typeString)
            {
                case "MThd":
                    ChunkType = Type.Header;
                    Data = new HeaderChunkData(reader);
                    break;
                case "MTrk":
                    ChunkType = Type.Track;
                    Data = new TrackChunkData(reader);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{typeString} is not a valid Chunk Type");
            }
        }

        public Chunk(Type type, uint length, ChunkData data)
        {
            ChunkType = type;
            Length = length;
            Data = data;
        }
    }

    public class ChunkData
    {
        public byte[] Raw { get; protected set; }
    }

    public class TrackChunkData : ChunkData
    {
        private readonly List<EventData> _events = new List<EventData>();
        
        [JsonProperty("events")]
        public IReadOnlyList<EventData> Events => _events.AsReadOnly();
        
        public TrackChunkData(BinaryReader reader)
        {
            do
            {
                var next = new EventData(reader);
                _events.Add(next);
                if (next.Event.GetType() == typeof(MetaEvent))
                {
                    if (((MetaEvent)next.Event).Type == MetaEvent.MetaEventType.EndOfTrack)
                    {
                        break;
                    }
                }
            } while (true);
        }
    }

    public class HeaderChunkData : ChunkData
    {
        [JsonProperty("fileFormat")]
        public FileType Format { get; set; }
        [JsonProperty("trackCount")]
        public ushort TrackCount { get; set; }
        [JsonProperty("division")]
        public Division Division { get; set; }

        public HeaderChunkData(BinaryReader reader)
        {
            Format = (FileType) reader.ReadUInt16();
            TrackCount = reader.ReadUInt16();
            Division = new Division(reader.ReadUInt16());
        }

        public enum FileType : ushort
        {
            SingleTrack   = 0,
            MultipleTrack = 1,
            MultipleSong  = 2
        }
    }

    public class Division
    {
        [JsonProperty("ticksPerType")]
        public ushort TicksPerType { get; private set; }
        [JsonProperty("divisionType")]
        public Type DivisionType { get; private set; }

        public Division(ushort value)
        {
            //Check bit 15 (leftmost) to determine type
            //0XXX XXXX XXXX XXXX -> Ticks / QuarterNote
            //1XXX XXXX XXXX XXXX -> Ticks / Second
            DivisionType = (value & 0x8000) == 0 ? Type.QuarterNote : Type.Second;

            //The rest of the bits designates the number
            TicksPerType = (ushort) (value & ~0x8000);
        }

        public enum Type : byte
        {
            QuarterNote = 0,
            Second = 1
        }
    } 
}