using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Nandaka.Core.Attributes;
using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Device
{
    public abstract class NandakaDevice : INotifyPropertyChanged
    {
        private readonly ISpecificMessageHandler _specificMessageHandler;
        private readonly ConcurrentQueue<ISpecificMessage> _specificMessages;
        private bool _isReflected;

        public IReadOnlyCollection<IRegisterGroup> RegisterGroups { get; private set; }
        public abstract string Name { get; }
        public int Address { get; }
        internal IRegistersUpdatePolicy UpdatePolicy { get; }
        public DeviceState State { get; set; }
        public Dictionary<DeviceError, int> ErrorCounter { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected NandakaDevice(int address, DeviceState state, IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler)
        {
            Address = address;
            UpdatePolicy = updatePolicy;
            _specificMessageHandler = specificMessageHandler;
            _specificMessages = new ConcurrentQueue<ISpecificMessage>();
            ErrorCounter = new Dictionary<DeviceError, int>();
            State = state;
        }

        protected NandakaDevice(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state)
            : this(address, state, updatePolicy, new NullSpecificMessageHandler()) { }

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

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void Reflect(bool isManagedByMaster)
        {
            if (_isReflected)
                return;
            
            Type type = GetType();

            var validator = new RegisterTableValidator();
            
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                Type propertyType = propertyInfo.PropertyType;
                
                if (!propertyInfo.PropertyType.IsInheritedFromInterface(nameof(IRegister)))
                    continue;

                if (!(propertyInfo.GetValue(this) is IRegisterGroup registerGroup))
                    continue;

                if (propertyType == typeof(IRwRegister<>))
                {
                    registerGroup.SetRegisterTypeViaReflection(isManagedByMaster
                        ? RegisterType.ReadWrite
                        : RegisterType.Read);
                }
                else if (propertyType == typeof(IRoRegister<>))
                {
                    registerGroup.SetRegisterTypeViaReflection(isManagedByMaster
                        ? RegisterType.Read
                        : RegisterType.ReadWrite);
                }
                else
                {
                    continue;
                }

                registerGroup.OnRegisterChanged += (sender, args) => RaisePropertyChanged(propertyInfo.Name);

                IEnumerable<RegisterModifyAttribute> attributes = propertyInfo.CustomAttributes
                    .SafeCast<CustomAttributeData, RegisterModifyAttribute>();

                foreach (RegisterModifyAttribute attribute in attributes)
                    attribute.Modify(registerGroup);

                validator.AddGroup(registerGroup);
            }

            RegisterGroups = validator.GetGroups();

            _isReflected = true;
        }
    }
}
