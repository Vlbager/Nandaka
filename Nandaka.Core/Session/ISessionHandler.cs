using System;

namespace Nandaka.Core.Session
{
    public interface ISessionHandler : IDisposable
    {
        void ProcessNext();
    }
}