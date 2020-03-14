using Nandaka.Core.Table;

namespace Nandaka.Core
{
    public interface IDevice
    {
        RegisterTable Table { get; }
        int Address { get; }
        // todo: implement this.
    }
}
