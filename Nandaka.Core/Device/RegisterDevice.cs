using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Device
{
    public abstract class RegisterDevice : INotifyPropertyChanged
    {
        public abstract ObservableCollection<IRegisterGroup> RegisterGroups { get; }
        public RegisterTable Table { get; }
        public IProtocol Protocol { get; }
        public int Address { get; }
        public IRegistersUpdatePolicy UpdatePolicy { get; }
        public int ErrorCounter { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected RegisterDevice(IProtocol protocol, int address, RegisterTable table, IRegistersUpdatePolicy updatePolicy)
        {
            Protocol = protocol;
            Address = address;
            Table = table;
            UpdatePolicy = updatePolicy;
        }

        public void ErrorOccured()
        {
            ErrorCounter++;
            RaisePropertyChanged(nameof(ErrorCounter));
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
