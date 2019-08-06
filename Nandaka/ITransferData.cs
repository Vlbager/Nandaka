using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface ITransferData
    {
        IEnumerable<IRegister> Registers { get; }
        DataType DataType { get; }
    }
}
