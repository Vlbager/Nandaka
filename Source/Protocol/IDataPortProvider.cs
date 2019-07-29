using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public interface IDataPortProvider<T>
    {
        void Write(T data);
        T Read();

        event EventHandler<T> OnDataRecieved;
    }
}
