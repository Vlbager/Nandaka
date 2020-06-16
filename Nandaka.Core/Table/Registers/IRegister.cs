using System;

namespace Nandaka.Core.Table
{
    public interface IRegister
    {
        int Address { get; }

        RegisterType RegisterType { get; }
        
        event EventHandler OnRegisterChanged;

        /// <summary>
        /// Convert register value in little-endianness bytes
        /// </summary>
        byte[] ToBytes();
    }
}
