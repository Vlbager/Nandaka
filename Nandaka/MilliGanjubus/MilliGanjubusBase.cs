using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public abstract class MilliGanjubusBase
    {
        // MinPacketLength = Header + Address + Length + CRC8-head + CRC-8 tail.
        protected const int MinPacketLength = 5;
        // 15 байт максимальный блок данных для CRC8 + само CRC
        protected const int MaxPacketLength = 15 + 1;
        // Минус Gbyte.
        protected const int MaxDataLength = MaxPacketLength - MinPacketLength - 1;
        protected const int StartByteOffset = 0;
        protected const int AddressOffset = 1;
        protected const int SizeOffset = 2;
        protected const int HeaderCheckSumOffset = 3;
        protected const int DataOffset = 4;
        protected const int HeaderCheckSumSize = sizeof(byte);
        protected const int PacketCheckSumSize = sizeof(byte);

        protected const byte GReply = 0xA;
        protected const byte GRequest = 0x0;
        protected const byte GError = 0xE;

        protected const byte FWriteSingle = 0x1;
        protected const byte FWriteRange = 0x2;
        protected const byte FWriteSeries = 0x3;
        protected const byte FReadSingle = 0x4;
        protected const byte FReadSeries = 0x6;
        protected const byte FReadRange = 0x5;
        protected const int MinimumRangeRegisterCount = 3;
        protected const byte StartByte = 0xBB;

        /// <summary>
        /// Адрес зарезервированный для широковещательных пакетов
        /// </summary>
        protected const byte BroadCastAddress = 0;

        /// <summary>
        /// Адрес зарезервированный для случая, когда на шине одно устройство.
        /// На запрос с этим адресом должно ответить любое устройство.
        /// </summary>
        protected const byte DirectCastAddress = 0xFF;
    }
}
