using System;
using System.IO;
using static ATL.AudioData.AudioDataManager;
using static ATL.ChannelsArrangements;

namespace ATL.AudioData.IO
{
    /// <summary>
    /// Class for OptimFROG files manipulation (extensions : .OFR, .OFS)
    /// </summary>
	class OptimFrog : IAudioDataIO
    {
        private const string OFR_SIGNATURE = "OFR ";

#pragma warning disable S1144 // Unused private types or members should be removed
        private static readonly string[] OFR_COMPRESSION = new string[10]
        {
            "fast", "normal", "high", "extra",
            "best", "ultra", "insane", "highnew", "extranew", "bestnew"
        };

        private static readonly sbyte[] OFR_BITS = new sbyte[11]
        {
            8, 8, 16, 16, 24, 24, 32, 32,
            -32, -32, -32 //negative value corresponds to floating point type.
        };
#pragma warning restore S1144 // Unused private types or members should be removed


        // Real structure of OptimFROG header
        public class TOfrHeader
        {
            public char[] ID = new char[4];                      // Always 'OFR '
            public uint Size;
            public uint Length;
            public ushort HiLength;
            public byte SampleType;
            public byte ChannelMode;
            public int SampleRate;
            public ushort EncoderID;
            public byte CompressionID;
            public void Reset()
            {
                Array.Clear(ID, 0, ID.Length);
                Size = 0;
                Length = 0;
                HiLength = 0;
                SampleType = 0;
                ChannelMode = 0;
                SampleRate = 0;
                EncoderID = 0;
                CompressionID = 0;
            }
        }


        private readonly TOfrHeader header = new TOfrHeader();

        private double bitrate;
        private double duration;

        private SizeInfo sizeInfo;
        private readonly string filePath;


        // ---------- INFORMATIVE INTERFACE IMPLEMENTATIONS & MANDATORY OVERRIDES

        public int SampleRate // Sample rate (Hz)
        {
            get { return getSampleRate(); }
        }
        public bool IsVBR
        {
            get { return false; }
        }
        public Format AudioFormat
        {
            get;
        }
        public int CodecFamily
        {
            get { return AudioDataIOFactory.CF_LOSSLESS; }
        }
        public string FileName
        {
            get { return filePath; }
        }
        public double BitRate
        {
            get { return bitrate; }
        }
        public double Duration
        {
            get { return duration; }
        }
        public ChannelsArrangement ChannelsArrangement
        {
            get { return GuessFromChannelNumber(header.ChannelMode + 1); }
        }
        public bool IsMetaSupported(MetaDataIOFactory.TagType metaDataType)
        {
            return (metaDataType == MetaDataIOFactory.TagType.APE) || (metaDataType == MetaDataIOFactory.TagType.ID3V1) || (metaDataType == MetaDataIOFactory.TagType.ID3V2);
        }
        public long AudioDataOffset { get; set; }
        public long AudioDataSize { get; set; }


        // ---------- CONSTRUCTORS & INITIALIZERS

        private void resetData()
        {
            duration = 0;
            bitrate = 0;
            AudioDataOffset = -1;
            AudioDataSize = 0;

            header.Reset();
        }

        public OptimFrog(string filePath, Format format)
        {
            this.filePath = filePath;
            AudioFormat = format;
            resetData();
        }


        // ---------- SUPPORT METHODS

        // Get number of samples
        private long getSamples()
        {
            return (((header.Length >> header.ChannelMode) * 0x00000001) +
                ((header.HiLength >> header.ChannelMode) * 0x00010000));
        }

        // Get song duration
        private double getDuration()
        {
            if (header.SampleRate > 0)
                return (double)getSamples() * 1000.0 / header.SampleRate;
            else
                return 0;
        }

        private int getSampleRate()
        {
            return header.SampleRate;
        }

        private double getBitrate()
        {
            return ((sizeInfo.FileSize - header.Size - sizeInfo.TotalTagSize) * 8 / Duration);
        }

        public bool Read(BinaryReader source, SizeInfo sizeInfo, MetaDataIO.ReadTagParams readTagParams)
        {
            bool result = false;
            this.sizeInfo = sizeInfo;
            resetData();

            // Read header data
            source.BaseStream.Seek(sizeInfo.ID3v2Size, SeekOrigin.Begin);

            long initialPos = source.BaseStream.Position;
            header.ID = source.ReadChars(4);
            header.Size = source.ReadUInt32();
            header.Length = source.ReadUInt32();
            header.HiLength = source.ReadUInt16();
            header.SampleType = source.ReadByte();
            header.ChannelMode = source.ReadByte();
            header.SampleRate = source.ReadInt32();
            header.EncoderID = source.ReadUInt16();
            header.CompressionID = source.ReadByte();

            if (StreamUtils.StringEqualsArr(OFR_SIGNATURE, header.ID))
            {
                result = true;
                AudioDataOffset = initialPos;
                AudioDataSize = sizeInfo.FileSize - sizeInfo.APESize - sizeInfo.ID3v1Size - AudioDataOffset;

                duration = getDuration();
                bitrate = getBitrate();
            }

            return result;
        }

    }
}