using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class AuditLogMapGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);
            var ns = new NamespaceInfo() { Name = "{0}.Map".F(project.Name) };
            var cls = new ClassInfo
            {
                Name = "{0}LogMap".F(options.ClassName),
                BaseClass = "AuditLogMap<{0}>".F(options.ClassName)
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

            ctor.Body.Add("Name(\"{0}\");".F(options.DisplayName));
            ctor.Body.Add("Description(x => string.Format(\"{0} №{{0}}\", x.Id));".F(options.DisplayName));
            ctor.Body.Add("");

            foreach (var field in options.Fields)
            {
                ctor.Body.Add("MapProperty(x => x.{0}, \"{1}\", \"{2}\");".F(field.FieldName, field.ColumnName.EndsWith("_ID") ? field.ColumnName.Substring(0, field.ColumnName.Length-3) : field.ColumnName, field.DisplayName));
            }
            cls.AddMethod(ctor);

            file.Name = options.ClassName + "LogMap.cs";
            file.Path = "Map\\Log\\";
            file.Body = ns.Generate();
            return file;
        }
    }
}
