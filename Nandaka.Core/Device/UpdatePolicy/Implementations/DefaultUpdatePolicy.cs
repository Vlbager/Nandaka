using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Nandaka.Model.Device;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// Default policy that doesn't update the device if it fails or disconnected
    /// </summary>
    public sealed class DefaultUpdatePolicy : IDeviceUpdatePolicy
    {
        private readonly int _maxErrorInRowCount;
        private readonly ConcurrentDictionary<int, DeviceErrorCounter> _errorCounterByDeviceAddress;

        /// <summary>
        /// <inheritdoc cref="IDeviceUpdatePolicy.UpdateTimeout"/>
        /// </summary>
        public TimeSpan UpdateTimeout { get; }
        
        /// <summary>
        /// <inheritdoc cref="IDeviceUpdatePolicy.RequestTimeout"/>
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        
        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        /// <param name="maxErrorInRowCount">The number of errors in a row, which is enough to disable the device</param>
        public DefaultUpdatePolicy(TimeSpan requestTimeout, TimeSpan updateTimeout, int maxErrorInRowCount)
        {
            RequestTimeout = requestTimeout;
            UpdateTimeout = updateTimeout;
            _maxErrorInRowCount = maxErrorInRowCount;
            _errorCounterByDeviceAddress = new ConcurrentDictionary<int, DeviceErrorCounter>();
        }

        public bool IsDeviceShouldBeProcessed(ForeignDevice device, ILogger logger)
        {
            return device.State == DeviceState.Connected;
        }

        public void OnMessageReceived(ForeignDevice device, ILogger logger)
        {
            GetDeviceErrorCounter(device).Clear();
        }

        public void OnErrorOccured(ForeignDevice device, DeviceError error, ILogger logger)
        {
            logger.LogError("Error occured with {0}. Reason: {1}", device, error);
            
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
            
            logger.LogError("Device has reached the max number of errors. {0} will be disconnected", device);
        }

        private bool IsDeviceShouldBeStopped(ForeignDevice device, DeviceError newError)
        {
            DeviceErrorCounter errorCounter = GetDeviceErrorCounter(device);
            
            int errorCount = errorCounter.Increment(newError);

            return errorCount >= _maxErrorInRowCount;
        }

        private DeviceErrorCounter GetDeviceErrorCounter(ForeignDevice device)
        {
            if (!_errorCounterByDeviceAddress.ContainsKey(device.Address))
                _errorCounterByDeviceAddress.TryAdd(device.Address, new DeviceErrorCounter());
            
            return _errorCounterByDeviceAddress[device.Address];
        }
    }
}