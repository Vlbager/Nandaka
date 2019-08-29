using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public enum MessageType
    {
        ReadDataRequest,
        WriteDataRequest,
        ReadDataResponse,
        WriteDataResponse,
        ErrorMessage,
        // todo: think about it.
        ApplicationDataError,
        None
    }

}
