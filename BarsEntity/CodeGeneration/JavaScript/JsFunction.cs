using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public class JsFunction : JsProperty
    {
        public string Params;

        public List<object> Body = new List<object>();

        public JsFunction() { Inline = false; }

        public JsFunction(string @params, params object[] body) 
        {
            Params = @params;
            foreach (object line in body)
                Body.Add(line);

            Inline = false; 
        }

        public override List<string> Draw(int indent)
        {
            _result.Clear();
            string row = "";
            if (!string.IsNullOrEmpty(Name))
            {
                row = Name + ": ";
            }
            row = row + "function({0}){{".F(Params);

            _result.Add(row.Ind(indent));

            foreach (var instr in Body)
            {
                if (instr == null)
                {
                    continue;
                }
                if (instr is string)
                {
                    string line = instr.ToString();
                    _result.Add(line.Ind(indent + 1));
                }
                else
                {
                    var inlist = ((JsProperty)instr).Draw(indent + 1);
                    inlist[inlist.Count - 1] = inlist.Last() + ";";
                    _result.AddRange(inlist);
                }
            }
            _result.Add("}".Ind(indent));

            return _result;
        }

        public void Add(JsProperty prop)
        {
            prop.Parent = this;
            Body.Add(prop);
        }

        public void Add(string prop)
        {
            Body.Add(prop);
        }
    }
}
