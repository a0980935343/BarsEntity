using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class MapGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            var ns = new NamespaceInfo();
            var cls = new ClassInfo();
            ns.NestedValues.Add(cls);
            ns.Name = "{0}.Map".F(_project.DefaultNamespace);
            
            if (options.BaseClass == "BaseEntity")
            {
                ns.InnerUsing.Add("B4.DataAccess");
            }
            else if (options.BaseClass == "NamedBaseEntity")
            {
                ns.InnerUsing.Add("MosKs.Core.Map.Base");
            }

            ns.InnerUsing.Add("B4.DataAccess.ByCode");
            ns.InnerUsing.Add("Entities");
            
            cls.Name = "{0}Map".F(options.ClassName);
            cls.BaseClass = "{2}{0}Map<{1}>".F(options.BaseClass, options.ClassName, _project.DefaultNamespace.StartsWith("Bars.B4.") ? "" : "Bars.MosKs.Core.Map.Base.");

            _knownTypes.Clear();
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add(options.BaseClass);
            _knownTypes.Add("IList");
            _knownTypes.Add("ReferenceMapConfig");

            var ctor = new MethodInfo() 
            { 
                IsConstructor = true,
                Name = cls.Name
            };
            
            if (options.BaseClass == "NamedBaseEntity")
            {
                var field = options.Fields.Single(x => x.FieldName == "Name");
                ctor.SignatureParams = "base(\"{0}\", {1}, {2})".F(options.TableName, field.Nullable.ToString().ToLower(), field.Length == 0 ? 100 : field.Length);
            }
            else
                ctor.SignatureParams = "base(\"{0}\")".F(options.TableName);
            

            foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
            {
                if (options.BaseClass == "NamedBaseEntity" && field.FieldName == "Name")
                    continue;

                if (field.IsBasicType())
                    ctor.Body.Add("Map(x => x.{0}, \"{1}\", {2}{3});".F(field.FieldName, field.ColumnName, (!field.Nullable).ToString().ToLower(), field.TypeName == "string" && field.Length >0 ? ", " + field.Length.ToString() : ""));
                else
                {
                    ctor.Body.Add("References(x => x.{0}, \"{1}\", ReferenceMapConfig.{2}Fetch);".F(field.FieldName, field.ColumnName, field.Nullable ? "" : "NotNullAnd"));
                }
            }


            if (options.Fields.Any(x => x.Collection))
                ctor.Body.Add("");
            foreach (var field in options.Fields.Where(x => x.Collection))
            {
                ctor.Body.Add("HasMany(x => x.{0}, \"{1}\", ReferenceMapConfig.CascadeDelete);".F(field.FieldName, field.ColumnName));
            }
            

            if (options.Fields.Any(x => x.TypeName.EndsWith("View")))
                ctor.Body.Add("");
            foreach (var field in options.Fields.Where(x => x.TypeName.EndsWith("View")))
            {
                ctor.Body.Add("OneToOne(x => x.{0}, map => {{ map.PropertyReference(typeof({1}).GetProperty(\"{2}\")); }});".F(field.FieldName, field.TypeName, options.ClassName));
                _knownTypes.Add(field.TypeName);
            }

            cls.AddMethod(ctor);

            file.Name = options.ClassName + "Map.cs";
            file.Path = (Directory.Exists(Path.Combine(_project.RootFolder, "Map")) ? "Map\\" : "Maps\\") + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate();
            return file;
        }
    }
}
