using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Barsix.BarsEntity.CodeGeneration
{
    public abstract class JsProperty
    {
        protected List<string> _result = new List<string>();

        public JsProperty Parent = null;

        public string Name;

        public bool Inline = true;

        public abstract List<string> Draw(int indent);
    }

    public class JsScalar : JsProperty
    {
        public string Value;

        public override List<string> Draw(int indent)
        {
            if (Name == "")
                return new List<string> { Value.Ind(indent) };
            else
                return new List<string> { (Name + ": " + Value).Ind(indent) };
        }

        public static JsScalar New(string name, string value) { return new JsScalar() { Name = name, Value = value }; }
        public static JsScalar String(string name, string value) { return new JsScalar() { Name = name, Value = value.Q("'") }; }
        public static JsScalar Boolean(string name, bool value) { return new JsScalar() { Name = name, Value = value.ToString().ToLower() }; }
        public static JsScalar Local(string name, string value) { return new JsScalar() { Name = name, Value = "lc('{0}')".F(value) }; }
        public static JsScalar Number(string name, long value) { return new JsScalar() { Name = name, Value = value.ToString() }; }
    }

    public class JsFunction : JsProperty
    {
        public string Params;

        public List<object> Body = new List<object>();

        public JsFunction(){ Inline = false; }

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

    public class JsInstance : JsFunctionCall
    {
        public JsInstance()
        {
            Instance = true;
        }
    }

    public class JsFunctionCall : JsProperty
    {
        public string Function;

        protected bool Instance;

        public List<JsProperty> Params = new List<JsProperty>();

        public override List<string> Draw(int indent)
        {
            _result.Clear();

            string left = "";

            if (Name == "return")
                left = Name + " ";
            else
                if (!string.IsNullOrEmpty(Name))
                {
                    if (Parent is JsFunction)
                        left = Name + " = ";
                    else
                        left = Name + ": ";
                }


            if (!Params.Any())
                _result.Add((left + (Instance ? "new " : "") + Function + "()").Ind(indent));
            else
            {
                string row = left + (Instance ? "new " : "") + Function + "(";

                foreach (var param in Params)
                {
                    param.Name = "x";
                    if (param.Inline)
                    {
                        row += param.Draw(0).First().Substring(3);
                    }
                    else
                    {
                        bool shift = param is JsObject || param is JsArray || param is JsFunctionCall;
                        var pr = param.Draw(indent + (!shift ? 1 : 0));

                        row = row + pr.First().Substring(indent * 4 + (!shift ? 4 : 0)+ 3);

                        if (pr.Count > 1)
                        {
                            pr.RemoveAt(0);

                            _result.Add(row.Ind(indent));

                            row = pr.Last().Trim();
                            pr.RemoveAt(pr.Count - 1);

                            _result.AddRange(pr);
                        }

                    }
                    if (param != Params.Last())
                        row += ", ";
                }

                row = (row + ")").Ind(indent);

                _result.Add(row);
            }
            return _result;
        }
    }

    public class JsArray : JsProperty
    {
        public JsArray()
        {
            Inline = false;
        }

        public List<JsProperty> Values = new List<JsProperty>();

        public void Add(JsProperty prop)
        {
            Values.Add(prop);
        }

        public void AddScalar(string value)
        {
            Values.Add(new JsScalar { Value = value });
        }

        public void AddBoolean(bool value)
        {
            Values.Add(new JsScalar { Value = value.ToString().ToLower() });
        }

        public void AddString(string value)
        {
            Values.Add(new JsScalar { Value = value.Q("'") });
        }

        public void AddLocal(string value)
        {
            Values.Add(new JsScalar { Value = "lc('{0}')".F(value) });
        }

        public override List<string> Draw(int indent)
        {
            _result.Clear();
            if (Inline)
            {
                List<string> props = new List<string>();
                foreach (JsProperty prop in Values)
                {
                    prop.Name = "x";
                    props.Add(prop.Draw(0).First().Substring(3));
                }

                _result.Add("{0}: [{1}]".F(Name, string.Join(", ", props)).Ind(indent));
            }
            else
            {
                if (Values.Any())
                {
                    _result.Add((Name + ": [").Ind(indent));
                    foreach (var prop in Values)
                    {
                        prop.Name = "x";
                        var pr = prop.Draw(indent + 1);
                        pr[0] = "".Ind(indent + 1) + pr[0].Substring(indent * 4 + 7);

                        if (prop != Values.Last())
                            pr[pr.Count - 1] = pr[pr.Count - 1] + ",";

                        _result.AddRange(pr);
                    }
                    _result.Add("]".Ind(indent));
                }
                else
                {
                    _result.Add((Name + ": []").Ind(indent));
                }
            }
            return _result;
        }
    }

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
                _result.Add("{0}: {{{2}{1}{2}}}".F(Name, string.Join(", ", props), props.Any() ? " ": "").Ind(indent));
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
