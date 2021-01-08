using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;

namespace Nandaka.Core.Device
{
    public class DefaultDeviceUpdatePolicy : IDeviceUpdatePolicy
    {
        // todo: specify default values.
        private const int DefaultMaxErrorInRowCount = 3;
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;

        private readonly int _maxErrorInRowCount;
        
        private IEnumerator<ForeignDeviceCtx> _enumerator;
        private ForeignDeviceCtx? _lastDeviceInCycle;

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
            _enumerator = Enumerable.Empty<ForeignDeviceCtx>().GetEnumerator();
        }
        
        public DefaultDeviceUpdatePolicy(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
            int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds, int maxErrorInRowCount = DefaultMaxErrorInRowCount) 
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
            TimeSpan.FromMilliseconds(updateTimoutMilliseconds), maxErrorInRowCount) { }
        

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
            deviceCtx.ErrorCounter.Clear();
        }

        public void OnErrorOccured(ForeignDeviceCtx deviceCtx, DeviceError error, ILog log)
        {
            log.AppendMessage(LogMessageType.Error, $"Error occured with {deviceCtx}. Reason: {error}");
            
            if (!IsDeviceShouldBeStopped(deviceCtx, error))
                return;
            
            switch (error)
            {
                case DeviceError.ErrorReceived:
                case DeviceError.WrongPacketData:
                    deviceCtx.State = DeviceState.Corrupted;
                    break;

                case DeviceError.NotResponding:
                    deviceCtx.State = DeviceState.NotResponding;
                    break;

                default:
                    deviceCtx.State = DeviceState.Disconnected;
                    break;
            }
            
            log.AppendMessage(LogMessageType.Error, $"Device has reached the max number of errors. {deviceCtx} will be disconnected");
        }

        public void OnUnexpectedDeviceResponse(IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, ForeignDeviceCtx expectedDeviceCtx, int responseDeviceAddress, ILog log)
        {
            log.AppendMessage(LogMessageType.Warning, $"Message from unexpected device {responseDeviceAddress} received");

            ForeignDeviceCtx? responseDevice = slaveDevices.FirstOrDefault(device => device.Address == responseDeviceAddress);
            if (responseDevice != null && IsDeviceSkipPreviousMessage(responseDevice))
            {
                log.AppendMessage(LogMessageType.Error,
                    $"Device {responseDevice} is responding too long and will be disconnected");
                responseDevice.State = DeviceState.Corrupted;
                return;
            }

            log.AppendMessage(LogMessageType.Error, $"Device {expectedDeviceCtx} response with wrong address");
            expectedDeviceCtx.State = DeviceState.Corrupted;
        }
        
        private bool IsDeviceShouldBeStopped(ForeignDeviceCtx deviceCtx, DeviceError newError)
        {
            int errorCount = deviceCtx.ErrorCounter[newError];
            deviceCtx.ErrorCounter[newError] = errorCount + 1;

            return errorCount > _maxErrorInRowCount;
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
        
        private static bool IsDeviceSkipPreviousMessage(ForeignDeviceCtx deviceCtx)
        {
            return deviceCtx.ErrorCounter.ContainsKey(DeviceError.NotResponding);
        }
    }
}