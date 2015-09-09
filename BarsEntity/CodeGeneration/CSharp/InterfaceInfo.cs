using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.CSharp
{
    public class InterfaceInfo : ClassInfo
    {
        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();

            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".R(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".R(a).Ind(indent)));

            List<string> baseTypes = new List<string>();
            if (!string.IsNullOrEmpty(BaseClass))
                baseTypes.Add(BaseClass);

            baseTypes.AddRange(Interfaces);

            list.Add("{2} interface {0}{1}".R(Name, (!baseTypes.Any() ? "" : " : " + string.Join(", ", baseTypes)), Access).Ind(indent));
            list.Add("{".Ind(indent));

            Properties.ToList().ForEach(x =>
            {
                list.AddRange(x.Generate(indent + 1));
            });

            Methods.ToList().ForEach(x =>
            {
                list.AddRange(x.Generate(indent + 1));
            });
            
            if (string.IsNullOrWhiteSpace(list.Last()))
                list.RemoveAt(list.Count - 1);

            list.Add("}".Ind(indent));

            return list;
        }
    }
}
