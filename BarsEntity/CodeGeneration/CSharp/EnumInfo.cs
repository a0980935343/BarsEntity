using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration.CSharp
{
    public class EnumInfo : CodeFragment
    {
        public string BaseType;

        public List<EnumValue> Values = new List<EnumValue>();

        public override List<string> Generate(int indent)
        {
            var list = new List<string>();
            list.Add(((!string.IsNullOrEmpty(Access) ? Access + " " : "") + "enum " + Name + (!string.IsNullOrEmpty(BaseType) ? " : " + BaseType : "")).Ind(indent));
            list.Add("{".Ind(indent));

            foreach (var value in Values)
            {
                list.Add((value.Name + (value.Value.HasValue ? " = " + value.Value.ToString() : "")).Ind(indent+1));
            }

            list.Add("}".Ind(indent));

            return list;
        }
    }

    public struct EnumValue
    {
        public string Name;
        public long? Value;
    }
}
