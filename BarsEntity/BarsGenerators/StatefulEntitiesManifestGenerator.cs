﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class StatefulEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);

            if (!File.Exists(Path.Combine(_projectFolder, "Domain\\StatefulEntitiesManifest.cs")))
            {
                var ns = new NamespaceInfo { Name = "{0}.Domain".F(project.Name) };
                ns.InnerUsing.Add("Bars.B4");
                ns.InnerUsing.Add("Bars.B4.Modules.States");
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

                return null;
            }
        }
    }
}
