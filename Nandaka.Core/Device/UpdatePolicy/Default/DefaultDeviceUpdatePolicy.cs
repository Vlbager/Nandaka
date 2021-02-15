using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Logging;

namespace Nandaka.Core.Device
{
    public class DefaultDeviceUpdatePolicy : IDeviceUpdatePolicy
    {
        // todo: specify default values.
        private const int DefaultMaxErrorInRowCount = 3;
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;

        private readonly int _maxErrorInRowCount;
        
        private IEnumerator<ForeignDevice> _enumerator;
        private ForeignDevice? _lastDeviceInCycle;

        /// <summary>
        /// Timeout between request and response.
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        
        /// <summary>
        /// Timeout between each update cycle.
        /// </summary>
        public TimeSpan UpdateTimeout { get; }

        public DefaultDeviceUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout, int maxErrorInRowCount)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
            _maxErrorInRowCount = maxErrorInRowCount;
            _enumerator = Enumerable.Empty<ForeignDevice>().GetEnumerator();
        }
        
        public DefaultDeviceUpdatePolicy(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
            int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds, int maxErrorInRowCount = DefaultMaxErrorInRowCount) 
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
            TimeSpan.FromMilliseconds(updateTimoutMilliseconds), maxErrorInRowCount) { }
        

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
            device.ErrorCounter.Clear();
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error)
        {
            Log.AppendWarning($"Error occured with {device}. Reason: {error}");
            
            if (!IsDeviceShouldBeStopped(device, error))
                return;
            
            switch (error)
            {
                case DeviceError.ErrorReceived:
                case DeviceError.WrongPacketData:
                    device.State = DeviceState.Corrupted;
                    break;

                case DeviceError.NotResponding:
                    device.State = DeviceState.NotResponding;
                    break;

                default:
                    device.State = DeviceState.Disconnected;
                    break;
            }
            
            Log.AppendWarning($"Device has reached the max number of errors. {device} will be disconnected");
        }

        public void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDevice> slaveDevices, ForeignDevice expectedDevice, int responseDeviceAddress)
        {
            Log.AppendWarning($"Message from unexpected device {responseDeviceAddress} received");

            ForeignDevice? responseDevice = slaveDevices.FirstOrDefault(device => device.Address == responseDeviceAddress);
            if (responseDevice != null && IsDeviceSkipPreviousMessage(responseDevice))
            {
                Log.AppendWarning($"Device {responseDevice} is responding too long and will be disconnected");
                responseDevice.State = DeviceState.Corrupted;
                return;
            }

            Log.AppendWarning($"Device {expectedDevice} response with wrong address");
            expectedDevice.State = DeviceState.Corrupted;
        }
        
        private bool IsDeviceShouldBeStopped(ForeignDevice device, DeviceError newError)
        {
            int errorCount = device.ErrorCounter.TryAdd(newError, 1) 
                           ? 1 
                           : device.ErrorCounter[newError];
            
            device.ErrorCounter[newError] = errorCount + 1;

            return errorCount > _maxErrorInRowCount;
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
        
        private static bool IsDeviceSkipPreviousMessage(ForeignDevice device)
        {
            return device.ErrorCounter.ContainsKey(DeviceError.NotResponding);
        }
    }
}