using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MusicStuff.Models.Midi
{
    public class EventData
    {
        private static Event lastEvent;

        private VLQ _deltaTime;

        public uint DeltaTime => _deltaTime.Value;
        public IReadOnlyCollection<byte> DeltaTimeBytes => _deltaTime.Bytes;
        public readonly Event Event;

        public EventData(BinaryReader reader)
        {
            _deltaTime = reader.ReadVLQ();

            var type = reader.ReadByte();
            if (type == 0xFF)
            {
                Event = new MetaEvent(reader);
            }
            else if (Enum.IsDefined(typeof(SysexEvent.SysexEventType),type))
            {
                reader.BaseStream.Position -= 1;
                Event = new SysexEvent(reader);
            }
            else if (Enum.IsDefined(typeof(MidiEvent.MidiEventType), (byte) (type & 0xF0)))
            {
                reader.BaseStream.Position -= 1;
                Event = new MidiEvent(reader);
            }
            else if (type < 0x80)
            {
                if (lastEvent != null && lastEvent.GetType() == typeof(MidiEvent))
                {
                    reader.BaseStream.Position -= 1;
                    Event = MidiEvent.Extend((MidiEvent) lastEvent, reader);
                }
                else
                {
                    throw new ArgumentException($"{type} is not a valid Event Type and there was no previous running MidiEvent. Last event was of type {lastEvent?.GetType()}");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($"{type} is not a valid Event Type");
            }

            lastEvent = Event;
        }

        
    }

    public class Event
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Type EventType { get; protected set; }
        [JsonConverter(typeof(ByteConverter))]
        public byte EventTypeByte { get; protected set; }
        public uint Length { get; set; }

        [JsonConverter(typeof(ByteConverter))]
        public byte[] DataBytes { get; protected set; }

        [JsonConverter(typeof(ByteConverter))]
        public byte[] LengthBytes { get; protected set; }

        public enum Type
        {
            Meta,
            Sysex,
            Midi
        }
    }

    public class ByteConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]) || objectType == typeof(byte);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var bytes = new List<byte>();
            var valueString = reader.ReadAsString();
            var stringArr = valueString.Split(' ');
            foreach (var hex in stringArr)
            {
                bytes.Add(Convert.ToByte(hex));
            }
            return bytes.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            byte[] arr;
            if (value is byte)
            {
                arr = new [] {(byte) value};
            }
            else
            {
                arr = (byte[]) value;
            }
            writer.WriteValue(BitConverter.ToString(arr).Replace("-"," "));
        }
    }

    public class MetaEvent : Event
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly MetaEventType Type;

        [JsonConverter(typeof(ByteConverter))]
        public byte SubTypeByte => (byte) Type;

        public MetaEvent(BinaryReader reader)
        {
            EventType = Event.Type.Meta;
            EventTypeByte = reader.ReadByte();
            Type = (MetaEventType) EventTypeByte;
            var length = new VLQ(reader);
            Length = length.Value;
            LengthBytes = length.Bytes.ToArray();
            DataBytes = reader.ReadBytes(Convert.ToInt32(Length));
        }

        public enum MetaEventType : byte
        {
            TrackNumber            = 0x00,
            TextEvent              = 0x01,
            CopyrightNotice        = 0x02,
            TrackName              = 0x03,
            InstrumentName         = 0x04,
            Lyric                  = 0x05,
            Marker                 = 0x06,
            CuePoint               = 0x07,
            MidiChannelPrefix      = 0x20,
            EndOfTrack             = 0x2F,
            SetTempo               = 0x51,
            SmtpeOffset            = 0x54,
            TimeSignature          = 0x58,
            KeySignature           = 0x59,
            TrackSpecificMetaEvent = 0x7F
        }
    }

    public class SysexEvent : Event
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public readonly SysexEventType Type;

        public SysexEvent(BinaryReader reader)
        {
            EventType = Event.Type.Sysex;
            LengthBytes = new byte[]{};
            Type = (SysexEventType) reader.ReadByte();
            EventTypeByte = (byte) Type;
            switch (Type)
            {
                case SysexEventType.SysExStart:
                    var dataBytes = new List<byte>();
                    byte next;
                    do
                    {
                        next = reader.ReadByte();
                        dataBytes.Add(next);
                    } while (next != 0xF7);
                    DataBytes = dataBytes.ToArray();
                    break;
                case SysexEventType.MTC:
                    DataBytes = reader.ReadBytes(1);
                    break;
                case SysexEventType.SongPointer:
                    DataBytes = reader.ReadBytes(2);
                    break;
                case SysexEventType.SongSelect:
                    reader.ReadBytes(1);
                    break;
                case SysexEventType.SysExEnd:
                    break;
                case SysexEventType.TuneRequest:
                    break;
                case SysexEventType.TimingClock:
                    break;
                case SysexEventType.Continue:
                    break;
                case SysexEventType.Stop:
                    break;
                case SysexEventType.ActiveSensing:
                    break;
                case SysexEventType.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{Type} is not a valid Sysex Event Type");
            }

        }

        public enum SysexEventType : byte
        {
            SysExStart = 0xF0,
            SysExEnd  = 0xF7,
            MTC = 0xF1,
            SongPointer = 0xF2,
            SongSelect = 0xF3,
            TuneRequest = 0xF6,
            TimingClock = 0xF8,
            Start = 0xF8,
            Continue = 0xFB,
            Stop = 0xFC,
            ActiveSensing = 0xFe,
            Reset = 0xFF
        }
    }

    public class MidiEvent : Event
    {
        public readonly byte Channel;

        [JsonConverter(typeof(StringEnumConverter))]
        public readonly MidiEventType Type;

        public MidiEvent(BinaryReader reader)
        {
            EventType = Event.Type.Midi;
            EventTypeByte = reader.ReadByte();
            Channel = (byte) (EventTypeByte & 0x0F);
            Type = (MidiEventType) (byte) (EventTypeByte & 0xF0);

            DataBytes = reader.ReadBytes(2);
        }

        public enum MidiEventType : byte
        {
            NoteOff              = 0x80,
            NoteOn               = 0x90,
            PolyphonicAfterTouch = 0xA0,
            ControlChange        = 0xB0,
            ProgramChange        = 0xC0,
            AfterTouchPressure   = 0xD0,
            PitchWheelRange      = 0xE0
        }

        private MidiEvent(byte channel, MidiEventType type, byte[] data)
        {
            Channel = channel;
            Type = type;
            DataBytes = data;
        }

        public static MidiEvent Extend(MidiEvent old, BinaryReader reader)
        {
            return new MidiEvent(old.Channel,old.Type, reader.ReadBytes(2));
        }
    }
}