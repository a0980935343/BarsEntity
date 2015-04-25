using System;
using System.Collections.Generic;
using System.Linq;
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

        public BaseBarsGenerator()
        {
            ClassList = new List<string>();
        }

        public virtual GeneratedFile Generate(Project project, EntityOptions options, GeneratedFragments fragments)
        {
            _project = project;
            _options = options;
            _projectFolder = project.RootFolder();
            return new GeneratedFile() { Generator = this };
        }

        public List<string> ClassList { get; set; }

        public IEnumerable<string> KnownTypes { get { return _knownTypes; } }

        protected string GetTypeNamespace(string type)
        { 
            string clsName = ClassList.FirstOrDefault(x => x.EndsWith("." + type));
            if (!string.IsNullOrEmpty(clsName))
                return clsName.Substring(0, clsName.Length - type.Length - 1);
            return "";
        }


    }
}
