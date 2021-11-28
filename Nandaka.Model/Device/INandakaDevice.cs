using System;
using Nandaka.Model.Registers;

namespace Nandaka.Model.Device
{
    public interface INandakaDevice
    {
        string Name { get; }
        int Address { get; }
        event EventHandler<RegisterChangedEventArgs>? OnRegisterChanged;
    }
}