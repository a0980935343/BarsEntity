using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
    public class ClassInfo : CodeFragment
    {
        public string BaseClass;
        
        public List<string> Interfaces = new List<string>();

        public new ClassInfo Public { get { return (ClassInfo)base.Public; } }
        public new ClassInfo Protected { get { return (ClassInfo)base.Protected; } }
        public new ClassInfo Private { get { return (ClassInfo)base.Private; } }

        public IEnumerable<FieldInfo> Fields { get { return NestedValues.Where(x => x is FieldInfo).Select(x => x as FieldInfo); } }
        public IEnumerable<PropertyInfo> Properties { get { return NestedValues.Where(x => x is PropertyInfo).Select(x => x as PropertyInfo); } }
        public IEnumerable<MethodInfo> Methods { get { return NestedValues.Where(x => x is MethodInfo).Select(x => x as MethodInfo); } }

        public void AddMethod(MethodInfo method)
        {
            NestedValues.Add(method);
        }

        public void AddProperty(PropertyInfo property)
        {
            NestedValues.Add(property);
        }

        public void AddField(FieldInfo field)
        {
            NestedValues.Add(field);
        }

        public override List<string> Generate(int indent)
        {
            List<string> list = new List<string>();
            

            if (!string.IsNullOrEmpty(Summary))
                list.Add("///<summary> {0} </summary>".F(Summary).Ind(indent));

            Attributes.ForEach(a => list.Add("[{0}]".F(a).Ind(indent)));

            List<string> baseTypes = new List<string>();
            if (!string.IsNullOrEmpty(BaseClass))
                baseTypes.Add(BaseClass);

            baseTypes.AddRange(Interfaces);

            list.Add("{2}{3} class {0}{1}".F(Name, (!baseTypes.Any() ? "" : " : " + string.Join(", ", baseTypes)), Access, IsStatic ? "static " : "").Ind(indent));
            list.Add("{".Ind(indent));

            Fields.ToList().ForEach(x =>
            {
                list.AddRange(x.Generate(indent + 1));
            });

            Properties.ToList().ForEach(x =>
            {
                list.AddRange(x.Generate(indent + 1));
            });

            Methods.ToList().ForEach(x =>
            {
                list.AddRange(x.Generate(indent + 1));
            });

            foreach (var cls in NestedValues.Where(x => x is ClassInfo))
            {
                list.AddRange(cls.Generate(indent + 1));
            }

            list.Add("}".Ind(indent));

            return list;
        }
    }
}
