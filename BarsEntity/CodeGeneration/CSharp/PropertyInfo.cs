using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.CSharp
{
    public class PropertyInfo : ClassCodeFragment
    {
        public bool IsNullable;
        public bool AutoProperty;
        public List<string> Getter;
        public List<string> Setter;

        public new PropertyInfo Public { get { return (PropertyInfo)base.Public; } }
        public new PropertyInfo Protected { get { return (PropertyInfo)base.Protected; } }
        public new PropertyInfo Private { get { return (PropertyInfo)base.Private; } }

        public new PropertyInfo Virtual { get { return (PropertyInfo)base.Virtual; } }
        public new PropertyInfo Override { get { return (PropertyInfo)base.Override; } }
        public PropertyInfo Nullable { get { IsNullable = true; return this; } }
        public PropertyInfo Auto { get { AutoProperty = true; return this; } }
        public PropertyInfo Get(string modifier = "") { Getter = new List<string> { modifier }; return this; }
        public PropertyInfo Set(string modifier = "") { Setter = new List<string> { modifier }; return this; }

        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();

            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".R(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".R(a).Ind(indent)));
            if (AutoProperty)
                list.Add("{5} {0}{3}{1} {2}{4}".R(IsVirtual ? "virtual " : IsOverride ? "override " : "", Type, Name, IsNullable ? "?" : "", AutoProperty ? " {{ {0} {1} }}".R(Getter != null ? Getter.First() + "get;" : "", Setter != null ? Setter.First() + "set;" : "") : ";", Access).Ind(indent));
            else
            {
                list.Add("{4} {0}{3}{1} {2}".R(IsVirtual ? "virtual " : IsOverride ? "override " : "", Type, Name, IsNullable ? "?" : "", Access).Ind(indent));
                list.Add("{".Ind(indent));

                if (Getter != null)
                {
                    if (Getter.Count == 1)
                        list.Add("get {{ {0} }}".R(Getter.First()).Ind(indent + 1));
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
                        list.Add("set {{ {0} }}".R(Setter.First()).Ind(indent + 1));
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
