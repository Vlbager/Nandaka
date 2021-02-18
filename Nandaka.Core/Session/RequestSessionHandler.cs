using System;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal sealed class RequestSessionHandler<TRequestMessage, TSentResult> : ISessionHandler
        where TRequestMessage: IMessage
        where TSentResult: ISentResult
    {
        private readonly ILog _log;
        private readonly MessageListener _listener;
        private readonly MessageFilterRules _filterRules;
        private readonly TimeSpan _requestTimeout;
        private readonly NandakaDevice _device;
        private readonly IErrorMessageHandler _errorMessageHandler;
        private readonly IRequestSession<TRequestMessage, TSentResult> _requestSession;

        public RequestSessionHandler(IRequestSession<TRequestMessage, TSentResult> requestSession, IProtocol protocol, NandakaDevice device, 
                                     TimeSpan requestTimeout, IErrorMessageHandler errorMessageHandler)
        {
            _requestTimeout = requestTimeout;
            _errorMessageHandler = errorMessageHandler;
            _requestSession = requestSession;
            _listener = new MessageListener(protocol);
            _device = device;
            _log = new PrefixLog(device.Name);
            _filterRules = new MessageFilterRules
            {
                message => message.MessageType == MessageType.Response &&
                           message.SlaveDeviceAddress == _device.Address &&
                           message is TRequestMessage or ErrorMessage
            };
        }

        public void ProcessNext()
        {
            TRequestMessage message = _requestSession.GetNextMessage();

            if (message is EmptyMessage)
            {
                _log.AppendMessage($"Nothing to process. Skip {_device.Name}");
                return;
            }
            
            TSentResult sentResult = _requestSession.SendRequest(message);

            if (!sentResult.IsResponseRequired)
                return;

            using MessageSocket socket = _listener.OpenSocket(_filterRules);
            
            if (!socket.WaitMessage(out IMessage? receivedMessage, _requestTimeout))
                throw new DeviceNotRespondException($"Device {_device.Name} not responding");

            if (receivedMessage is ErrorMessage errorMessage)
                ProcessErrorResponse(errorMessage);
            else
                _requestSession.ProcessResponse(receivedMessage!, sentResult);
        }

        private void ProcessErrorResponse(ErrorMessage errorMessage)
        {
            _log.AppendWarning(LogLevel.Low, $"ErrorMessage received: {errorMessage.ToLogLine()}");
            _errorMessageHandler.OnErrorReceived(errorMessage);
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}