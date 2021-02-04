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
    internal sealed class MasterThread : IDisposable
    {
        private readonly Dictionary<int, ISession> _deviceSessions;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly Thread _thread;
        private readonly string _masterName;
        private readonly DisposableList _disposable;
        
        private bool _isStopped;

        private MasterThread(MasterDeviceDispatcher dispatcher, IProtocol protocol, string masterName)
        {
            _dispatcher = dispatcher;
            _masterName = masterName;
            _deviceSessions = dispatcher.SlaveDevices
                                        .ToDictionary(device => device.Address, 
                                            device => new MasterSyncSession(protocol, dispatcher.RequestTimeout, device) as ISession);

            _disposable = new DisposableList();
            
            _thread = new Thread(Routine) { IsBackground = true };
        }

        public static MasterThread Create(IReadOnlyCollection<ForeignDevice> slaveDevices, IProtocol protocol, IDeviceUpdatePolicy updatePolicy, string masterName)
        {
            var dispatcher = new MasterDeviceDispatcher(slaveDevices, updatePolicy);
            return new MasterThread(dispatcher, protocol, masterName);
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

                    ForeignDevice device = _dispatcher.GetNextDevice();

                    Log.AppendMessage($"Set current device: {device}");

                    SendNextMessage(device);
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
            
            string devicesInfo = String.Join(Environment.NewLine, _dispatcher.SlaveDevices.Select(device => device.ToLogLine()));
            
            Log.AppendMessage(LogLevel.Low, "Starting Master thread, devices:" + Environment.NewLine + devicesInfo);
        }

        private void SendNextMessage(ForeignDevice device)
        {
            ISession session = _deviceSessions[device.Address];

            try
            {
                // if (device.TryGetSpecific(out ISpecificMessage? specificMessage))
                //     session.ProcessSpecificMessage(specificMessage!);
                // else
                session.ProcessNextMessage();

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
