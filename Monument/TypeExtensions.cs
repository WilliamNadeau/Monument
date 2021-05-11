using System;
using System.Collections.Generic;
using System.Linq;

namespace Monument
{
    public static class TypeExtensions
    {
        public static Type ToTypeKey(this Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        public static bool ImplementsInterface(this Type type, Type parentType) => parentType.IsInterface
            && (parentType.IsAssignableFrom(type)
            || (parentType.IsGenericType
            && type.GetInterfaces().Select(ToTypeKey).Contains(parentType.ToTypeKey())));

        public static bool IsOpenGeneric(this Type type) => type.ContainsGenericParameters;

        public static bool HasInterfaces(this Type type) => type.GetInterfaces().Any();

        public static bool IsComposite(this Type type) => type.HasInterfaces()
            && type.GetConstructors().Single().GetParameters()
                .Where(p => p.ParameterType.IsGenericType)
                .Where(p => p.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Any(p => p.ParameterType.GetGenericArguments().Single().ToTypeKey() == type.GetInterfaces().Single().ToTypeKey());

        public static bool IsDecorator(this Type type) => type.HasInterfaces()
            && type.GetConstructors().Single().GetParameters()
                .Any(p => p.ParameterType == type.GetInterfaces().Single());

        public static Lifestyle GetLifestyle(this Type type) => Lifestyle.Transient;
    }
}
