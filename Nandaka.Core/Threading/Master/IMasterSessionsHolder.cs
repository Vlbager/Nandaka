using System;

namespace Nandaka.Core.Threading
{
    public interface IMasterSessionsHolder : IDisposable
    {
        void StartRoutine();
    }
}