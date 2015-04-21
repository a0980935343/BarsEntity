using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class AuditLogMapProviderGenerator : BaseBarsGenerator
    {
        public override void Generate(Project project, EntityOptions options)
        {
            base.Generate(project, options);

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

                DontForget.Add("Module.cs/Module/Install");
                DontForget.Add("Container.Register(Component.For<IAuditLogMapProvider>().ImplementedBy<AuditLogMapProvider>().LifestyleTransient());");
            }
            else
            {
                DontForget.Add("AuditLogMapProvider.cs");
                DontForget.Add("container.Add<{0}LogMap>();".F(options.ClassName));
            }
        }
    }
}
