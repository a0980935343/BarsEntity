using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
    public class PropertyInfo : ClassCodeFragment
    {
        public bool IsNullable;
        public bool AutoProperty;
        public List<string> Getter;
        public List<string> Setter;

        public PropertyInfo Public { get { return (PropertyInfo)base.Public; } }
        public PropertyInfo Protected { get { return (PropertyInfo)base.Protected; } }
        public PropertyInfo Private { get { return (PropertyInfo)base.Private; } }

        public PropertyInfo Virtual { get { return (PropertyInfo)base.Virtual; } }
        public PropertyInfo Override { get { return (PropertyInfo)base.Override; } }
        public PropertyInfo Nullable { get { IsNullable = true; return this; } }
        public PropertyInfo Auto { get { AutoProperty = true; return this; } }
        public PropertyInfo Get(string modifier = "") { Getter = new List<string> { modifier }; return this; }
        public PropertyInfo Set(string modifier = "") { Setter = new List<string> { modifier }; return this; }

        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();

            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".F(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".F(a).Ind(indent)));
            if (AutoProperty)
                list.Add("{5} {0}{3}{1} {2}{4}".F(IsVirtual ? "virtual " : IsOverride ? "override " : "", Type, Name, IsNullable ? "?" : "", AutoProperty ? " {{ {0} {1} }}".F(Getter != null ? Getter.First() + "get;" : "", Setter != null ? Setter.First() + "set;" : "") : ";", Access).Ind(indent));
            else
            {
                list.Add("{4} {0}{3}{1} {2}".F(IsVirtual ? "virtual " : IsOverride ? "override " : "", Type, Name, IsNullable ? "?" : "", Access).Ind(indent));
                list.Add("{".Ind(indent));

                if (Getter != null)
                {
                    if (Getter.Count == 1)
                        list.Add("get {{ {0} }}".F(Getter.First()).Ind(indent + 1));
                    else
                    {
                        list.Add("get".Ind(indent + 1));
                        list.Add("{".Ind(indent + 1));
                        foreach (var line in Getter)
                        {
                            list.Add(line.Ind(indent + 2));
                        }
                        list.Add("}".Ind(indent + 1));
                    }
                }

                if (Setter != null)
                {
                    if (Setter.Count == 1)
                        list.Add("set {{ {0} }}".F(Setter.First()).Ind(indent + 1));
                    else
                    {
                        list.Add("set".Ind(indent + 1));
                        list.Add("{".Ind(indent + 1));
                        foreach (var line in Setter)
                        {
                            list.Add(line.Ind(indent+2));
                        }
                        list.Add("}".Ind(indent+1));
                    }
                }

                list.Add("}".Ind(indent));
            }
            list.Add("".Ind(indent));
            return list;
        }
    }
}
