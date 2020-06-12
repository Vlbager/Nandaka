using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;

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
        
        public NandakaDevice GetNextDevice(IReadOnlyCollection<NandakaDevice> slaveDevices, ILog log, out bool isUpdateCycleCompleted)
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    UpdateEnumerator(slaveDevices);
                    continue;
                }

                NandakaDevice nextDevice = _enumerator.Current;
                if (nextDevice == null)
                    throw new NandakaBaseException("Next device is null");

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

        public void OnUnexpectedDeviceResponse(IReadOnlyCollection<NandakaDevice> slaveDevices, NandakaDevice expectedDevice, int responseDeviceAddress, ILog log)
        {
            log.AppendMessage(LogMessageType.Warning, $"Message from unexpected device {responseDeviceAddress} received");
        }
        
        private void UpdateEnumerator(IReadOnlyCollection<NandakaDevice> slaveDevices)
        {
            IEnumerable<NandakaDevice> devicesToUpdate = slaveDevices
                .Where(device => device.State == DeviceState.Connected)
                .ToArray();
            
            if (devicesToUpdate.All(device => device.State != DeviceState.Connected))
                throw new DeviceNotFoundException("All devices is not connected");

            _lastDeviceInCycle = devicesToUpdate.Last();
            _enumerator = devicesToUpdate.GetEnumerator();
        }
    }
}