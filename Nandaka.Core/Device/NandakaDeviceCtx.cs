using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public abstract class NandakaDeviceCtx : INotifyPropertyChanged
    {
        private readonly ISpecificMessageHandler _specificMessageHandler;
        private readonly ConcurrentQueue<ISpecificMessage> _specificMessages;

        public IReadOnlyCollection<IRegister> Registers { get; private set; }
        public abstract string Name { get; }
        public int Address { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected NandakaDeviceCtx(int address, ISpecificMessageHandler specificMessageHandler)
        {
            Address = address;
            _specificMessageHandler = specificMessageHandler;
            _specificMessages = new ConcurrentQueue<ISpecificMessage>();
            Registers = Array.Empty<IRegister>();
        }

        protected NandakaDeviceCtx(int address)
            : this(address, new NullSpecificMessageHandler()) { }

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
            builder.AppendLine($"Registers count: '{Registers.Count}'");

            foreach (IRegister register in Registers)
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

        protected static IRegister<T> CreateRwRegister<T>(int address, T value = default)
            where T: struct
        {
            return new Register<T>(address, RegisterType.ReadRequest, value);
        }

        protected static IReadOnlyRegister<T> CreateRoRegister<T>(int address, T value = default)
            where T: struct
        {
            return new Register<T>(address, RegisterType.WriteRequest, value);
        }
    }
}