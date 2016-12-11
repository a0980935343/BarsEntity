using System.Collections.Generic;
using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;
    using Types;

    public interface IBarsGenerator
    {
        List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments);

        /// <summary>
        /// Типы, которые надо подсветить в редакторе кода
        /// </summary>
        IEnumerable<string> KnownTypes { get; }

        /// <summary>
        /// Сущности/перечисления в проекте/решении
        /// </summary>
        List<string> CodeItemNameList { get; set; }

        /// <summary>
        /// Запрос дополнительных классов для генератора
        /// </summary>
        ClassRequest ClassRequest { get; }

        /// <summary>
        /// Полученные по запросу елементы кода
        /// </summary>
        Dictionary<string, CodeClass> Classes { set; }
    }
}
