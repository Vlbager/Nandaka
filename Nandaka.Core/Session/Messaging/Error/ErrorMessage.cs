using System;

namespace Nandaka.Core.Session
{
    public sealed class ErrorMessage : IMessage
    {
        public int SlaveDeviceAddress { get; }
        public MessageType MessageType { get; }
        public ErrorType ErrorType { get; }
        public ProtocolSpecifiedErrorMessage? ProtocolSpecifiedErrorMessage { get; }

        private ErrorMessage(int slaveDeviceAddress, MessageType messageType, ErrorType errorType, ProtocolSpecifiedErrorMessage? protocolSpecifiedErrorMessage)
        {
            SlaveDeviceAddress = slaveDeviceAddress;
            MessageType = messageType;
            ErrorType = errorType;
            ProtocolSpecifiedErrorMessage = protocolSpecifiedErrorMessage;
        }

        public static ErrorMessage CreateCommon(int slaveDeviceAddress, MessageType messageType, ErrorType errorType)
        {
            return new ErrorMessage(slaveDeviceAddress, messageType, errorType, null);
        }
        
        public static ErrorMessage CreateCommon(int slaveDeviceAddress, MessageType messageType, ErrorType errorType, 
                                                ProtocolSpecifiedErrorMessage protocolSpecifiedErrorMessage)
        {
            return new ErrorMessage(slaveDeviceAddress, messageType, errorType, protocolSpecifiedErrorMessage);
        }

        public static ErrorMessage CreateFromProtocol(int slaveDeviceAddress, MessageType messageType,
                                                      ProtocolSpecifiedErrorMessage protocolSpecifiedErrorMessage)
        {
            return new ErrorMessage(slaveDeviceAddress, messageType, ErrorType.InternalProtocolError, protocolSpecifiedErrorMessage);
        }

        public string ToLogLine()
        {
            string protocolSpecifiedInfo = ProtocolSpecifiedErrorMessage != null
                                           ? $"{Environment.NewLine}{ProtocolSpecifiedErrorMessage.ToLogLine()}"
                                           : String.Empty;
            
            return $"Error from '{SlaveDeviceAddress.ToString()}' device: errorType: {ErrorType}" + protocolSpecifiedInfo;
        }
    }
}
