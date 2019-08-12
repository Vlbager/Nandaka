using System;
using System.Text;
using System.Linq;
using Nandaka;


namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var serialPort = new SerivalDataPortProvider("COM6");
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
