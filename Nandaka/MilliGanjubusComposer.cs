using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka
{
    public class MilliGanjubusComposer : IComposer<byte[]>
    {
        // todo: how to make protocolInfo fields changeable? 
        // Interface with getter-methods?
        readonly MilliGanjubusProtocolInfo _protocolInfo;

        public MilliGanjubusComposer(MilliGanjubusProtocolInfo protocolInfo)
        {
            _protocolInfo = protocolInfo;
        }

        public byte[] Compose(IProtocolMessage message)
        {
            var data = GetDataBytes(message);
            var packet = new byte[MilliGanjubusProtocolInfo.MinPacketLength + data.Length];
            if (packet.Length > MilliGanjubusProtocolInfo.MaxPacketLength)
            {
                // todo: create a custom exception.
                throw new ArgumentOutOfRangeException();
            }
            // todo: resolve problem with MilliGanjubusProcolInfoEveryFieldNamingStyle.
            packet[MilliGanjubusProtocolInfo.StartByteOffset] = MilliGanjubusProtocolInfo.StartByte;
            // todo: Add device address to IProtocolMessage interface?
            packet[MilliGanjubusProtocolInfo.AddressOffset] = 0;
            packet[MilliGanjubusProtocolInfo.SizeOffset] = (byte)packet.Length;
            packet[MilliGanjubusProtocolInfo.HeaderCheckSumOffset] = 
                Checksum.CRC8(packet.Take(MilliGanjubusProtocolInfo.HeaderCheckSumOffset).ToArray());
            Array.Copy(data, 0, packet, MilliGanjubusProtocolInfo.DataOffset, data.Length);
            packet[packet.Length - 1] = 
                Checksum.CRC8(packet.Take(packet.Length - MilliGanjubusProtocolInfo.PacketCheckSumSize).ToArray());
            return packet;
        }

        private byte[] GetDataBytes(IProtocolMessage message)
        {
            // Here I have to decide whether it's a series or a group. 
            // Session does the same to calculate packet length.
            throw new NotImplementedException();
        }
    }
}
