using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Logging
{

    public sealed class Log : ILog, IDisposable
    {
        // todo: define options in separate entity
        private const string DefaultLogFileName = "Nandaka.log";
        
        private static readonly string AppDataFolderPath;
        private static readonly object SyncRoot;
        private static readonly Dictionary<string, Log> Instances;

        private static readonly ThreadLocal<ILog> ThreadInstance;
        private static readonly ThreadLocal<int> ThreadIdTag;

        private static LogLevel LogLevel = LogLevel.Normal;
        
        private readonly BlockingCollection<LogMessage> _buffer;
        private readonly StreamWriter _writer;
        private int _ownerCounter;
        
        public static ILog Instance => ThreadInstance.Value!;

        static Log()
        {
            AppDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Nandaka");
            SyncRoot = new object();
            Instances = new Dictionary<string, Log>();
            ThreadInstance = new ThreadLocal<ILog>(() => new NullLog());
            ThreadIdTag = new ThreadLocal<int>(() => Thread.CurrentThread.ManagedThreadId);
        }

        private Log(string logFileName)
        {
            string logFilePath = Path.Combine(AppDataFolderPath, logFileName);
            _writer = new StreamWriter(logFilePath) { AutoFlush = true };
            _buffer = new BlockingCollection<LogMessage>();
            _ownerCounter = 1;
            Thread thread = new Thread(Routine) { IsBackground = true };
            thread.Start();
        }

        private static Log Create(string fileName)
        {
            lock (SyncRoot)
            {
                if (Instances.ContainsKey(fileName))
                {
                    Log instance = Instances[fileName];
                    Interlocked.Increment(ref instance._ownerCounter);
                    return instance;
                }
                
                if (!Directory.Exists(AppDataFolderPath))
                    Directory.CreateDirectory(AppDataFolderPath);
                
                var log = new Log(fileName);
                Instances.Add(fileName, log);

                return log;
            }  
        }
        
        public static IDisposable InitializeLog(string fileName = DefaultLogFileName)
        {
            Log logInstance = Create(fileName);
            ThreadInstance.Value = logInstance;
            return logInstance;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region static appends
        public static void AppendMessage(string message)
        {
            Instance.AppendMessage(message);
        }

        public static void AppendMessage(LogLevel logLevel, string message)
        {
            Instance.AppendMessage(logLevel, message);
        }
        
        public static void AppendWarning(string message)
        {
            Instance.AppendWarning(message);
        }
        
        public static void AppendWarning(LogLevel logLevel, string message)
        {
            Instance.AppendWarning(logLevel, message);
        }

        public static void AppendException(Exception exception, string message)
        {
            Instance.AppendException(exception, message);
        }
        
        public static void AppendException(LogLevel logLevel, Exception exception, string message)
        {
            Instance.AppendException(logLevel, exception, message);
        }
        #endregion

        #region ILog
        void ILog.AppendMessage(string message)
        {
            Append(LogMessageType.Info, message);
        }
        
        void ILog.AppendMessage(LogLevel logLevel, string message)
        {
            Append(LogMessageType.Info, message, logLevel);
        }

        void ILog.AppendWarning(string message)
        {
            Append(LogMessageType.Warning, message, LogLevel.Low);
        }

        void ILog.AppendWarning(LogLevel logLevel, string message)
        {
            Append(LogMessageType.Warning, message, logLevel);
        }

        void ILog.AppendException(Exception exception, string message)
        {
            Append(LogMessageType.Error, message + Environment.NewLine + exception, LogLevel.Low);
        }

        void ILog.AppendException(LogLevel logLevel, Exception exception, string message)
        {
            Append(LogMessageType.Error, message + Environment.NewLine + exception, logLevel);
        }
        #endregion

        private void Append(LogMessageType type, string message, LogLevel logLevel = LogLevel.Normal)
        {
            if (logLevel > LogLevel)
                return;
            
            _buffer.Add(new LogMessage(ThreadIdTag.Value, message, type));
        }

        private void Routine()
        {
            while (_ownerCounter > 0)
            {
                LogMessage message = _buffer.Take();
                _writer.WriteLine(FormatLogMessage(message));
            }
        }

        private static string FormatLogMessage(LogMessage msg)
        {
            return $"[{msg.Time.ToString(CultureInfo.InvariantCulture)}] <{msg.ThreadId.ToString()}> {msg.Type.ToString()}    {msg.Message}";
        }

        ~Log()
        {
            Dispose(false);
        }
        
        private void Dispose(bool isDisposing)
        {
            if (Interlocked.Decrement(ref _ownerCounter) > 0)
                return;

            if (!isDisposing) 
                return;
            
            Flush();
            _writer.Dispose();
            _buffer.Dispose();
        }

        private void Flush()
        {
            while (!_buffer.IsEmpty())
                _writer.WriteLine(_buffer.Take());
            
            _writer.Flush();
        }
    }
}