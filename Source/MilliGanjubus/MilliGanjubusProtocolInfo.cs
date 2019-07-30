using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public class MilliGanjubusProtocolInfo
    {
        // MinPacketLength = Header + Address + Length + CRC8-head + CRC-8 tail.
        private const int MinPacketLength = 5;
        // 15 байт максимальный блок данных для CRC8 + само CRC
        private const int MaxPacketLength = 15 + 1;
        // Минус Gbyte.
        private const int MaxDataLength = MaxPacketLength - MinPacketLength - 1;
        private const int StartByteOffset = 0;
        private const int AddressOffset = 1;
        private const int SizeOffset = 2;
        private const int HeaderCheckSumOffset = 3;
        private const int DataOffset = 4;
        private const int HeaderCheckSumSize = sizeof(byte);
        private const int PacketCheckSumSize = sizeof(byte);

        private const byte GAcknowledge = 0xA;
        private const byte GRequest = 0x0;
        private const byte GError = 0xE;

        private const byte FWriteSingle = 0x1;
        private const byte FWriteRange = 0x2;
        private const byte FWriteSeries = 0x3;
        private const byte FReadSingle = 0x4;
        private const byte FReadSeries = 0x6;
        private const byte FReadRange = 0x5;
        private const int MinimumRangeRegisterCount = 3;
        private const byte StartByte = 0xBB;
    }
}
