using System;
using System.Collections.Generic;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration.CSharp;

    public class NavigationGenerator : BaseBarsGenerator
    {
        public override List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var nav = options.Navigation;

            fragments.AddLines("NavigationProvider.cs", this, new List<string> { 
                "root.Add(\"{0}\").Add(\"{1}\", \"#{2}\", \"modules/{3}\").WithIcon(\"content/modules/MosKs/icons64/meterUnit.png\"){4};"
                .R(nav.Root, nav.Name, nav.Anchor, project.DefaultNamespace.Substring(5) + "." + options.ClassName, nav.MapPermission ? ".AddRequiredPermission(\"{0}\")".R(options.Permission.Prefix) : "")});

            return null;
        }
    }
}
