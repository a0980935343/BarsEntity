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

            var navigation = new GeneratedFragment
            {
                FileName = "NavigationProvider.cs",
                InsertToFile = true,
                InsertClass = "public class NavigationProvider",
                InsertMethod = "public void Init(MenuItem root)",
                Generator = this
            };
            string menuItem = "root.Add(\"{0}\").Add(\"{1}\", \"{2}{3}\", \"modules/{4}\")"
                .R(nav.Root,
                    nav.Name,
                    options.Profile.HrefPrefix,
                    nav.Anchor,
                    project.DefaultNamespace.Substring(5) + "." + options.ClassName);

            if (nav.MapPermission)
                menuItem = menuItem + ".AddRequiredPermission(\"{0}.View\")".R(options.Permission.Prefix);

            if (!string.IsNullOrWhiteSpace(options.Profile.IconPath))
                menuItem = menuItem + ".WithIcon(\"{0}\")".R(options.Profile.IconPath);

            menuItem = menuItem + ";";

            navigation.Lines.Add(menuItem);

            fragments.Add("Module.cs", navigation);

            return null;
        }
    }
}
