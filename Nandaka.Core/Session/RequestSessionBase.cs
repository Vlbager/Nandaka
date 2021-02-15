using System;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    public abstract class RequestSessionBase<TRequestMessage, TSentResult> : ISession
        where TRequestMessage: IMessage
        where TSentResult: ISentResult
    {
        private readonly MessageListener _listener;
        private readonly TimeSpan _requestTimeout;
        private readonly NandakaDevice _device;
        private readonly IErrorMessageHandler _errorMessageHandler;
        
        protected MessageFilterRules FilterRules { get; }
        
        protected abstract ILog Log { get; }

        protected RequestSessionBase(IProtocol protocol, NandakaDevice device, TimeSpan requestTimeout, IErrorMessageHandler errorMessageHandler)
        {
            _requestTimeout = requestTimeout;
            _errorMessageHandler = errorMessageHandler;
            _listener = new MessageListener(protocol);
            _device = device;
            FilterRules = new MessageFilterRules
            {
                message => message.MessageType == MessageType.Response &&
                           message.SlaveDeviceAddress == _device.Address &&
                           message is TRequestMessage or ErrorMessage
            };
        }

        public void ProcessNext()
        {
            TRequestMessage message = GetNextMessage();

            if (message is EmptyMessage)
            {
                Log.AppendMessage($"Nothing to process. Skip {_device.Name}");
                return;
            }
            
            TSentResult sentResult = SendRequest(message);

            if (!sentResult.IsResponseRequired)
                return;

            using MessageSocket socket = _listener.OpenSocket(FilterRules);
            
            if (!socket.WaitMessage(out IMessage? receivedMessage, _requestTimeout))
                throw new DeviceNotRespondException($"Device {_device.Name} not responding");

            if (receivedMessage is ErrorMessage errorMessage)
                ProcessErrorResponse(errorMessage);
            else
                ProcessResponse(receivedMessage!, sentResult);
        }

        private void ProcessErrorResponse(ErrorMessage errorMessage)
        {
            Log.AppendWarning(LogLevel.Low, $"ErrorMessage received: {errorMessage.ToLogLine()}");
            _errorMessageHandler.OnErrorReceived(errorMessage);
        }

        protected abstract TRequestMessage GetNextMessage();
        protected abstract TSentResult SendRequest(TRequestMessage message);
        protected abstract void ProcessResponse(IMessage message, TSentResult sentResult);
    }
}