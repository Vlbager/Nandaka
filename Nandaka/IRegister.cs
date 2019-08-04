using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IRegister
    {
        int Address { get; }

        byte[] GetBytes();
    }
}
