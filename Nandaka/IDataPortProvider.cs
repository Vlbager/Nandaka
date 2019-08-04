using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IDataPortProvider<T>
    {
        void Write(T data);
        T Read();

        event EventHandler<T> OnDataRecieved;
    }
}
