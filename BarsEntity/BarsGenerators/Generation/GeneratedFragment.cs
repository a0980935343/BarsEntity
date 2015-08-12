using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.BarsGenerators
{
    public class GeneratedFragment
    {
        public IBarsGenerator Generator;

        public List<string> Lines = new List<string>();

        public List<string> GetStringList()
        {
            var result = new List<string>();

            if (Generator != null)
                result.Add("///   " + Generator.GetType().Name);

            result.AddRange(Lines);
            result.Add("");

            return result;
        }
    }
}
