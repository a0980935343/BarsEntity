using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barsix.BarsEntity
{
    using BarsOptions;

    public static class TypeHelper
    {
        public static bool IsReference(this FieldOptions field)
        {
            return !field.IsBasicType() && !field.Collection;
        }

        public static bool IsBasicType(string type)
        {
            List<string> types = new List<string>() { "int", "long", "string", "DateTime", "bool", "short", "byte", "decimal", "float", "double" };
            return types.Contains(TypeHelper.BasicAlias(type));
        }

        public static bool IsBasicType(this FieldOptions field)
        {
            return TypeHelper.IsBasicType(field.TypeName) || field.Enum;
        }

        public static bool IsAnonymous(this Type type)
        { 
            return type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0 &&
                   type.FullName.Contains("AnonymousType");
        }

        internal static string BasicAlias(string type)
        {
            Dictionary<string, string> aliasMap = new Dictionary<string, string>{ 
                {"Boolean", "bool"}, 
                {"Int16", "short"}, 
                {"Int32", "int"}, 
                {"Int64", "long"}, 
                {"DateTime", "DateTime" },
                {"Single", "float" } 
            };

            if (aliasMap.ContainsKey(type))
                return aliasMap[type];
            else
                return type.ToLower();
        }

        internal static string BasicStrongName(string type)
        {
            Dictionary<string, string> aliasMap = new Dictionary<string, string>{ 
                {"Boolean", "bool"}, 
                {"Int16", "short"}, 
                {"Int32", "int"}, 
                {"Int64", "long"}, 
                {"DateTime", "DateTime" },
                {"Single", "float" } 
            };

            if (aliasMap.ContainsValue(type))
                return aliasMap.First(x => x.Value == type).Key;
            else
                return type.Substring(0,1).ToUpper() + type.Substring(1);
        }
    }
}
