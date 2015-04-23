using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
    using BarsGenerators;

    public class GeneratedFragments : IEnumerable<KeyValuePair<string, List<string>>>
    {
        private Dictionary<string, List<string>> _lines = new Dictionary<string,List<string>>();
        
        public void AddLines(string fileName, IBarsGenerator source, List<string> lines)
        {
            if (!_lines.ContainsKey(fileName))
                _lines.Add(fileName, new List<string>());

            _lines[fileName].Add("///   " + source.GetType().Name);
            _lines[fileName].AddRange(lines);
            _lines[fileName].Add("");
        }

        public List<string> ToList()
        {
            List<string> result = new List<string>();

            foreach (var file in _lines)
            {
                result.Add("///             " + file.Key);
                result.AddRange(file.Value);
                result.Add("///---------------------------------------------------------------------------");
                result.Add("");
            }
            return result;
        }

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _lines.GetEnumerator();
        }
    }
}
