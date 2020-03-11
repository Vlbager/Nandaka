namespace Nandaka.MilliGanjubus
{
    internal static class MilliGanjubusBase
    {
        // MinPacketLength = Header + Address + Length + CRC8-head + CRC-8 tail.
        public const int MinPacketLength = 5;
        // 15 байт максимальный блок данных для CRC8 + само CRC
        public const int MaxPacketLength = 15 + 1;
        // Минус Gbyte.
        public const int MaxDataLength = MaxPacketLength - MinPacketLength - 1;
        public const int StartByteOffset = 0;
        public const int AddressOffset = 1;
        public const int SizeOffset = 2;
        public const int HeaderCheckSumOffset = 3;
        public const int DataOffset = 4;
        public const int AddressSize = 1;
        public const int HeaderCheckSumSize = sizeof(byte);
        public const int PacketCheckSumSize = sizeof(byte);

        public const byte GReply = 0xA;
        public const byte GRequest = 0x0;
        public const byte GError = 0xE;

        public const byte FWriteSingle = 0x1;
        public const byte FWriteRange = 0x2;
        public const byte FWriteSeries = 0x3;
        public const byte FReadSingle = 0x4;
        public const byte FReadSeries = 0x6;
        public const byte FReadRange = 0x5;
        public const int MinimumRangeRegisterCount = 3;
        public const byte StartByte = 0xBB;

        /// <summary>
        /// Адрес зарезервированный для широковещательных пакетов
        /// </summary>
        public const byte BroadCastAddress = 0;

        /// <summary>
        /// Адрес зарезервированный для случая, когда на шине одно устройство.
        /// На запрос с этим адресом должно ответить любое устройство.
        /// </summary>
        public const byte DirectCastAddress = 0xFF;
    }
}
