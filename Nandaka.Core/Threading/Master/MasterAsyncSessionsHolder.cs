using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Logging;
using Nandaka.Core.Session;
using Nandaka.Core.Util;

namespace Nandaka.Core.Threading
{
    internal sealed class MasterAsyncSessionsHolder : IMasterSessionsHolder
    {
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly Thread[] _threads;
        private readonly string _masterName;
        private readonly DisposableList _disposable;
        
        private bool _isStopped;

        public MasterAsyncSessionsHolder(MasterDeviceDispatcher dispatcher, IReadOnlyCollection<DeviceSessionCollection> sessions, string masterName)
        {
            _dispatcher = dispatcher;
            _masterName = masterName;
            _disposable = new DisposableList();
            _threads = sessions.Select(deviceSessions => new Thread(() => Routine(deviceSessions)))
                               .ToArray();
        }

        public void StartRoutine() => _threads.ForEach(thread => thread.Start());

        private void Routine(DeviceSessionCollection deviceSessions)
        {
            try
            {
                ForeignDevice device = deviceSessions.Device;

                InitializeLog(device);
                
                IReadOnlyCollection<ISession> sessions = deviceSessions.Sessions;
                
                while (true)
                {
                    if (_isStopped)
                        break;

                    ProcessNext(device, sessions);
                }
            }
            catch (Exception exception)
            {
                Log.AppendException(exception, "Unexpected error occured");
            }
            
            Log.AppendWarning("Master thread has been stopped");
        }

        private void InitializeLog(ForeignDevice device)
        {
            _disposable.Add(Log.InitializeLog($"{_masterName}.Master.log"));

            Log.AppendMessage("Starting Master thread, device:" + Environment.NewLine + device.ToLogLine());
        }

        private void ProcessNext(ForeignDevice device, IReadOnlyCollection<ISession> sessions)
        {
            try
            {
                foreach (ISession session in sessions)
                    session.ProcessNext();
                
                _dispatcher.OnMessageReceived(device);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                Log.AppendWarning(deviceNotRespondException.Message);
                _dispatcher.OnErrorOccured(device, DeviceError.NotResponding);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                Log.AppendWarning(invalidMessageException.Message);
                _dispatcher.OnErrorOccured(device, DeviceError.WrongPacketData);
            }
        }

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}