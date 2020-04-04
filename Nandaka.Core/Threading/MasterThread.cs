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
        private readonly MasterDevice _masterDevice;
        private readonly Dictionary<int, MasterSession> _deviceSessions;
        private readonly IDeviceUpdatePolicy _updatePolicy;
        private readonly ILog _log;

        private readonly Thread _thread;
        private bool _isStopped;

        private MasterThread(MasterDevice masterDevice, IProtocol protocol, IDeviceUpdatePolicy updatePolicy)
        {
            _masterDevice = masterDevice;
            _updatePolicy = updatePolicy;
            _log = new PrefixLog(Log.Instance, $"[{masterDevice.Name}Thread]");
            _deviceSessions = _masterDevice.SlaveDevices
                .ToDictionary(device => device.Address,
                    device => new MasterSession(protocol, device, updatePolicy, _log));

            _thread = new Thread(Routine) { IsBackground = true};
        }

        public static MasterThread Create(MasterDevice masterDevice, IProtocol protocol)
        {
            throw new NotImplementedException();
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

                    RegisterDevice device = _updatePolicy.GetNextDevice(_masterDevice);

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

        private void SendNextMessage(RegisterDevice device)
        {
            MasterSession session = _deviceSessions[device.Address];

            ISpecificMessage specificMessage = default;
            try
            {
                if (device.TryGetSpecific(out specificMessage))
                    session.SendSpecificMessage(specificMessage, _updatePolicy.WaitTimeout);
                else
                    session.SendNextMessage(_updatePolicy.WaitTimeout);
                
                _updatePolicy.OnMessageReceived(device);
            }
            // todo: refactor with exception handling system.
            catch (ApplicationException expectedException)
            {
                _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding);
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
