using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.Protocol
{
    public class SerivalDataPortProvider : IDataPortProvider<byte[]>
    {
        public event EventHandler<byte[]> OnDataRecieved;

        private readonly SerialPort _serialPort;

        public SerivalDataPortProvider(string portName, int baudRate = 9600)
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Open();
            _serialPort.DataReceived += (sender, args) => OnDataRecieved(sender, Read());
        }

        public byte[] Read()
        {
            var byteCount = _serialPort.BytesToRead;
            var result = new byte[byteCount];
            _serialPort.Read(result, 0, byteCount);
            return result;
        }

        public void Write(byte[] data)
        {
            _serialPort.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Closes serial port and releases unmanaged resources.
        /// If Serial Port is disconnected physically, then called safe
        /// disconnect to avoid exception.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Close();
                _serialPort.Dispose();

            }
            catch (UnauthorizedAccessException)
            {
                SafeDisconnect(_serialPort, _serialPort.BaseStream);
            }
        }

        static void SafeDisconnect(SerialPort port, Stream internalSerialStream)
        {
            GC.SuppressFinalize(port);
            GC.SuppressFinalize(internalSerialStream);

            ShutdownEventLoopHandler(internalSerialStream);

            try
            {
                port.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        static void ShutdownEventLoopHandler(Stream internalSerialStream)
        {
            try
            {
                FieldInfo eventRunnerField = internalSerialStream.GetType()
                    .GetField("eventRunner", BindingFlags.NonPublic | BindingFlags.Instance);

                if (eventRunnerField != null)
                {
                    object eventRunner = eventRunnerField.GetValue(internalSerialStream);
                    Type eventRunnerType = eventRunner.GetType();

                    FieldInfo endEventLoopFieldInfo = eventRunnerType.GetField(
                        "endEventLoop", BindingFlags.Instance | BindingFlags.NonPublic);

                    FieldInfo eventLoopEndedSignalFieldInfo = eventRunnerType.GetField(
                        "eventLoopEndedSignal", BindingFlags.Instance | BindingFlags.NonPublic);

                    FieldInfo waitCommEventWaitHandleFieldInfo = eventRunnerType.GetField(
                        "waitCommEventWaitHandle", BindingFlags.Instance | BindingFlags.NonPublic);

                    if (endEventLoopFieldInfo != null && eventLoopEndedSignalFieldInfo != null &&
                        waitCommEventWaitHandleFieldInfo != null)
                    {
                        var eventLoopEndedWaitHandle =
                            (WaitHandle)eventLoopEndedSignalFieldInfo.GetValue(eventRunner);
                        var waitCommEventWaitHandle =
                            (ManualResetEvent)waitCommEventWaitHandleFieldInfo.GetValue(eventRunner);

                        endEventLoopFieldInfo.SetValue(eventRunner, true);

                        // Sometimes the event loop handler resets the wait handle
                        // before exiting the loop and hangs (in case of USB disconnect)
                        // In case it takes too long, brute-force it out of its wait by
                        // setting the handle again.
                        do
                        {
                            waitCommEventWaitHandle.Set();
                        } while (!eventLoopEndedWaitHandle.WaitOne(2000));

                    }

                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
