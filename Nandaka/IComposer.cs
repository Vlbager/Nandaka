using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IComposer<in TIn, out TOut>
    {
        TOut Compose(TIn message);
    }
}
