using System;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    internal static class ReflectionHelper
    {
        public static bool IsInheritedFromInterface(this Type self, string interfaceName)
        {
            return self.GetInterface(interfaceName) != null;
        }

        public static void SetRegisterTypeViaReflection(this IRegisterGroup registerGroup, RegisterType registerType)
        {
            registerGroup.GetType()
                .GetProperty(nameof(IRegisterGroup.RegisterType))
                ?.SetValue(registerGroup, registerType);
        }
    }
}