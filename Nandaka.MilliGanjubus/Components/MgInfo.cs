// ReSharper disable InconsistentNaming

using System;

using Nandaka.Core.Protocol;

namespace Nandaka.MilliGanjubus.Components
{
    public sealed class MgInfo : IProtocolInfo
    {
        public static readonly MgInfo Instance = new MgInfo();
        
        private MgInfo() { }
        
        internal const byte GReply = 0xA;
        internal const byte GRequest = 0x0;
        internal const byte GError = 0xE;
        internal const byte FWriteSingle = 0x1;
        internal const byte FWriteRange = 0x2;
        internal const byte FWriteSeries = 0x3;
        internal const byte FReadSingle = 0x4;
        internal const byte FReadSeries = 0x6;
        internal const byte FReadRange = 0x5;

        internal const int MaxRegisterValueSize = 9;

        internal static readonly Range ReadRequestRegistersAddressRange = new(0, 127);
        internal static readonly Range WriteRequestRegistersAddressRange = new(128, 255);

        public int MinPacketLength => 5;

        public int MaxPacketLength => 16;

        // Does not include G-byte;
        public int MaxDataLength => 10;

        public int StartByteOffset => 0;

        public int AddressOffset => 1;

        public int SizeOffset => 2;

        public int HeaderCheckSumOffset => 3;

        public int DataOffset => 4;

        public int AddressSize => sizeof(byte);

        public int PacketCheckSumSize => sizeof(byte);
        
        public int MinRegisterAddress => 0;
        public int MaxRegisterAddress => 255;

        public int MinimumRangeRegisterCount => 3;
        public bool IsSpecificMessageSupported => false;
        public bool IsHighPriorityMessageSupported => false;

        public byte StartByte => 0xBB;

        public byte BroadCastAddress => 0x00;

        public byte DirectCastAddress => 0xFF;
    }
}
