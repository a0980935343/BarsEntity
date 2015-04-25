using System;
using System.Collections.Generic;

namespace Barsix.BarsEntity.CodeGeneration
{
    public class MethodInfo : ClassCodeFragment
    {
        public bool IsConstructor;
        public string Params = "";
        public string SignatureParams;

        public List<string> Body = new List<string>();

        public MethodInfo()
        {
            Type = "void";
        }

        public new MethodInfo Public { get { return (MethodInfo)base.Public; } }
        public new MethodInfo Protected { get { return (MethodInfo)base.Protected; } }
        public new MethodInfo Private { get { return (MethodInfo)base.Private; } }

        public new MethodInfo Virtual { get { return (MethodInfo)base.Virtual; } }
        public new MethodInfo Override { get { return (MethodInfo)base.Override; } }

        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();

            
            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".F(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".F(a).Ind(indent)));

            list.Add("{5} {0}{1} {2}({3}){4}".F(IsOverride ? "override " : "", IsConstructor ? "" : Type, Name, Params, !string.IsNullOrEmpty(SignatureParams) ? " : " + SignatureParams : "", Access).Ind(indent));
            list.Add("{".Ind(indent));

            Body.ForEach(b => list.Add(b.Ind(indent+1)));

            list.Add("}".Ind(indent));
            list.Add("".Ind(indent));

            return list;
        }
    }
}
