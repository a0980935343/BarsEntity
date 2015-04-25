using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
    public class JsObject : JsProperty
    {
        public JsObject()
        {
            Inline = false;
        }

        public List<JsProperty> Properties = new List<JsProperty>();

        public void Add(JsProperty prop)
        {
            Properties.Add(prop);
        }

        public void AddScalar(string name, string value)
        {
            Properties.Add(new JsScalar { Name = name, Value = value });
        }

        public void AddBoolean(string name, bool value)
        {
            Properties.Add(new JsScalar { Name = name, Value = value.ToString().ToLower() });
        }

        public void AddString(string name, string value)
        {
            Properties.Add(new JsScalar { Name = name, Value = value.Q("'") });
        }

        public void AddLocal(string name, string value)
        {
            Properties.Add(new JsScalar { Name = name, Value = "lc('{0}')".F(value) });
        }

        public override List<string> Draw(int indent)
        {
            _result.Clear();
            if (Inline)
            {
                List<string> props = new List<string>();
                foreach (JsProperty prop in Properties)
                {
                    prop.Inline = true;
                    props.Add(prop.Draw(0).First());
                }
                _result.Add("{0}: {{{2}{1}{2}}}".F(Name, string.Join(", ", props), props.Any() ? " " : "").Ind(indent));
            }
            else
            {
                if (Properties.Any())
                {
                    _result.Add((Name + ": {").Ind(indent));
                    foreach (var prop in Properties)
                    {
                        var pr = prop.Draw(indent + 1);

                        if (prop != Properties.Last())
                            pr[pr.Count - 1] = pr[pr.Count - 1] + ",";

                        _result.AddRange(pr);
                    }
                    _result.Add("}".Ind(indent));
                }
                else
                    _result.Add((Name + ": {}").Ind(indent));
            }
            return _result;
        }
    }
}
