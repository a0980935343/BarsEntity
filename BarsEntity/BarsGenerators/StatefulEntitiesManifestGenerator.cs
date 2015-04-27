using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class StatefulEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            if (!File.Exists(Path.Combine(_project.RootFolder, "Domain\\StatefulEntitiesManifest.cs")))
            {
                var ns = new NamespaceInfo { Name = "{0}.Domain".F(_project.DefaultNamespace) };
                ns.InnerUsing.Add("B4");
                ns.InnerUsing.Add("B4.Modules.States");
                ns.InnerUsing.Add("Entities");

                ns.OuterUsing.Add("System.Collections.Generic");

                var cls = new ClassInfo { Name = "StatefulEntitiesManifest" };
                cls.Interfaces.Add("IStatefulEntitiesManifest");

                var classInfo = (new FieldInfo
                {
                    Name = options.ClassName,
                    Type = "StatefulEntityInfo",
                    Value = "new StatefulEntityInfo(\"{0}\", \"{1}\".Localize(), typeof({2}))".F(options.ClassFullName, options.DisplayName, options.ClassName)
                }
                ).Public.Static.ReadOnly;

                _knownTypes.Clear();
                _knownTypes.Add(cls.Name);
                _knownTypes.AddRange(cls.Interfaces);
                _knownTypes.Add("StatefulEntityInfo");
                _knownTypes.Add("IEnumerable");

                cls.AddField(classInfo);

                var getAllInfo = new MethodInfo { Name = "GetAllInfo", Type = "IEnumerable<StatefulEntityInfo>" };
                getAllInfo.Body.Add("return new[]{{ {0} }};".F(options.ClassName));
                cls.AddMethod(getAllInfo);

                ns.NestedValues.Add(cls);
                
                fragments.AddLines("Module.cs", this, new List<string> { 
                    "Container.Register(Component.For<IStatefulEntitiesManifest>().ImplementedBy<StatefulEntitiesManifest>().LifestyleTransient());"});

                file.Name = "StatefulEntitiesManifest.cs";
                file.Path = "Domain";
                file.Body = ns.Generate();
                return file;
            }
            else
            {
                var field = (new FieldInfo {
                    Name = options.ClassName,
                    Type = "StatefulEntityInfo",
                    Value = "new StatefulEntityInfo(\"{0}\", \"{1}\".Localize(), typeof({2}))".F(options.ClassFullName, options.DisplayName, options.ClassName)
                }).Public.Static.ReadOnly.Generate(0).First();

                fragments.AddLines("Domain/StatefulEntitiesManifest.cs", this, new List<string> { field });

                file.Body.Add("/**");
                file.Body.Add(" *     Файл Domain/StatefulEntitiesManifest.cs уже есть в проекте");
                file.Body.Add(" *     Вставьте в манифест строки ниже для регистрации новой сущности");
                file.Body.Add(" */");
                file.Body.Add("");
                file.Body.AddRange(fragments.First(x => x.Key == "Domain/StatefulEntitiesManifest.cs").Value);
                return file;
            }
        }
    }
}
