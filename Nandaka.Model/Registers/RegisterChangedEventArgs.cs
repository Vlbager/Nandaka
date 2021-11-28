using System;

namespace Nandaka.Model.Registers
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