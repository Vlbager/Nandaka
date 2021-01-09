using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Example.MasterExample;
using Nandaka.Core.Device;
using Nandaka.Core.Network;
using Nandaka.Core.Registers;
using Nandaka.MilliGanjubus;

namespace Example
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static class Program
    {
        public static void Main()
        {
            //MasterExample();

            IReadOnlyList<IRegister> byteRegisters = new IRegister<byte>[]
            {
                new Register<byte>(1, RegisterType.ReadRequest),
                new Register<byte>(1, RegisterType.ReadRequest)
            };

            if (byteRegisters is IReadOnlyList<IRegister<byte>> roByte)
                Console.WriteLine("byte registers is OK");

            Console.ReadLine();
        }

        private static void MasterExample()
        {
            Console.WriteLine("MasterExample started");
            
            var manager = new MasterDeviceManager();
            var device = TestDeviceCtx.Create();
            
            var serialPortProvider = new SerialDataPortProvider("COM3", 57600);
            var protocol = MgProtocol.Create(serialPortProvider, device);
            
            manager.AddSlaveDevice(device);
            manager.Start(protocol, new ForceUpdatePolicy(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1)));

            device.ByteRoRegister.OnRegisterChanged += (sender, args) => Console.WriteLine("Byte ro register has been changed");

            device.ByteRwRegister.Value  = 1;
            device.ShortRwRegister.Value = 2;
            device.IntRwRegister.Value   = 3;
            device.LongRwRegister.Value  = 4;
            device.UlongRwRegister.Value = 5;

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}