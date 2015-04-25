using System.Collections.Generic;
using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public interface IBarsGenerator
    {
        GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments);

        IEnumerable<string> KnownTypes { get; }
    }
}
