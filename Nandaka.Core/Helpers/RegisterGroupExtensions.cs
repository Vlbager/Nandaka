using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterGroupExtensions
    {
        public static string AllAddresses(this IEnumerable<IRegisterGroup> groups)
        {
            IEnumerable<int> addresses = groups.Select(group => group.Address);
            return String.Join(", ", addresses);
        }
    }
}