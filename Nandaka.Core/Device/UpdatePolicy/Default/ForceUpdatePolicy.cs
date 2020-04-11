using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Device
{
    public class ForceUpdatePolicy : IDeviceUpdatePolicy
    {
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;
        
        private IEnumerator<NandakaDevice> _enumerator;
        private NandakaDevice _lastDeviceInCycle;
        
        public TimeSpan RequestTimeout { get; }
        public TimeSpan UpdateTimeout { get; }
        
        public ForceUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
            _enumerator = Enumerable.Empty<NandakaDevice>().GetEnumerator();
        }
        
        public ForceUpdatePolicy(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
            int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds) 
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
                TimeSpan.FromMilliseconds(updateTimoutMilliseconds)) { }
        
        public NandakaDevice GetNextDevice(MasterDeviceManager manager, ILog log, out bool isUpdateCycleCompleted)
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    UpdateEnumerator(manager);
                    continue;
                }

                NandakaDevice nextDevice = _enumerator.Current;
                if (nextDevice == null)
                    // todo: create a custom exception
                    throw new Exception("Next device is null");

                if (nextDevice.State != DeviceState.Connected)
                    continue;

                isUpdateCycleCompleted = nextDevice.Address == _lastDeviceInCycle.Address;

                return _enumerator.Current;
            }
        }

        public void OnMessageReceived(NandakaDevice device, ILog log)
        {
            // Empty.
        }

        public void OnErrorOccured(NandakaDevice device, DeviceError error, ILog log)
        {
            log.AppendMessage(LogMessageType.Error, $"Error occured with {device}. Reason: {error}");
        }

        public void OnUnexpectedDeviceResponse(MasterDeviceManager manager, NandakaDevice expectedDevice, int responseDeviceAddress, ILog log)
        {
            log.AppendMessage(LogMessageType.Warning, $"Message from unexpected device {responseDeviceAddress} received");
        }
        
        private void UpdateEnumerator(MasterDeviceManager manager)
        {
            IEnumerable<NandakaDevice> devicesToUpdate = manager.SlaveDevices
                .Where(device => device.State == DeviceState.Connected)
                .ToArray();
            
            if (devicesToUpdate.All(device => device.State != DeviceState.Connected))
                // todo: create a custom exception
                throw new Exception("All devices is not connected");

            _lastDeviceInCycle = devicesToUpdate.Last();
            _enumerator = devicesToUpdate.GetEnumerator();
        }
    }
}