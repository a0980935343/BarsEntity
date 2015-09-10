using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    /// <summary>  </summary>
    public class EntityGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            _knownTypes.Clear();
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add(options.BaseClass);
            _knownTypes.Add("IList");
            _knownTypes.Add("DateTime");

            NamespaceInfo ns = new NamespaceInfo();
            ClassInfo cls = new ClassInfo();
            ns.NestedValues.Add(cls);

            if (options.Fields.Any(x => x.TypeName == "DateTime"))
                ns.OuterUsing.Add("System");
            if (options.Fields.Any(x => x.Collection ))
                ns.OuterUsing.Add("System.Collections.Generic");
            
            if (options.Fields.Any(x => x.Collection))
                ns.OuterUsing.Add("Newtonsoft.Json");

            ns.Name = project.DefaultNamespace + ".Entities";

            if (options.BaseClass == "NamedBaseEntity")
            {
                ns.InnerUsing.Add("MosKs.Core.Entities.Base");
            }
            else
            {
                ns.InnerUsing.Add("B4.DataAccess");
            }

            if (options.AcceptFiles)
            {
                ns.InnerUsing.Add("B4.Modules.FileStorage");
            }

            if (!string.IsNullOrEmpty(options.DisplayName))
                cls.Summary = options.DisplayName;

            cls.Name = options.ClassName;
            cls.BaseClass = options.BaseClass;

            if (options.Stateful)
            {
                ns.InnerUsing.Add("B4.Modules.States");
                cls.Interfaces.Add("IStatefulEntity");
                _knownTypes.Add("IStatefulEntity");
            }

            if (options.Signable)
            {
                ns.InnerUsing.Add("B4.Modules.DigitalSignature");
                ns.InnerUsing.Add("B4.Modules.DigitalSignature.Attributes");
                cls.Interfaces.Add("ISignableEntity");
                _knownTypes.Add("ISignableEntity");
            }

            foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
            {
                if (options.BaseClass == "NamedBaseEntity" && field.FieldName == "Name")
                    continue;

                var pi = (new PropertyInfo()).Public.Virtual.Auto.Get().Set();

                if (!string.IsNullOrEmpty(field.Comment))
                    pi.Summary = field.Comment;

                if (field.Collection)
                    pi.Attributes.Add("JsonIgnore");

                pi.Name = field.FieldName;
                pi.Type = field.FullTypeName;

                if (options.Signable && pi.Attributes.Count == 0)
                    pi.Attributes.Add("DigitalSignature(\"{0}\")".R(pi.Name));

                cls.AddProperty(pi);

                if (!field.IsBasicType() || field.Enum)
                {
                    var fieldSpace = GetTypeNamespace(field.TypeName);
                    if (field.Enum && fieldSpace == "" && !ns.NestedValues.Any(x => x is EnumInfo && x.Name == field.TypeName))
                    {
                        var enumType = new EnumInfo() { Name = field.TypeName, BaseType = "byte" };
                        enumType.Values.Add(new EnumValue { Name = "First", Value = 10 });
                        enumType.Values.Add(new EnumValue { Name = "Second", Value = 20 });

                        ns.NestedValues.Insert(0, enumType);
                    }
                    else
                        ns.InnerUsing.AddDistinct(fieldSpace);
                }

                if ((!field.IsBasicType() || field.Enum) && field.TypeName != field.FieldName)
                    _knownTypes.Add(field.TypeName);
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

                if (!field.IsBasicType())
                {
                    ns.InnerUsing.AddDistinct(GetTypeNamespace(field.TypeName));
                    _knownTypes.Add(field.TypeName);
                }
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

                _knownTypes.Add(field.TypeName);
            }
            
            // Добавить ссылку на View в базовую сущность
            var baseEntity = options.ClassName.Substring(0, options.ClassName.Length - 4);
            if (options.ClassName.EndsWith("View") && options.Fields.Any(f => f.FieldName == baseEntity && f.TypeName == baseEntity))
            {
                fragments.AddLines(baseEntity + ".cs", this, new List<string> { 
                "    [JsonIgnore]\n    public virtual {0} AggregatedInfo { get; set; }".R(options.ClassName)});

                fragments.AddLines(baseEntity + "Map.cs", this, new List<string> { 
                "    OneToOne(x => x.AggregatedInfo, map => {{ map.PropertyReference(typeof({0}).GetProperty(\"{0}\")); }});".R(baseEntity)});
            }

            file.Name = options.ClassName + ".cs";
            file.Path = "Entities\\" + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate(0).ToList();

            return files;
        }
    }
}
