namespace Nandaka.Core.Protocol
{
    public interface IProtocolInfo
    {
        int MinPacketLength { get; }
        int MaxPacketLength { get; }
        int MaxDataLength { get; }
        int AddressSize { get; }
        int HeaderCheckSumSize { get; }
        int PacketCheckSumSize { get; }
        int MinRegisterAddress { get; }
        int MaxRegisterAddress { get; }
        int MinimumRangeRegisterCount { get; }
        bool IsSpecificMessageSupported { get; }
        bool IsHighPriorityMessageSupported { get; }
    }
}
