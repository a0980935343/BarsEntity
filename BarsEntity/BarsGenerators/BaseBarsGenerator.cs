using System;
using System.Collections.Generic;
using System.Linq;

using EnvDTE;

namespace Barsix.BarsEntity.BarsGenerators
{
    using BarsOptions;
    using CodeGeneration;
    using Types;

    public class BaseBarsGenerator : IBarsGenerator
    {
        protected List<string> _knownTypes = new List<string>();

        public BaseBarsGenerator()
        {
            CodeItemNameList = new List<string>();
        }

        public virtual List<GeneratedFile> Generate(ProjectInfo project, EntityOptions options, GeneratedFragments fragments)
        {
            return new List<GeneratedFile> { new GeneratedFile() { Generator = this } };
        }

        public List<string> CodeItemNameList { get; set; }

        public IEnumerable<string> KnownTypes { get { return _knownTypes; } }

        protected string GetTypeNamespace(string type)
        { 
            string clsName = CodeItemNameList.FirstOrDefault(x => x.EndsWith("." + type));
            if (!string.IsNullOrEmpty(clsName))
                return clsName.Substring(0, clsName.Length - type.Length - 1);
            return "";
        }

        public virtual ClassRequest ClassRequest
        {
            get { return null; }
        }

        public Dictionary<string, CodeClass> Classes { protected get; set; }
    }
}
