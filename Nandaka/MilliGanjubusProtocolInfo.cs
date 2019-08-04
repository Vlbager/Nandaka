using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public class MilliGanjubusProtocolInfo : GanjubusBaseProtocolInfo
    {
        // MinPacketLength = Header + Address + Length + CRC8-head + CRC-8 tail.
        public readonly int MinPacketLength = 5;
        // 15 байт максимальный блок данных для CRC8 + само CRC
        public readonly int MaxPacketLength = 15 + 1;
        // Минус Gbyte.
        public readonly int MaxDataLength = 10;
        public readonly int StartByteOffset = 0;
        public readonly int AddressOffset = 1;
        public readonly int SizeOffset = 2;
        public readonly int HeaderCheckSumOffset = 3;
        public readonly int DataOffset = 4;
        public readonly int HeaderCheckSumSize = sizeof(byte);
        public readonly int PacketCheckSumSize = sizeof(byte);

        public readonly byte GAcknowledge = 0xA;
        public readonly byte GRequest = 0x0;
        public readonly byte GError = 0xE;

        public readonly byte FWriteSingle = 0x1;
        public readonly byte FWriteRange = 0x2;
        public readonly byte FWriteSeries = 0x3;
        public readonly byte FReadSingle = 0x4;
        public readonly byte FReadSeries = 0x6;
        public readonly byte FReadRange = 0x5;
        public readonly int MinimumRangeRegisterCount = 3;
        public readonly byte StartByte = 0xBB;

        public int GetMaxDataLength()
        {
            return MaxDataLength;
        }
    }
}
