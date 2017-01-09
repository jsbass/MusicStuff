using System;
using System.Collections.Generic;
using System.IO;

namespace MusicStuff.Models.Midi
{
    public class MidiObject
    {
        private readonly List<TrackChunkData> _tracks = new List<TrackChunkData>();
        public readonly HeaderChunkData Header;
        public IReadOnlyCollection<TrackChunkData> Tracks => _tracks.AsReadOnly();

        public MidiObject(BinaryReader reader)
        {
            var first = true;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                try
                {
                    var chunk = new Chunk(reader);
                    if (first && chunk.ChunkType == Chunk.Type.Header)
                    {
                        Header = (HeaderChunkData) chunk.Data;
                    }
                    else
                    {
                        _tracks.Add((TrackChunkData) chunk.Data);
                    }
                }
                catch (Exception e)
                {
                    throw new MidiParseException(e.Message, new MidiObject(Header, _tracks));
                }
            }
        }

        private MidiObject(HeaderChunkData header, List<TrackChunkData> tracks)
        {
            Header = header;
            _tracks = tracks;
        }

        public class MidiParseException : Exception
        {
            public readonly MidiObject Partial;

            public MidiParseException(string message, MidiObject partial) : base(message)
            {
                Partial = partial;
            }
        }
    }
}