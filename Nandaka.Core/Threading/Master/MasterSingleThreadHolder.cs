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
        private readonly DeviceUpdatePolicyWrapper _updatePolicyWrapper;
        private readonly Thread _thread;
        private readonly string _masterName;
        private readonly DisposableList _disposable;
        
        private bool _isStopped;

        public MasterSingleThreadHolder(DeviceUpdatePolicyWrapper updatePolicyWrapper, IProtocol protocol, MasterDeviceSessionMap sessionMap, string masterName)
        {
            _disposable = new DisposableList();
            _updatePolicyWrapper = updatePolicyWrapper;
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
                
                while (true)
                {
                    if (_isStopped)
                        break;
                    
                    _highPrioritySessionHandler.ProcessNext();

                    ProcessNext();
                }
            }
            catch (Exception exception)
            {
                Log.AppendException(exception, "Unexpected error occured");
            }
            
            Log.AppendWarning("Master thread has been stopped");
        }

        private void InitializeLog()
        {
            _disposable.Add(Log.InitializeLog($"{_masterName}.Master.log"));
            
            string devicesInfo = String.Join(Environment.NewLine, _updatePolicyWrapper.SlaveDevices.Select(device => device.ToLogLine()));
            
            Log.AppendMessage(LogLevel.Low, "Starting single Master thread, devices:" + Environment.NewLine + devicesInfo);
        }

        private void ProcessNext()
        {
            DeviceSessionCollection sessionCollection = _deviceSessionsMap.GetNextDevice();

            ForeignDevice device = sessionCollection.Device;

            Log.AppendMessage($"Set current device: {device}");
            
            IReadOnlyCollection<ISessionHandler> sessions = sessionCollection.SessionHandlers;

            try
            {
                foreach (ISessionHandler session in sessions)
                    session.ProcessNext();
                
                _updatePolicyWrapper.OnMessageReceived(device);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                Log.AppendWarning(deviceNotRespondException.Message);
                _updatePolicyWrapper.OnErrorOccured(device, DeviceError.NotResponding);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                Log.AppendWarning(invalidMessageException.Message);
                _updatePolicyWrapper.OnErrorOccured(device, DeviceError.WrongPacketData);
            }
        }

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}
