using Nandaka.Core.Device;

namespace Nandaka.Core.Session
{
    internal sealed class DefaultErrorMessageHandler : IErrorMessageHandler
    {
        private readonly DeviceUpdatePolicyWrapper _updatePolicyWrapper;
        private readonly ForeignDevice _device;

        public DefaultErrorMessageHandler(DeviceUpdatePolicyWrapper updatePolicyWrapper, ForeignDevice device)
        {
            _updatePolicyWrapper = updatePolicyWrapper;
            _device = device;
        }
        
        public void OnErrorReceived(ErrorMessage errorMessage)
        {
            _updatePolicyWrapper.OnErrorOccured(_device, DeviceError.ErrorReceived);
        }
    }
}