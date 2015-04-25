using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class BaseBarsGenerator : IBarsGenerator
    {
        protected Project _project;
        protected EntityOptions _options;
        protected string _projectFolder;
        protected List<string> _knownTypes = new List<string>();

        public virtual GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            _project = project;
            _options = options;
            _projectFolder = project.RootFolder();
            return new GeneratedFile() { Generator = this };
        }

        public IEnumerable<string> KnownTypes { get { return _knownTypes; } }
    }
}
