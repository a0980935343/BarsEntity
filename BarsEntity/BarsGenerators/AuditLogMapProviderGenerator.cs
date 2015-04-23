using System.Collections.Generic;
using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class AuditLogMapProviderGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            base.Generate(project, options, fragments);

            if (!FileExists("AuditLogMapProvider.cs"))
            {
                var ns = new NamespaceInfo { Name = project.Name };
                ns.InnerUsing.Add("B4.Modules.NHibernateChangeLog");
                ns.InnerUsing.Add("Map");

                var cls = new ClassInfo { Name = "AuditLogMapProvider" };
                cls.Interfaces.Add("IAuditLogMapProvider");

                var init = new MethodInfo { Name = "Init", Type = "void", Params = "IAuditLogMapContainer container" };
                init.Body.Add("container.Add<{0}LogMap>();".F(options.ClassName));
                cls.AddMethod(init);

                ns.NestedValues.Add(cls);

                CreateFile("AuditLogMapProvider.cs", ns.ToString());

                fragments.AddLines("Module.cs", this, new List<string> { "Container.Register(Component.For<IAuditLogMapProvider>().ImplementedBy<AuditLogMapProvider>().LifestyleTransient());"});
            }
            else
            {
                fragments.AddLines("AuditLogMapProvider.cs", this, new List<string>{ "container.Add<{0}LogMap>();".F(options.ClassName)});
            }
        }
    }
}
