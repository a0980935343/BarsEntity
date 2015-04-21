using System;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class NavigationGenerator : BaseBarsGenerator
    {
        public override void Generate(EnvDTE.Project project, EntityOptions options)
        {
            base.Generate(project, options);
            var nav = options.Navigation;

            DontForget.Add("NavigationProvider.cs/NavigationProvider/Init");
            DontForget.Add("root.Add(\"{0}\").Add(\"{1}\", \"#{2}\", \"modules/{3}\").WithIcon(\"content/modules/MosKs/icons64/operator.png\"){4};"
                .F(nav.Root, nav.Name, nav.Anchor, project.Name.Substring(5) + "." + options.ClassName, nav.MapPermission ? ".AddRequiredPermission(\"{0}\")".F(options.Permission.Prefix) : ""));
        }
    }
}
