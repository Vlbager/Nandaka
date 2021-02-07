using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Session;

namespace Nandaka.Core.Threading
{
    public sealed class DeviceSessionCollection
    {
        public ForeignDevice Device { get; }
        public IReadOnlyCollection<ISession> Sessions { get; }

        public DeviceSessionCollection(ForeignDevice device, IReadOnlyCollection<ISession> sessions)
        {
            Device = device;
            Sessions = sessions;
        }
    }
}