﻿using Nandaka.Core.Device;

namespace Nandaka.Core.Session
{
    internal sealed class DefaultErrorMessageHandler : IErrorMessageHandler
    {
        private readonly DeviceUpdatePolicy _updatePolicy;
        private readonly ForeignDevice _device;

        public DefaultErrorMessageHandler(DeviceUpdatePolicy updatePolicy, ForeignDevice device)
        {
            _updatePolicy = updatePolicy;
            _device = device;
        }
        
        public void OnErrorReceived(ErrorMessage errorMessage)
        {
            _updatePolicy.OnErrorOccured(_device, DeviceError.ErrorReceived);
        }
    }
}