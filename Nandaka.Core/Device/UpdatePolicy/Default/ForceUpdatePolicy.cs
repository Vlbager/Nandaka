using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;

namespace Nandaka.Core.Device
{
    public class ForceUpdatePolicy : IDeviceUpdatePolicy
    {
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;
        
        private IEnumerator<ForeignDevice> _enumerator;
        private ForeignDevice? _lastDeviceInCycle;
        
        public TimeSpan RequestTimeout { get; }
        public TimeSpan UpdateTimeout { get; }
        
        public ForceUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
            _enumerator = Enumerable.Empty<ForeignDevice>().GetEnumerator();
        }
        
        public ForceUpdatePolicy(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
            int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds) 
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
                TimeSpan.FromMilliseconds(updateTimoutMilliseconds)) { }
        
        public ForeignDevice GetNextDevice(IReadOnlyCollection<ForeignDevice> slaveDevices, out bool isUpdateCycleCompleted)
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    UpdateEnumerator(slaveDevices);
                    continue;
                }

                ForeignDevice nextDevice = _enumerator.Current;
                if (nextDevice == null)
                    throw new NandakaBaseException("Next device is null");

                if (nextDevice.State != DeviceState.Connected)
                    continue;

                isUpdateCycleCompleted = nextDevice.Address == _lastDeviceInCycle?.Address;

                return _enumerator.Current;
            }
        }

        public void OnMessageReceived(ForeignDevice device)
        {
            // Empty.
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            Log.AppendWarning($"Error occured with {device}. Reason: {error}");
        }

        public void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDevice> slaveDevices, ForeignDevice expectedDevice, int responseDeviceAddress)
        {
            Log.AppendWarning($"Message from unexpected device {responseDeviceAddress} received");
        }
        
        private void UpdateEnumerator(IReadOnlyCollection<ForeignDevice> slaveDevices)
        {
            IEnumerable<ForeignDevice> devicesToUpdate = slaveDevices
                .Where(device => device.State == DeviceState.Connected)
                .ToArray();
            
            if (devicesToUpdate.All(device => device.State != DeviceState.Connected))
                throw new DeviceNotFoundException("All devices is not connected");

            _lastDeviceInCycle = devicesToUpdate.Last();
            _enumerator = devicesToUpdate.GetEnumerator();
        }
    }
}