using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity
{
    using BarsOptions;

    public static class TypeHelper
    {
        public static bool IsReference(this FieldOptions field)
        {
            return !field.IsBasicType() && !field.Collection;
        }
        
        public static bool IsBasicType(this FieldOptions field)
        {
            List<string> types = new List<string>() { "int", "long", "string", "DateTime", "bool", "short", "byte" };
            return types.Contains(field.TypeName);
        }
    }
}
