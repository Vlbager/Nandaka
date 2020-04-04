﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Device
{
    public class DefaultDeviceUpdatePolicy : IDeviceUpdatePolicy
    {
        private const int MaxErrorInRow = 3;

        private readonly IReadOnlyCollection<DeviceInfo> _devicesToUpdate;
        private readonly ILog _log;
        
        private IEnumerator<DeviceInfo> _enumerator;

        public TimeSpan WaitTimeout { get; }

        private DefaultDeviceUpdatePolicy(TimeSpan waitTimeout, MasterDevice masterDevice, ILog log)
        {
            WaitTimeout = waitTimeout;
            _log = log;
            _devicesToUpdate = masterDevice.SlaveDevices
                .Select(device => new DeviceInfo(device))
                .ToArray();
            _enumerator = GetNextEnumerator();
        }

        public static DefaultDeviceUpdatePolicy Create(TimeSpan waitTimeout, MasterDevice masterDevice, ILog log)
        {
            if (masterDevice.SlaveDevices.IsEmpty())
                // todo: create a custom exception
                throw new Exception("Master device should contain at least 1 slave device");
            
            return new DefaultDeviceUpdatePolicy(waitTimeout, masterDevice, new PrefixLog(log, "[Device Update Policy]"));
        }

        public RegisterDevice GetNextDevice(MasterDevice masterDevice)
        {
            while (true)
            {
                if (!_enumerator.MoveNext())
                {
                    _enumerator = GetNextEnumerator();
                    continue;
                }

                RegisterDevice nextDevice = _enumerator.Current?.Device;

                if (nextDevice?.State != DeviceState.Connected)
                    continue;

                return _enumerator.Current.Device;
            }
        }

        public void OnMessageReceived(RegisterDevice device)
        {
            DeviceInfo deviceInfo = GetDeviceInfo(device);
            deviceInfo.ClearErrorCounter();
        }

        public DeviceErrorHandlerResult OnErrorOccured(RegisterDevice device, DeviceError error)
        {
            _log.AppendMessage(LogMessageType.Error, $"Error occured with {device}. Reason: {error}");
            
            DeviceInfo deviceInfo = GetDeviceInfo(device);
            if (!deviceInfo.IsDeviceShouldBeStopped(error, MaxErrorInRow))
                return DeviceErrorHandlerResult.Continue;
            
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
            
            _log.AppendMessage(LogMessageType.Error, $"Device has reached the max number of errors. {device} will be disconnected");
            
            return DeviceErrorHandlerResult.Stop;
        }

        public void OnUnexpectedDeviceResponse(int expectedDeviceAddress, int responseDeviceAddress)
        {
            _log.AppendMessage(LogMessageType.Warning, $"Message from unexpected device {responseDeviceAddress} received");
            
            DeviceInfo responseDeviceInfo = FindDeviceInfo(responseDeviceAddress);
            if (responseDeviceInfo != null && responseDeviceInfo.IsDeviceSkipPreviousMessage())
            {
                _log.AppendMessage(LogMessageType.Error,
                    $"Device {responseDeviceAddress} is responding too long and will be disconnected");
                responseDeviceInfo.Device.State = DeviceState.Corrupted;
                return;
            }

            _log.AppendMessage(LogMessageType.Error, $"Device {expectedDeviceAddress}");
            
            RegisterDevice exceptedDevice = GetDeviceInfo(expectedDeviceAddress).Device;
            exceptedDevice.State = DeviceState.Corrupted;
        }


        private IEnumerator<DeviceInfo> GetNextEnumerator()
        {
            if (_devicesToUpdate.All(deviceInfo => deviceInfo.Device.State != DeviceState.Connected))
                // todo: create a custom exception
                throw new Exception("All devices is not connected");

            return _devicesToUpdate.GetEnumerator();
        }

        private DeviceInfo GetDeviceInfo(RegisterDevice device)
            => GetDeviceInfo(device.Address);

        private DeviceInfo GetDeviceInfo(int deviceAddress)
            => _devicesToUpdate.First(deviceInfo => deviceInfo.Device.Address == deviceAddress);

        private DeviceInfo FindDeviceInfo(int deviceAddress)
            => _devicesToUpdate.FirstOrDefault(deviceInfo => deviceInfo.Device.Address == deviceAddress);
    }
}