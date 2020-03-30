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

        private readonly Thread _thread;
        private bool _isStopped;

        public MasterThread(MasterDevice masterDevice, IProtocol protocol, IDeviceUpdatePolicy updatePolicy)
        {
            _masterDevice = masterDevice;
            _updatePolicy = updatePolicy;
            _deviceSessions = _masterDevice.SlaveDevices
                .ToDictionary(device => device.Address,
                    device => new MasterSession(protocol, device, device.UpdatePolicy, updatePolicy));

            _thread = new Thread(Routine) { IsBackground = true};
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

                    SendNextMessage(device);
                }
            }
            catch (Exception exception)
            {
                // todo: Add loger.
                Console.WriteLine(exception);
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
            }
            // todo: refactor with exception handling system.
            catch (ApplicationException expectedException)
            {
                _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding);
                if (specificMessage != null)
                    device.SendSpecific(specificMessage, false);
            }
            catch (Exception unexpectedException)
            {
                throw;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
