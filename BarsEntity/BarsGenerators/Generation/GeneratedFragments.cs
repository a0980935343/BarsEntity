using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.BarsGenerators
{
    public class GeneratedFragments : IEnumerable<KeyValuePair<string, List<GeneratedFragment>>>
    {
        private Dictionary<string, List<GeneratedFragment>> _lines = new Dictionary<string, List<GeneratedFragment>>();

        public void Add(string fileName, GeneratedFragment fragment)
        {
            if (!_lines.ContainsKey(fileName))
                _lines.Add(fileName, new List<GeneratedFragment>());

            _lines[fileName].Add(fragment);
        }

        public void AddLines(string fileName, IBarsGenerator source, List<string> lines)
        {
            if (!_lines.ContainsKey(fileName))
                _lines.Add(fileName, new List<GeneratedFragment>());

            _lines[fileName].Add(new GeneratedFragment { Generator = source, Lines = lines });
        }

        public List<string> ToList()
        {
            List<string> result = new List<string>();

            foreach (var file in _lines)
            {
                result.Add("///             " + file.Key);
                foreach (var fragment in file.Value)
                {
                    result.AddRange(fragment.GetStringList());
                }
                result.Add("///---------------------------------------------------------------------------");
                result.Add("");
            }
            return result;
        }

        public IEnumerator<KeyValuePair<string, List<GeneratedFragment>>> GetEnumerator()
        {
            return _lines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _lines.GetEnumerator();
        }
    }
}
