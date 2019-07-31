using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public abstract class GanjubusBaseProtocolInfo : IProtocolInfo
    {
        // MinPacketLength = Header + Address + Length + CRC8-head + CRC-8 tail.
        public readonly int MinPacketLength;
        // 15 байт максимальный блок данных для CRC8 + само CRC
        public readonly int MaxPacketLength;
        // Минус Gbyte.
        public readonly int MaxDataLength;
        public readonly int StartByteOffset;
        public readonly int AddressOffset;
        public readonly int SizeOffset;
        public readonly int HeaderCheckSumOffset;
        public readonly int DataOffset;
        public readonly int HeaderCheckSumSize;
        public readonly int PacketCheckSumSize;

        public readonly byte GAcknowledge;
        public readonly byte GRequest;
        public readonly byte GError;

        public readonly byte FWriteSingle;
        public readonly byte FWriteRange;
        public readonly byte FWriteSeries;
        public readonly byte FReadSingle;
        public readonly byte FReadSeries;
        public readonly byte FReadRange;
        public readonly int MinimumRangeRegisterCount;
        public readonly byte StartByte;

        public int GetMaxDataLength()
        {
            return MaxDataLength;
        }
    }
}
