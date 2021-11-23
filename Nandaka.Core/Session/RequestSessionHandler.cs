using System;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;

namespace Nandaka.Core.Session
{
    internal sealed class RequestSessionHandler<TRequestMessage, TSentResult> : ISessionHandler
        where TRequestMessage: IMessage
        where TSentResult: ISentResult
    {
        private readonly ILogger _logger;
        private readonly MessageListener _listener;
        private readonly MessageFilterRules _filterRules;
        private readonly TimeSpan _requestTimeout;
        private readonly NandakaDevice _device;
        private readonly IErrorMessageHandler _errorMessageHandler;
        private readonly IRequestSession<TRequestMessage, TSentResult> _requestSession;

        public RequestSessionHandler(IRequestSession<TRequestMessage, TSentResult> requestSession, IProtocol protocol, NandakaDevice device, 
                                     TimeSpan requestTimeout, IErrorMessageHandler errorMessageHandler, ILogger logger)
        {
            _requestTimeout = requestTimeout;
            _errorMessageHandler = errorMessageHandler;
            _requestSession = requestSession;
            _listener = new MessageListener(protocol);
            _device = device;
            _logger = logger;
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
                _logger.LogInformation($"Nothing to process. Skip {_device}");
                return;
            }
            
            using MessageSocket socket = _listener.OpenSocket(_filterRules);
            
            TSentResult sentResult = _requestSession.SendRequest(message);

            if (!sentResult.IsResponseRequired)
                return;

            if (!socket.WaitMessage(out IMessage? receivedMessage, _requestTimeout))
                throw new DeviceNotRespondException($"Device {_device.Name} not responding");

            if (receivedMessage is ErrorMessage errorMessage)
                ProcessErrorResponse(errorMessage);
            else
                _requestSession.ProcessResponse(receivedMessage!, sentResult);
        }

        private void ProcessErrorResponse(ErrorMessage errorMessage)
        {
            _logger.LogWarning($"ErrorMessage received: {errorMessage}");
            _errorMessageHandler.OnErrorReceived(errorMessage, _logger);
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}