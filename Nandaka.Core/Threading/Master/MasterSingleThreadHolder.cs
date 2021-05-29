using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;
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
        private readonly string _masterName;
        private readonly DisposableList _disposable;
        
        private bool _isStopped;

        public MasterSingleThreadHolder(IDeviceUpdatePolicy updatePolicy, UpdateCandidatesHolder candidatesHolder, IProtocol protocol,
                                        MasterDeviceSessionMap sessionMap, string masterName)
        {
            _disposable = new DisposableList();
            _updatePolicy = updatePolicy;
            _candidatesHolder = candidatesHolder;
            _masterName = masterName;
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
            try
            {
                InitializeLog();
                
                RoutineInternal();
            }
            catch (Exception exception)
            {
                Log.AppendException(exception, "Unexpected error occured");
            }
            
            Log.AppendWarning("Master thread has been stopped");
        }

        private void RoutineInternal()
        {
            while (true)
            {
                foreach (ForeignDevice nextDevice in _candidatesHolder.GetDevicesForProcessing())
                {
                    if (_isStopped)
                        return;

                    _highPrioritySessionHandler.ProcessNext();

                    ProcessDevice(nextDevice);   
                }
            }
        }

        private void ProcessDevice(ForeignDevice device)
        {
            DeviceSessionCollection sessionCollection = _deviceSessionsMap.GetDeviceSessions(device);

            Log.AppendMessage($"Set current device: {device}");
            
            IReadOnlyCollection<ISessionHandler> sessions = sessionCollection.SessionHandlers;

            try
            {
                foreach (ISessionHandler session in sessions)
                    session.ProcessNext();
                
                _updatePolicy.OnMessageReceived(device);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                Log.AppendWarning(deviceNotRespondException.Message);
                _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                Log.AppendWarning(invalidMessageException.Message);
                _updatePolicy.OnErrorOccured(device, DeviceError.WrongPacketData);
            }
        }
        
        private void InitializeLog()
        {
            _disposable.Add(Log.InitializeLog($"{_masterName}.Master.log"));
            
            string devicesInfo = String.Join(Environment.NewLine, _candidatesHolder.Candidates.Select(device => device.ToLogLine()));
            
            Log.AppendMessage(LogLevel.Low, "Starting single Master thread, devices:" + Environment.NewLine + devicesInfo);
        }

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}
