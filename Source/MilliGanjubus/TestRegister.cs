using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public class TestByteRegister : IRegister
    {
        public int Address { get; private set; }

        public byte Value { get; set; }

        public TestByteRegister(int address, byte value = 0)
        {
            Address = address;
            Value = value;
        }

        public byte[] GetBytes()
        {
            return new byte[] { Value };
        }
    }
}
