using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Device
{
    public abstract class NandakaDevice : INotifyPropertyChanged
    {
        private readonly ISpecificMessageHandler _specificMessageHandler;
        private readonly ConcurrentQueue<ISpecificMessage> _specificMessages;

        public abstract ObservableCollection<IRegisterGroup> RegisterGroups { get; }
        public abstract string Name { get; }
        public RegisterTable Table { get; }
        public int Address { get; }
        internal IRegistersUpdatePolicy UpdatePolicy { get; }
        public DeviceState State { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected NandakaDevice(int address, RegisterTable table, DeviceState state,
            IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler)
        {
            Address = address;
            Table = table;
            UpdatePolicy = updatePolicy;
            _specificMessageHandler = specificMessageHandler;
            _specificMessages = new ConcurrentQueue<ISpecificMessage>();
            State = state;
        }

        protected NandakaDevice(int address, RegisterTable table, IRegistersUpdatePolicy updatePolicy, DeviceState state)
            : this(address, table, state, updatePolicy, new NullSpecificMessageHandler()) { }

        public void SendSpecific(ISpecificMessage message, bool isAsync)
        {
            _specificMessages.Enqueue(message);

            if (isAsync) 
                return;

            while (!_specificMessages.IsEmpty)
                _specificMessageHandler.WaitResponse();
        }
        internal void OnSpecificMessageReceived(ISpecificMessage message)
            => _specificMessageHandler.OnSpecificMessageReceived(message);

        internal bool TryGetSpecific(out ISpecificMessage message)
            => _specificMessages.TryDequeue(out message);

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
