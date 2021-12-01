using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Example.MasterExample;
using Nandaka.Core.Device;
using Nandaka.Core.Network;
using Nandaka.MilliGanjubus;

namespace Example
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static class Program
    {
        public static void Main()
        {
            MasterExample();
        }

        private static void MasterExample()
        {
            Console.WriteLine("MasterExample started");
            
            var device = new TestDevice(address: 1);
            
            var serialPortProvider = new SerialDataPortProvider("COM3", 57600);
            var protocol = MgProtocol.Create(serialPortProvider, device);
            var updatePolicy = new ForceUpdatePolicy(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1));

            var dealer = MasterDeviceDealer.Start(protocol, updatePolicy, new[] { device });

            device.OnRegisterChanged += (_, args) => Console.WriteLine($"Register {args.RegisterAddress.ToString()} has been changed");
            
            device.IsPowered.Value  = true;
            device.LeftDriveSpeed.Value = 5;
            device.RightDriveSpeed.Value = 5;

            while (true)
            {
                Console.WriteLine($"Right sensor distance: {device.RightSensorDistance};" +
                                  $"Front sensor distance: {device.FrontSensorDistance}");
                Thread.Sleep(1000);
            }
        }
    }
}