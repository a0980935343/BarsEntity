using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    /// <summary>  </summary>
    public class EntityGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            base.Generate(project, options, fragments); 

            CheckFolder("Entities" + (options.IsDictionary ? "\\Dict" : ""));

            NamespaceInfo ns = new NamespaceInfo();
            ClassInfo cls = new ClassInfo();
            ns.NestedValues.Add(cls);

            if (options.Fields.Any(x => x.TypeName == "DateTime"))
                ns.OuterUsing.Add("System");
            if (options.Fields.Any(x => x.Collection ))
                ns.OuterUsing.Add("System.Collections.Generic");
            
            if (options.Fields.Any(x => x.JsonIgnore || x.Collection))
                ns.OuterUsing.Add("Newtonsoft.Json");

            ns.Name = "{0}.Entities".F(project.Name);

            if (options.BaseClass == "NamedBaseEntity")
            {
                ns.InnerUsing.Add("Bars.MosKs.Core.Entities.Base");
            }
            else
            {
                ns.InnerUsing.Add("Bars.B4.DataAccess");
            }

            if (options.AcceptFiles)
            {
                ns.InnerUsing.Add("Bars.B4.Modules.FileStorage");
            }

            if (!string.IsNullOrEmpty(options.DisplayName))
                cls.Summary = options.DisplayName;

            cls.Name = options.ClassName;
            cls.BaseClass = options.BaseClass;

            if (options.Stateful)
            {
                ns.InnerUsing.Add("Bars.B4.Modules.States");
                cls.Interfaces.Add("IStatefulEntity");
            }

            if (options.Signable)
            {
                ns.InnerUsing.Add("Bars.B4.Modules.DigitalSignature");
                ns.InnerUsing.Add("Bars.B4.Modules.DigitalSignature.Attributes");
                cls.Interfaces.Add("ISignableEntity");
            }

            foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
            {
                if (options.BaseClass == "NamedBaseEntity" && field.FieldName == "Name")
                    continue;

                var pi = (new PropertyInfo()).Public.Virtual.Auto.Get().Set();

                if (!string.IsNullOrEmpty(field.Comment))
                    pi.Summary = field.Comment;

                if (field.JsonIgnore || field.Collection)
                    pi.Attributes.Add("JsonIgnore");

                pi.Name = field.FieldName;
                pi.Type = field.FullTypeName;

                if (options.Signable && pi.Attributes.Count == 0)
                    pi.Attributes.Add("DigitalSignature(\"{0}\")".F(pi.Name));

                cls.AddProperty(pi);
            }

            foreach (var field in options.Fields.Where(x => x.Collection))
            {
                var pi = (new PropertyInfo()).Public.Virtual.Auto.Get().Set();

                if (!string.IsNullOrEmpty(field.Comment))
                    pi.Summary = field.Comment;

                pi.Attributes.Add("JsonIgnore");

                pi.Name = field.FieldName;
                pi.Type = field.FullTypeName;

                cls.AddProperty(pi);
            }

            foreach (var field in options.Fields.Where(x => x.TypeName.EndsWith("View")))
            {
                var pi = (new PropertyInfo()).Public.Virtual.Auto.Get().Set();

                if (!string.IsNullOrEmpty(field.Comment))
                    pi.Summary = field.Comment;

                pi.Attributes.Add("JsonIgnore");

                pi.Name = field.FieldName;
                pi.Type = field.FullTypeName;

                cls.AddProperty(pi);
            }

            /*if (options.Stateful)
            {
                cls.AddProperty(new PropertyInfo() { Name = "State", Type = "State", Attributes = new List<string>{"DigitalSignature(\"State\")" }});
            }*/
            
            CreateFile("Entities\\" +(options.IsDictionary ? "Dict\\" : "")+ options.ClassName + ".cs", ns.ToString());
        }
    }
}
