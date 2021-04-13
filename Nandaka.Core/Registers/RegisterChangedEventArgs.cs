using System;

namespace Nandaka.Core.Registers
{
    public sealed class RegisterChangedEventArgs : EventArgs
    {
        public int RegisterAddress { get; }
        
        public RegisterChangedEventArgs(int registerAddress)
        {
            RegisterAddress = registerAddress;
        }
    }
}