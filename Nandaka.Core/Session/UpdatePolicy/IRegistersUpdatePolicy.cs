﻿using Nandaka.Core.Device;

namespace Nandaka.Core.Session
{
    public interface IRegistersUpdatePolicy
    {
        IRegisterMessage GetNextMessage(NandakaDevice device);
    }
}
