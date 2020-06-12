using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Nandaka.Core.Helpers;

namespace Nandaka.Core
{
    internal class Log : ILog, IDisposable
    {
        // todo: define options in separate entity
        private const string LogFileName = "Nandaka.log";
        private static readonly string s_appDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Nandaka");

        private static Lazy<ILog> s_instance = new Lazy<ILog>(Create);
        private bool _isDisposed;

        private readonly StreamWriter _writer;
        private readonly BlockingCollection<string> _buffer;
        private readonly Thread _thread;

        private Log()
        {
            string logFilePath = Path.Combine(s_appDataFolderPath, LogFileName);
            _writer = new StreamWriter(logFilePath) { AutoFlush = true };
            _buffer = new BlockingCollection<string>();
            _thread = new Thread(Routine) { IsBackground = true };
            _thread.Start();
        }

        private static Log Create()
        {
            Directory.CreateDirectory(s_appDataFolderPath);
            return new Log();
        }

        public static ILog Instance => s_instance.Value;

        public void AppendMessage(LogMessageType type, string message)
        {
            if (_isDisposed)
                return;
            
            _buffer.Add($"[{DateTime.Now}][{type}]{message}");
        }

        public void Dispose()
        {
            Dispose(true);
        }


        private void Routine()
        {
            while (!_isDisposed)
            {
                _writer.WriteLine(_buffer.Take());
            }
        }

        ~Log()
        {
            Dispose(false);
        }
        
        private void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;

            if (isDisposing)
            {
                Flush();
                _writer.Dispose();
                _buffer.Dispose();
            }

            _isDisposed = true;
        }

        private void Flush()
        {
            while (!_buffer.IsEmpty())
                _writer.WriteLine(_buffer.Take());
            _writer.Flush();
        }
    }
}