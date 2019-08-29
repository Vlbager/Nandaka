using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IComposer<out T>
    {
        T Compose(IMessage message);
    }
}
