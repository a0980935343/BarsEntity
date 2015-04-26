﻿using System;
using System.Collections.Generic;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class NavigationGenerator : BaseBarsGenerator
    {
        public override GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            var file = base.Generate(project, options, fragments);
            var nav = options.Navigation;

            fragments.AddLines("NavigationProvider.cs", this, new List<string> { 
                "root.Add(\"{0}\").Add(\"{1}\", \"#{2}\", \"modules/{3}\").WithIcon(\"content/modules/MosKs/icons64/meterUnit.png\"){4};"
                .F(nav.Root, nav.Name, nav.Anchor, _project.DefaultNamespace.Substring(5) + "." + options.ClassName, nav.MapPermission ? ".AddRequiredPermission(\"{0}\")".F(options.Permission.Prefix) : "")});

            return null;
        }
    }
}
