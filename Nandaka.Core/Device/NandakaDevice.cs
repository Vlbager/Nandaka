using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Model.Device;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Device
{
    public abstract class NandakaDevice : INandakaDevice
    {
        private readonly ISpecificMessageHandler _specificMessageHandler = new NullSpecificMessageHandler();
        private readonly ConcurrentQueue<ISpecificMessage> _specificMessages;

        public ISpecificMessageHandler SpecificMessageHandler
        {
            init => _specificMessageHandler = value;
        }

        public abstract RegisterTable Table { get; }
        public abstract string Name { get; }
        public abstract int Address { get; }

        public event EventHandler<RegisterChangedEventArgs>? OnRegisterChanged
        {
            add => Table.OnRegisterChanged += value;
            remove => Table.OnRegisterChanged -= value;
        }

        protected NandakaDevice()
        {
            _specificMessages = new ConcurrentQueue<ISpecificMessage>();
        }

        public void SendSpecific(ISpecificMessage message, bool isAsync)
        {
            _specificMessages.Enqueue(message);

            if (isAsync) 
                return;

            while (!_specificMessages.IsEmpty)
                _specificMessageHandler.WaitResponse();
        }

        public string ToLogLine()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Device name: '{Name}'; Device address: '{Address.ToString()}'");
            builder.AppendLine($"Registers count: '{Table.Count}'");

            foreach (IRegister register in Table)
            {
                builder.AppendLine($"        {register.GetValueType()} register: " +
                                   $"registerType = {register.RegisterType.ToString()}; " +
                                   $"address = {register.Address.ToString()};");
            }

            builder.AppendLine();

            return builder.ToString();
        }

        public override string ToString()
        {
            return ToLogLine();
        }

        internal void OnSpecificMessageReceived(ISpecificMessage message, ILogger logger)
            => _specificMessageHandler.OnSpecificMessageReceived(message, logger);

        internal bool TryGetSpecific(out ISpecificMessage? message)
            => _specificMessages.TryDequeue(out message);
    }
}