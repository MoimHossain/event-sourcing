using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace SuperNova.Shared.Supports
{
    public static class ReflectionSupport
    {
        // Quick and dirty implementation for now.
        public static ExpandoObject ToExpando(this object anyObject)
        {
            Ensure.ArgumentNotNull(anyObject, nameof(anyObject));

            if(anyObject.GetType().IsPrimitive)
            {
                throw new NotSupportedException("Primitive types are not supported to convert into Expandos.");
            }

            var expando = new ExpandoObject();
            foreach (var property in anyObject
                .GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType.IsPrimitive)
                {   
                    // for now, not supporting complex types - that a different problem we need to solve
                    expando.TryAdd(property.Name, property.GetValue(anyObject));
                }
            }
            return expando;
        }
    }
}
