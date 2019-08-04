using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public class Checksum
    {
        public static byte CRC8(byte[] pcBlock)
        {
            return CRC8(pcBlock, pcBlock.Length);
        }

        private static byte CRC8(byte[] pcBlock, int len)
        {
            byte crc = 0xFF;

            for (int k = 0; k < len; k++)
            {
                crc ^= pcBlock[k];

                for (int i = 0; i < 8; i++)

                    crc = (byte)(((crc & 0x80) != 0) ? (crc << 1) ^ 0x31 : crc << 1);
            }
            return crc;
        }
    }
}
