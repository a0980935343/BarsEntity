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
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files.First();

            var ns = new NamespaceInfo();
            var cls = new ClassInfo();
            ns.NestedValues.Add(cls);
            ns.Name = project.DefaultNamespace + ".Map";

            if (options.BaseClass == "NamedBaseEntity")
            {
                ns.InnerUsing.Add("MosKs.Core.Map.Base");
            }
            else
            {
                ns.InnerUsing.Add("B4.DataAccess.ByCode");
            }
            ns.InnerUsing.Add("Entities");
            
            cls.Name = options.ClassName + "Map";
            cls.BaseClass = "{0}<{1}>".R(options.MapBaseClass(), options.ClassName);

            _knownTypes.Clear();
            _knownTypes.Add(options.ClassName);
            _knownTypes.Add(options.BaseClass);
            _knownTypes.Add("IList");
            _knownTypes.Add("ReferenceMapConfig");
            _knownTypes.Add(options.BaseClass + "Map");
            _knownTypes.Add("SubclassMap");
            _knownTypes.Add("BaseJoinedSubclassMap");

            var ctor = new MethodInfo() 
            { 
                IsConstructor = true,
                Name = cls.Name
            };
            
            if (options.BaseClass == "NamedBaseEntity")
            {
                var field = options.Fields.Single(x => x.FieldName == "Name");
                ctor.SignatureParams = "base(\"{0}\", {1}, {2})".R(options.TableName, field.Nullable.ToString().ToLower(), field.Length == 0 ? 100 : field.Length);
            }
            else
                ctor.SignatureParams = "base(\"{0}\"{1})".R(options.TableName, options.MapBaseClass() == "BaseJoinedSubclassMap" ? ", \"ID\"" : string.Empty);


            if (options.MapBaseClass() == "SubclassMap")
            {
                ctor.Body.Add("DiscriminatorValue(\"{0}\");\n".R(options.DiscriminatorValue));
            }

            foreach (var field in options.Fields.Where(x => !x.Collection && !x.TypeName.EndsWith("View")))
            {
                if (options.BaseClass == "NamedBaseEntity" && field.FieldName == "Name")
                    continue;

                if (field.IsBasicType())
                    ctor.Body.Add("Map(x => x.{0}, \"{1}\", {2}{3});".R(field.FieldName, field.ColumnName, (!field.Nullable).ToString().ToLower(), field.TypeName == "string" && field.Length >0 ? ", " + field.Length.ToString() : ""));
                else
                {
                    ctor.Body.Add("References(x => x.{0}, \"{1}\", ReferenceMapConfig.{2}Fetch);".R(field.FieldName, field.ColumnName, field.Nullable ? "" : "NotNullAnd"));
                }
            }


            if (options.Fields.Any(x => x.Collection))
                ctor.Body.Add("");
            foreach (var field in options.Fields.Where(x => x.Collection))
            {
                ctor.Body.Add("HasMany(x => x.{0}, \"{1}\", ReferenceMapConfig.CascadeDelete);".R(field.FieldName, field.ColumnName));
            }
            

            if (options.Fields.Any(x => x.TypeName.EndsWith("View")))
                ctor.Body.Add("");
            foreach (var field in options.Fields.Where(x => x.TypeName.EndsWith("View")))
            {
                ctor.Body.Add("OneToOne(x => x.{0}, map => {{ map.PropertyReference(typeof({1}).GetProperty(\"{2}\")); }});".R(field.FieldName, field.TypeName, options.ClassName));
                _knownTypes.Add(field.TypeName);
            }

            cls.AddMethod(ctor);

            file.Name = options.ClassName + "Map.cs";
            file.Path = (Directory.Exists(Path.Combine(project.RootFolder, "Map")) ? "Map\\" : "Maps\\") + (options.IsDictionary ? "Dict\\" : (!string.IsNullOrWhiteSpace(options.Subfolder) ? options.Subfolder : ""));
            file.Body = ns.Generate();
            return files;
        }
    }
}
