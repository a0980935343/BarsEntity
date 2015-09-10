using System.Collections.Generic;
using System.IO;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class AuditLogMapProviderGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var files = base.Generate(project, options, fragments);
            var file = files[0];

            if (!File.Exists(Path.Combine(project.RootFolder, "AuditLogMapProvider.cs")))
            {
                var ns = new NamespaceInfo { Name = project.DefaultNamespace };
                ns.InnerUsing.Add("B4.Modules.NHibernateChangeLog");
                ns.InnerUsing.Add("Map");

                var cls = new ClassInfo { Name = "AuditLogMapProvider" };
                cls.Interfaces.Add("IAuditLogMapProvider");

                var init = new MethodInfo { Name = "Init", Type = "void", Params = "IAuditLogMapContainer container" };
                init.Body.Add("container.Add<{0}LogMap>();".R(options.ClassName));
                cls.AddMethod(init);

                ns.NestedValues.Add(cls);
                
                fragments.AddLines("Module.cs", this, new List<string> { "Container.Register(Component.For<IAuditLogMapProvider>().ImplementedBy<AuditLogMapProvider>().LifestyleTransient());"});

                file.Name = "AuditLogMapProvider.cs";
                file.Body = ns.Generate();
                return files;
            }
            else
            {
                fragments.AddLines("AuditLogMapProvider.cs", this, new List<string>{ "container.Add<{0}LogMap>();".R(options.ClassName)});
                return null;
            }
        }
    }
}
