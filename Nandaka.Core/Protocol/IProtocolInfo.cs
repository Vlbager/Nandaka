namespace Nandaka.Core.Protocol
{
    public interface IProtocolInfo
    {
        // note: Окончательный список таких значений уточню, когда напишу парсер для второго МГ.
        int MinPacketLength { get; }
        int MaxPacketLength { get; }
        int MaxDataLength { get; }
        int StartByteOffset { get; }
        int AddressOffset { get; }
        int SizeOffset { get; }
        int HeaderCheckSumOffset { get; }
        int DataOffset { get; }
        int AddressSize { get; }
        int DataHeaderSize { get; }
        int HeaderCheckSumSize { get; }
        int PacketCheckSumSize { get; }
        
        int MinRegisterAddress { get; }
        int MaxRegisterAddress { get; }

        int MinimumRangeRegisterCount { get; }

        bool IsSpecificMessageSupported { get; }
        bool IsHighPriorityMessageSupported { get; }
    }
}
