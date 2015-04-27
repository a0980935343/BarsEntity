using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity
{
    using BarsGenerators;

    public class GeneratedFile
    {
        public string Name;

        public string Path;

        public List<string> Body = new List<string>();

        public Dictionary<string, object> Properties = new Dictionary<string,object>();

        public IBarsGenerator Generator;
    }
}
