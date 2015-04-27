using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration.CSharp
{
    public class FieldInfo : ClassCodeFragment
    {
        public bool IsNullable;
        public bool IsReadOnly;
        public bool IsConst;
        public string Value;

        public new FieldInfo Public { get { return (FieldInfo)base.Public; } }
        public new FieldInfo Protected { get { return (FieldInfo)base.Protected; } }
        public new FieldInfo Private { get { return (FieldInfo)base.Private; } }
        public new FieldInfo Static { get { return (FieldInfo)base.Static; } }
        public new FieldInfo Virtual { get { throw new NotSupportedException(); } }
        public new FieldInfo Override { get { throw new NotSupportedException(); } }
        public FieldInfo Nullable { get { IsNullable = true; return this ; } }

        public FieldInfo ReadOnly { get { IsReadOnly = true; return this; } }
        public FieldInfo Const { get { IsConst = true; return this; } }

        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();

            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".F(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".F(a).Ind(indent)));

            list.Add("{0} {1}{2}{3} {4}{5} {6};".F(
                Access, 
                IsStatic ? "static ": IsConst ? "const " : "", 
                IsReadOnly ? "readonly " : "", 
                Type, 
                IsNullable ? "?" : "",
                Name, 
                string.IsNullOrEmpty(Value) ? "" : " = " + Value).Ind(indent));
            
            list.Add("".Ind(indent));
            return list;
        }
    }
}