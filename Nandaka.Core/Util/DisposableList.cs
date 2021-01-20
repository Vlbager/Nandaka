using System;
using System.Collections.Generic;

namespace Nandaka.Core.Util
{
    public sealed class DisposableList : IDisposable
    {
        private readonly IList<IDisposable> _list;

        public DisposableList()
        {
            _list = new List<IDisposable>();
        }

        public T Add<T>(T disposable)
            where T: IDisposable
        {
            _list.Add(disposable);
            return disposable;
        }
        
        public void Dispose()
        {
            foreach (IDisposable disposable in _list)
                disposable.Dispose();
        }
    }
}