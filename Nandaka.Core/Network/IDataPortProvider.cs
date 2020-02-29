using System;

namespace Nandaka.Core.Network
{
    public interface IDataPortProvider<T>
    {
        void Write(T data);
        T Read();

        event EventHandler<T> OnDataReceived;
    }
}
