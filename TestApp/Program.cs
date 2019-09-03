using System;
using System.Text;
using System.Linq;
using Nandaka;
using Nandaka.MilliGanjubus;


namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var commonMessage = new MilliGanjubusMessage(MessageType.ReadDataRequest, 0xFF);
            commonMessage.AddRegister(new TestByteRegister(1, 2));
            commonMessage.AddRegister(new TestByteRegister(1, 3));
            commonMessage.AddRegister(new TestByteRegister(1, 4));

            foreach (var register in commonMessage.Registers)
            {
                Console.WriteLine(register.GetBytes()[0].ToString());
            }
        }

        private void TestSerialPort()
        {
            var serialPort = new SerialDataPortProvider("COM6");
            serialPort.OnDataRecieved += (sender, message) =>
            {
                Console.Write(Encoding.ASCII.GetString(message));
            };
            foreach (var unused in Enumerable.Range(0, 10))
            {
                serialPort.Write(Encoding.ASCII.GetBytes("Hello, Serial Port!\n"));
            }
            Console.ReadLine();
        }
    }
}
