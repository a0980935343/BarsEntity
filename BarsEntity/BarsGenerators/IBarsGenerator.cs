using System.Collections.Generic;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public interface IBarsGenerator
    {
        GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments);

        IEnumerable<string> KnownTypes { get; }

        List<string> ClassList { get; set; }
    }
}
