using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class StatefulEntitiesManifestGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options)
        {
            base.Generate(project, options);

            if (!FileExists("Domain\\StatefulEntitiesManifest.cs"))
            {
                CheckFolder("Domain");

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

                cls.AddField(classInfo);

                var getAllInfo = new MethodInfo { Name = "GetAllInfo", Type = "IEnumerable<StatefulEntityInfo>" };
                getAllInfo.Body.Add("return new[]{{ {0} }};".F(options.ClassName));
                cls.AddMethod(getAllInfo);

                ns.NestedValues.Add(cls);

                CreateFile("Domain\\StatefulEntitiesManifest.cs", ns.ToString());

                DontForget.Add("Module.cs/Module/Install");
                DontForget.Add("Container.Register(Component.For<IStatefulEntitiesManifest>().ImplementedBy<StatefulEntitiesManifest>().LifestyleTransient());");
            }
            else
            {
                DontForget.Add("Domain/StatefulEntitiesManifest.cs");

                var field = (new FieldInfo {
                    Name = options.ClassName,
                    Type = "StatefulEntityInfo",
                    Value = "new StatefulEntityInfo(\"{0}\", \"{1}\".Localize(), typeof({2}))".F(options.ClassFullName, options.DisplayName, options.ClassName)
                }).Public.Static.ReadOnly.Generate(0).First();

                DontForget.Add(field);
            }
        }
    }
}
