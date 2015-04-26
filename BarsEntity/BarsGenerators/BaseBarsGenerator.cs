using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;

    public class BaseBarsGenerator : IBarsGenerator
    {
        protected ProjectInfo _project;
        protected List<string> _knownTypes = new List<string>();

        public BaseBarsGenerator()
        {
            ClassList = new List<string>();
        }

        public virtual GeneratedFile Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            _project = project;
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
