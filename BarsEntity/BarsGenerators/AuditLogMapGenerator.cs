using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class AuditLogMapGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();
            var ns = new NamespaceInfo() { Name = project.DefaultNamespace + ".Map" };
            var cls = new ClassInfo
            {
                Name = options.ClassName + "LogMap",
                BaseClass = "AuditLogMap<{0}>".R(options.ClassName)
            };
            ns.NestedValues.Add(cls);
            ns.InnerUsing.Add("B4.Modules.NHibernateChangeLog");
            ns.InnerUsing.Add("Entities");

            var ctor = new MethodInfo()
            {
                IsConstructor = true,
                Name = cls.Name
            };

            _knownTypes.Clear();
            _knownTypes.Add(cls.Name);
            _knownTypes.Add("AuditLogMap");
            _knownTypes.Add(options.ClassName);

            ctor.Body.Add("Name(\"{0}\");".R(options.DisplayName));
            ctor.Body.Add("Description(x => string.Format(\"{0} №{{0}}\", x.Id));".F(options.DisplayName));
            ctor.Body.Add("");

            foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
            {
                if (field.IsBasicType())
                {
                    if (field.TypeName == "bool")
                        ctor.Body.Add("MapProperty(x => x.{0}, \"{1}\", \"{2}\", x => x ? \"Да\" : \"Нет\");".R(field.FieldName, field.ColumnName, field.DisplayName));
                    else
                        ctor.Body.Add("MapProperty(x => x.{0}, \"{1}\", \"{2}\");".R(field.FieldName, field.ColumnName, field.DisplayName, field.TypeName));
                }
                else
                {
                    ctor.Body.Add("MapProperty(x => x.{0}, \"{1}\", \"{2}\", x => x == null ? string.Empty : x.{3});".R(field.FieldName, field.ColumnName.Substring(0, field.ColumnName.Length - 3), field.DisplayName, field.TextProperty));
                }
            }
            cls.AddMethod(ctor);

            file.Name = options.ClassName + "LogMap.cs";
            file.Path = (Directory.Exists(Path.Combine(project.RootFolder, "Map")) ? "Map\\" : "Maps\\") + "Log\\";
            file.Body = ns.Generate();
            return files;
        }
    }
}
