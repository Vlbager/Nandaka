using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public interface IProtocol
    {
        void SendMessage(ITransferData message);

        event EventHandler<ITransferData> MessageReceived;
    }
}
