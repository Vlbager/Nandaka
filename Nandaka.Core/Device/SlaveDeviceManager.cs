using System;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class SlaveDeviceManager : IDisposable
    {
        private readonly ILog _log;

        private SlaveThread? _thread;
        
        public ForeignDeviceCtx DeviceCtx { get; }

        public SlaveDeviceManager(ForeignDeviceCtx deviceCtx)
        {
            _log = Log.Instance;
            DeviceCtx = deviceCtx;
        }

        public void Start(IProtocol protocol)
        {
            _log.AppendMessage(LogMessageType.Info, $"Starting {DeviceCtx.Name} slave thread");
            _thread = SlaveThread.Create(DeviceCtx, protocol, _log);
            _thread.Start();
        }

        public void Dispose()
        {
            _thread?.Dispose();
        }
    }
}
