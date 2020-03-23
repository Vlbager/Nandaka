using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Threading
{
    internal class MasterThread : IDisposable
    {
        private readonly MasterDevice _masterDevice;
        private readonly Dictionary<int, MasterSession> _deviceSessions;
        private readonly IDeviceUpdatePolicy _updatePolicy;

        private readonly Thread _thread;
        private bool _isStopped;

        public MasterThread(MasterDevice masterDevice, IDeviceUpdatePolicy updatePolicy)
        {
            _masterDevice = masterDevice;
            _updatePolicy = updatePolicy;
            _deviceSessions = _masterDevice.SlaveDevices
                .ToDictionary(device => device.Address,
                    device => new MasterSession(device.Protocol, device, device.UpdatePolicy));

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

                    SendRegisterMessage();
                }
            }
            catch (Exception exception)
            {
                // todo: Add loger.
                Console.WriteLine(exception);
            }
        }

        private void SendRegisterMessage()
        {
            RegisterDevice device = _updatePolicy.GetNextDevice(_masterDevice);
            MasterSession session = _deviceSessions[device.Address];

            session.SendRegisterMessage(out IReadOnlyCollection<IRegisterGroup> sentGroups);

            var messageReceivedResetEvent = new ManualResetEventSlim(initialState: false);

            void ResponseReceived(object sender, IFrameworkMessage message)
                => OnResponseReceived(message, messageReceivedResetEvent);

            _masterDevice.Protocol.MessageReceived += ResponseReceived;

            if (messageReceivedResetEvent.Wait(_updatePolicy.MillisecondsTimeout))
            {
                _masterDevice.Protocol.MessageReceived -= ResponseReceived;
                return;
            }

            _masterDevice.Protocol.MessageReceived -= ResponseReceived;

            DeviceErrorHandlerResult handlerResult = _updatePolicy.OnErrorOccured(device, DeviceError.NotResponding);
            
        }

        private void OnResponseReceived(IFrameworkMessage response, ManualResetEventSlim resetEvent)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
