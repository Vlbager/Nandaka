﻿using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public abstract class NandakaDevice : INotifyPropertyChanged
    {
        private readonly ISpecificMessageHandler _specificMessageHandler = new NullSpecificMessageHandler();
        private readonly ConcurrentQueue<ISpecificMessage> _specificMessages;

        public ISpecificMessageHandler SpecificMessageHandler
        {
            init => _specificMessageHandler = value;
        }

        public RegisterTable Table { get; private set; }
        public abstract string Name { get; }
        public int Address { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected NandakaDevice(int address, RegisterTable table)
        {
            Address = address;
            _specificMessages = new ConcurrentQueue<ISpecificMessage>();
            Table = table;
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
        
        internal void OnSpecificMessageReceived(ISpecificMessage message)
            => _specificMessageHandler.OnSpecificMessageReceived(message);

        internal bool TryGetSpecific(out ISpecificMessage? message)
            => _specificMessages.TryDequeue(out message);

        protected virtual void RaisePropertyChanged([CallerMemberName]string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}