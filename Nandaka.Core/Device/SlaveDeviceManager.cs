using System;
using Nandaka.Core.Logging;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class SlaveDeviceManager : IDisposable
    {
        private readonly SlaveThread _thread;
        
        public NandakaDeviceCtx DeviceCtx { get; }

        private SlaveDeviceManager(IProtocol protocol, NandakaDeviceCtx deviceCtx)
        {
            DeviceCtx = deviceCtx;
            _thread = SlaveThread.Create(deviceCtx, protocol);
            _thread.Start();
        }

        public static SlaveDeviceManager Start(IProtocol protocol, NandakaDeviceCtx deviceCtx)
        {
            return new SlaveDeviceManager(protocol, deviceCtx);
        }

        public void Dispose()
        {
            _thread.Dispose();
        }
    }
}
