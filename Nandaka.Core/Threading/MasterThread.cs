using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    internal class MasterThread : IDisposable
    {
        private readonly Dictionary<int, MasterSession> _deviceSessions;
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly ILog _log;

        private readonly Thread _thread;
        private bool _isStopped;

        private MasterThread(MasterDeviceManager manager, MasterDeviceDispatcher dispatcher, IProtocol protocol, ILog log)
        {
            _dispatcher = dispatcher;
            _log = log;
            _deviceSessions = manager.SlaveDevices.ToDictionary(device => device.Address,
                    device => new MasterSession(protocol, device, dispatcher, _log));
            
            _thread = new Thread(Routine) { IsBackground = true };
        }

        public static MasterThread Create(MasterDeviceManager deviceManager, IProtocol protocol, IDeviceUpdatePolicy updatePolicy, ILog log)
        {
            var threadLog = new PrefixLog(log, "[Master]");
            var dispatcher = MasterDeviceDispatcher.Create(deviceManager, updatePolicy, threadLog);
            return new MasterThread(deviceManager, dispatcher, protocol, threadLog);
        }

        public void StartRoutine() => _thread.Start();

        public void Join() => _thread.Join();

        public void Stop() => _isStopped = true;

        private void Routine()
        {
            try
            {
                while (true)
                {
                    if (_isStopped)
                        break;

                    NandakaDevice device = _dispatcher.GetNextDevice();

                    _log.AppendMessage(LogMessageType.Info, $"Set current device: {device}");

                    SendNextMessage(device);
                }
            }
            catch (Exception exception)
            {
                _log.AppendMessage(LogMessageType.Error, "Unexpected error occured");
                _log.AppendMessage(LogMessageType.Error, exception.ToString());
                Stop();
            }
        }

        private void SendNextMessage(NandakaDevice device)
        {
            MasterSession session = _deviceSessions[device.Address];

            ISpecificMessage specificMessage = default;
            try
            {
                if (device.TryGetSpecific(out specificMessage))
                    session.ProcessSpecificMessage(specificMessage);
                else
                    session.ProcessNextMessage();
                
                _dispatcher.OnMessageReceived(device);
            }
            // todo: refactor with exception handling system.
            catch (ApplicationException expectedException)
            {
                _dispatcher.OnErrorOccured(device, DeviceError.NotResponding);
                // Back specific message in specific message queue.
                if (specificMessage != null)
                    device.SendSpecific(specificMessage, false);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
