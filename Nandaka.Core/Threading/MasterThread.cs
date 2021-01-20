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
        private readonly Dictionary<int, MasterSession> _deviceSessions;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly Thread _thread;
        private readonly string _masterName;
        private readonly DisposableList _disposable;
        
        private bool _isStopped;

        private MasterThread(MasterDeviceDispatcher dispatcher, IProtocol protocol, string masterName)
        {
            _dispatcher = dispatcher;
            _masterName = masterName;
            _deviceSessions = dispatcher.SlaveDevices.ToDictionary(device => device.Address,
                                                                   device => new MasterSession(protocol, device, dispatcher));

            _disposable = new DisposableList();
            
            _thread = new Thread(Routine) { IsBackground = true };
        }

        public static MasterThread Create(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, IProtocol protocol, IDeviceUpdatePolicy updatePolicy, string masterName)
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

                    ForeignDeviceCtx deviceCtx = _dispatcher.GetNextDevice();

                    Log.AppendMessage($"Set current device: {deviceCtx}");

                    SendNextMessage(deviceCtx);
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

        private void SendNextMessage(ForeignDeviceCtx deviceCtx)
        {
            MasterSession session = _deviceSessions[deviceCtx.Address];

            try
            {
                if (deviceCtx.TryGetSpecific(out ISpecificMessage? specificMessage))
                    session.ProcessSpecificMessage(specificMessage!);
                else
                    session.ProcessNextMessage();

                _dispatcher.OnMessageReceived(deviceCtx);
            }
            catch (DeviceNotRespondException deviceNotRespondException)
            {
                Log.AppendWarning(deviceNotRespondException.Message);
                _dispatcher.OnErrorOccured(deviceCtx, DeviceError.NotResponding);
            }
            catch (InvalidAddressReceivedException invalidAddressException)
            {
                Log.AppendWarning(invalidAddressException.Message);
                _dispatcher.OnUnexpectedDeviceResponse(deviceCtx, invalidAddressException.ReceivedAddress);
            }
            catch (InvalidMessageReceivedException invalidMessageException)
            {
                Log.AppendWarning(invalidMessageException.Message);
                _dispatcher.OnErrorOccured(deviceCtx, DeviceError.WrongPacketData);
            }
        }

        public void Dispose()
        {
            _isStopped = true;
            _disposable.Dispose();
        }
    }
}
