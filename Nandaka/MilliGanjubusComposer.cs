using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka
{
    public class MilliGanjubusComposer : MilliGanjubusProtocolInfo, IComposer<byte[]>
    {
        // todo: how to make protocolInfo fields changeable? 
        // Interface with getter-methods?

        public byte[] Compose(ITransferData message)
        {
            var data = GetDataBytes(message);
            var packet = new byte[MinPacketLength + data.Length];
            if (packet.Length > MaxPacketLength)
            {
                // todo: create a custom exception.
                throw new ArgumentOutOfRangeException();
            }
            packet[StartByteOffset] = StartByte;
            // todo: Add device address to ITransferData interface or to IProtocol.SendMessage() paremeters?
            packet[AddressOffset] = 0;
            packet[SizeOffset] = (byte)packet.Length;
            packet[HeaderCheckSumOffset] =
                CheckSum.CRC8(packet.Take(HeaderCheckSumOffset).ToArray());
            Array.Copy(data, 0, packet, DataOffset, data.Length);
            packet[packet.Length - 1] =
                CheckSum.CRC8(packet.Take(packet.Length - PacketCheckSumSize).ToArray());
            return packet;
        }

        private byte[] GetDataBytes(ITransferData message)
        {
            // Here I have to decide whether it's a series or a group. 
            // Session does the same to calculate packet length.
            throw new NotImplementedException();
        }
    }
}
