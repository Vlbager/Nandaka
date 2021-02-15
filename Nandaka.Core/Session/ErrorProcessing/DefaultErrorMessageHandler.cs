using Nandaka.Core.Device;

namespace Nandaka.Core.Session
{
    internal sealed class DefaultErrorMessageHandler : IErrorMessageHandler
    {
        private readonly MasterDeviceDispatcher _dispatcher;
        private readonly ForeignDevice _device;

        public DefaultErrorMessageHandler(MasterDeviceDispatcher dispatcher, ForeignDevice device)
        {
            _dispatcher = dispatcher;
            _device = device;
        }
        
        public void OnErrorReceived(ErrorMessage errorMessage)
        {
            _dispatcher.OnErrorOccured(_device, DeviceError.ErrorReceived);
        }
    }
}