using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SuperNova.Shared.Tests.TestUtils
{
    public static class TestUtils
    {
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static object GetInstanceProperty(Type type, object instance, string propertyName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            var property = type.GetProperty(propertyName, bindFlags);
            return property.GetValue(instance);
        }
    }
}
