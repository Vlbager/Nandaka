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
        
        private IEnumerator<ForeignDeviceCtx> _enumerator;
        private ForeignDeviceCtx? _lastDeviceInCycle;
        
        public TimeSpan RequestTimeout { get; }
        public TimeSpan UpdateTimeout { get; }
        
        public ForceUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
            _enumerator = Enumerable.Empty<ForeignDeviceCtx>().GetEnumerator();
        }
        
        public ForceUpdatePolicy(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
            int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds) 
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
                TimeSpan.FromMilliseconds(updateTimoutMilliseconds)) { }
        
        public ForeignDeviceCtx GetNextDevice(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ILog log, out bool isUpdateCycleCompleted)
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    UpdateEnumerator(slaveDevices);
                    continue;
                }

                ForeignDeviceCtx nextDeviceCtx = _enumerator.Current;
                if (nextDeviceCtx == null)
                    throw new NandakaBaseException("Next device is null");

                if (nextDeviceCtx.State != DeviceState.Connected)
                    continue;

                isUpdateCycleCompleted = nextDeviceCtx.Address == _lastDeviceInCycle?.Address;

                return _enumerator.Current;
            }
        }

        public void OnMessageReceived(ForeignDeviceCtx deviceCtx, ILog log)
        {
            // Empty.
        }

        public void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error, ILog log)
        {
            log.AppendMessage(LogMessageType.Error, $"Error occured with {deviceCtx}. Reason: {error}");
        }

        public void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress, ILog log)
        {
            log.AppendMessage(LogMessageType.Warning, $"Message from unexpected device {responseDeviceAddress} received");
        }
        
        private void UpdateEnumerator(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices)
        {
            IEnumerable<ForeignDeviceCtx> devicesToUpdate = slaveDevices
                .Where(device => device.State == DeviceState.Connected)
                .ToArray();
            
            if (devicesToUpdate.All(device => device.State != DeviceState.Connected))
                throw new DeviceNotFoundException("All devices is not connected");

            _lastDeviceInCycle = devicesToUpdate.Last();
            _enumerator = devicesToUpdate.GetEnumerator();
        }
    }
}