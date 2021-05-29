using System;
using Nandaka.Core.Logging;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// Default policy that doesn't update the device if it fails or disconnected
    /// </summary>
    public sealed class DefaultUpdatePolicy : IDeviceUpdatePolicy
    {
        private readonly int _maxErrorInRowCount;

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
        }

        public bool IsDeviceShouldBeProcessed(ForeignDevice device)
        {
            return device.State == DeviceState.Connected;
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

        private bool IsDeviceShouldBeStopped(ForeignDevice device, DeviceError newError)
        {
            int errorCount = device.ErrorCounter.TryAdd(newError, 1) 
                           ? 1 
                           : device.ErrorCounter[newError];
            
            device.ErrorCounter[newError] = errorCount + 1;

            return errorCount > _maxErrorInRowCount;
        }
    }
}