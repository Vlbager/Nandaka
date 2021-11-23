using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.Core.Util;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterAsyncSessionsHolder : IMasterSessionsHolder
    {
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly Thread[] _threads;
        private readonly DisposableList _disposable;
        private readonly ILogger _logger;
        
        private bool _isStopped;

        public MasterAsyncSessionsHolder(IDeviceUpdatePolicy updatePolicy, IReadOnlyCollection<DeviceSessionCollection> sessions, ILogger logger)
        {
            _updatePolicy = updatePolicy;
            _logger = logger;
            _disposable = new DisposableList();
            _disposable.AddRange(sessions);
            _threads = sessions.Select(deviceSessions => new Thread(() => Routine(deviceSessions)))
                               .ToArray();
        }

        public void StartRoutine() => _threads.ForEach(thread => thread.Start());

        private void Routine(DeviceSessionCollection deviceSessions)
        {
            try
            {
                ForeignDevice device = deviceSessions.Device;

                IReadOnlyCollection<ISessionHandler> sessions = deviceSessions.SessionHandlers;
                
                RoutineInternal(device, sessions);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error occured");
            }
            
            _logger.LogCritical($"Master thread '{Thread.CurrentThread.ManagedThreadId.ToString()}' has been stopped");
        }

        private void RoutineInternal(ForeignDevice device, IReadOnlyCollection<ISessionHandler> sessions)
        {
            while (true)
            {
                if (_isStopped)
                    break;

                if (_updatePolicy.IsDeviceShouldBeProcessed(device, _logger))
                    ProcessNext(device, sessions);

                Thread.Sleep(_updatePolicy.UpdateTimeout);
            }
        }

        private void ProcessNext(ForeignDevice device, IReadOnlyCollection<ISessionHandler> sessions)
        {
            try
            {
                foreach (ISessionHandler session in sessions)
                    session.ProcessNext();
                
                _updatePolicy.OnMessageReceived(device, _logger);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                _logger.LogWarning(deviceNotRespondException.Message);
                _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding, _logger);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                _logger.LogCritical(invalidMessageException.Message);
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