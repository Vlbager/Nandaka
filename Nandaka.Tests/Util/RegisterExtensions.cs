﻿using System;
using System.Reflection;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Tests.Util
{
    public static class RegisterExtensions
    {
        public static void ChangeLastUpdateTime(this IRegister register, DateTime updateTime)
        {
            Type type = register.GetType();
            FieldInfo? field = type.GetField("_lastUpdateTime", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(register, updateTime);
        }
    }
}