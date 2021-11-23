using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Util;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterSingleThreadHolder : IMasterSessionsHolder
    {
        private readonly MasterDeviceSessionMap _deviceSessionsMap;
        private readonly ISessionHandler _highPrioritySessionHandler;
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly UpdateCandidatesHolder _candidatesHolder;
        private readonly Thread _thread;
        private readonly DisposableList _disposable;
        private readonly ILogger _logger;
        
        private bool _isStopped;

        public MasterSingleThreadHolder(IDeviceUpdatePolicy updatePolicy, UpdateCandidatesHolder candidatesHolder, IProtocol protocol,
                                        MasterDeviceSessionMap sessionMap, ILogger logger)
        {
            _disposable = new DisposableList();
            _updatePolicy = updatePolicy;
            _candidatesHolder = candidatesHolder;
            _logger = logger;
            _deviceSessionsMap = _disposable.Add(sessionMap);
            _highPrioritySessionHandler = InitHighPrioritySession(protocol);
            _thread = new Thread(Routine) { IsBackground = true };
        }

        private static ISessionHandler InitHighPrioritySession(IProtocol protocol)
        {
            if (!protocol.Info.IsHighPriorityMessageSupported)
                return new NullSessionHandler();

            throw new NotImplementedException("High priority message session");
        }

        public void StartRoutine() => _thread.Start();

        private void Routine()
        {
            _logger.LogInformation("Starting single Master thread, devices: {0}", _candidatesHolder);
            try
            {
                RoutineInternal();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Unexpected error occured");
            }
            
            _logger.LogWarning("Master thread has been stopped");
        }

        private void RoutineInternal()
        {
            while (true)
            {
                foreach (ForeignDevice nextDevice in _candidatesHolder.GetDevicesForProcessing(_logger))
                {
                    if (_isStopped)
                        return;

                    _highPrioritySessionHandler.ProcessNext();

                    ProcessDevice(nextDevice);   
                }
                
                Thread.Sleep(_updatePolicy.UpdateTimeout);
            }
        }

        private void ProcessDevice(ForeignDevice device)
        {
            DeviceSessionCollection sessionCollection = _deviceSessionsMap.GetDeviceSessions(device);

            _logger.LogDebug("Set current device: {0}", device.Name);
            
            IReadOnlyCollection<ISessionHandler> sessions = sessionCollection.SessionHandlers;

            try
            {
                foreach (ISessionHandler session in sessions)
                    session.ProcessNext();
                
                _updatePolicy.OnMessageReceived(device, _logger);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                _logger.LogError(deviceNotRespondException.Message);
                _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding, _logger);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                _logger.LogError(invalidMessageException.Message);
                _updatePolicy.OnErrorOccured(device, DeviceError.WrongPacketData, _logger);
            }
        }

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}
