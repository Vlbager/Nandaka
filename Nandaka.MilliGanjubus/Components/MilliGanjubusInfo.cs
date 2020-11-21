// ReSharper disable InconsistentNaming
using Nandaka.Core.Protocol;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MilliGanjubusInfo : IProtocolInfo
    {
        private const int _startByteOffset = 0;
        private const int _addressOffset = 1;
        private const int _sizeOffset = 2;
        private const int _headerCheckSumOffset = 3;
        private const int _dataOffset = 4;
        private const int _addressSize = sizeof(byte);
        private const int _headerCheckSumSize = sizeof(byte);
        private const int _packetCheckSumSize = sizeof(byte);
        private const int _minimumRangeRegisterCount = 3;
        private const byte _startByte = 0xBB;
        private const byte _broadCastAddress = 0x00;
        private const byte _directCastAddress = 0xFF;
        private const int _maxPacketLength = 16;
        private const int _maxDataLength = _maxPacketLength - _minPacketLength;
        private const int _minPacketLength = 5;
        private const bool _isSpecificMessageSupported = false;
        private const bool _isHighPriorityMessageSupported = false;

        public const byte GReply = 0xA;
        public const byte GRequest = 0x0;
        public const byte GError = 0xE;
        public const byte FWriteSingle = 0x1;
        public const byte FWriteRange = 0x2;
        public const byte FWriteSeries = 0x3;
        public const byte FReadSingle = 0x4;
        public const byte FReadSeries = 0x6;
        public const byte FReadRange = 0x5;

        public int MinPacketLength => _minPacketLength;

        public int MaxPacketLength => _maxPacketLength;

        public int MaxDataLength => _maxDataLength;

        public int StartByteOffset => _startByteOffset;

        public int AddressOffset => _addressOffset;

        public int SizeOffset => _sizeOffset;

        public int HeaderCheckSumOffset => _headerCheckSumOffset;

        public int DataOffset => _dataOffset;

        public int AddressSize => _addressSize;
        public int DataHeaderSize => 1;

        public int HeaderCheckSumSize => _headerCheckSumSize;

        public int PacketCheckSumSize => _packetCheckSumSize;
        
        //todo: check with doc
        public int MinRegisterAddress => 1;
        public int MaxRegisterAddress => 254;

        public int MinimumRangeRegisterCount => _minimumRangeRegisterCount;
        public bool IsSpecificMessageSupported => _isSpecificMessageSupported;
        public bool IsHighPriorityMessageSupported => _isHighPriorityMessageSupported;

        public byte StartByte => _startByte;

        public byte BroadCastAddress => _broadCastAddress;

        public byte DirectCastAddress => _directCastAddress;
    }
}
