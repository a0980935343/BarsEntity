using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
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

        public void Add(string name, JsProperty prop)
        {
            prop.Name = name;
            Properties.Add(prop);
        }

        public void Add(string name, string str)
        {
            Properties.Add(new JsScalar { Name = name, Value = str.Q("'") });
        }

        public void Add(string name, long num)
        {
            Properties.Add(new JsScalar { Name = name, Value = num.ToString() });
        }

        public void Add(string name, bool flag)
        {
            Properties.Add(new JsScalar { Name = name, Value = flag.ToString().ToLower() });
        }

        public void AddScalar(string name, string value)
        {
            Properties.Add(new JsScalar { Name = name, Value = value });
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

        public static JsObject FormObject(object item)
        {
            JsObject jsObject = new JsObject();

            foreach (PropertyInfo prop in item.GetType().GetProperties().ToList())
            {
                var val = prop.GetValue(item);

                if (prop.Name.StartsWith("__"))
                {
                    if (prop.Name == "__name")
                        jsObject.Name = (string)val;
                    if (prop.Name == "__inline")
                        jsObject.Inline = (bool)val;
                    if (prop.Name == "__parent")
                        jsObject.Parent = (JsProperty)val;
                }
                else
                {
                    

                    if (val is JsProperty)
                    {
                        jsObject.Add(prop.Name, (JsProperty)val);
                    }
                    if (val is string) jsObject.Add(prop.Name, (string)val);
                    else
                    if (val is bool) jsObject.Add(prop.Name, (bool)val);
                    else
                    if (val is int) jsObject.AddScalar(prop.Name, ((int)val).ToString());
                    else
                    {
                        if (val is Array)
                        {
                            JsArray array = JsArray.FromArray((object[])val);
                            array.Name = prop.Name;
                            jsObject.Add(array);
                        }
                        if (val.GetType().IsAnonymous())
                        {
                            JsObject @object = JsObject.FormObject(val);
                            @object.Name = prop.Name;
                            jsObject.Add(@object);
                        }
                    }
                }
            }
            return jsObject;
        }
    }
}
