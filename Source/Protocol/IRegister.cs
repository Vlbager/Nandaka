using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IRegister
    {
        int Address { get; }

        byte[] GetBytes();
    }
}
