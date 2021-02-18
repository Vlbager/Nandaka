using System;
using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    public sealed class DeviceSessionCollection : IDisposable
    {
        public ForeignDevice Device { get; }
        public IReadOnlyCollection<ISessionHandler> SessionHandlers { get; }

        public DeviceSessionCollection(ForeignDevice device, IReadOnlyCollection<ISessionHandler> sessionHandlers)
        {
            Device = device;
            SessionHandlers = sessionHandlers;
        }

        public void Dispose()
        {
            foreach (ISessionHandler handler in SessionHandlers)
                handler.Dispose();
        }
    }
}