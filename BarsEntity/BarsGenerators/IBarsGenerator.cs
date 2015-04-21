using System.Collections.Generic;
using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;

    public interface IBarsGenerator
    {
        void Generate(Project project, EntityOptions options);
    }
}
