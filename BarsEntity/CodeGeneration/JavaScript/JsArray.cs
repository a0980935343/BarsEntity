using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public class JsArray : JsProperty
    {
        public JsArray()
        {
            Inline = false;
        }

        public static JsArray FromArray(object[] array)
        {
            var jsArray = new JsArray();

            foreach (object item in array)
            {
                if (item is JsProperty)
                {
                    jsArray.Add((JsProperty)item);
                }
                if (item is JsStyle && (JsStyle)item == JsStyle.Inline)
                {
                    jsArray.Inline = true;
                }
                if (item is string) jsArray.Add((string)item); else
                if (item is bool) jsArray.Add((bool)item); else
                if (item is int) jsArray.AddScalar(((int)item).ToString());
                else
                {
                    if (item is Array)
                        jsArray.Add(JsArray.FromArray((object[])item));
                    if (item.GetType().IsAnonymous())
                        jsArray.Add(JsObject.FormObject(item));
                }
            }

            return jsArray;
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

        public void Add(long value)
        {
            Values.Add(new JsScalar { Value = value.ToString() });
        }

        public void Add(bool value)
        {
            Values.Add(new JsScalar { Value = value.ToString().ToLower() });
        }

        public void Add(string value)
        {
            Values.Add(new JsScalar { Value = value.Q("'") });
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

                _result.Add("{0}: [{1}]".R(Name, string.Join(", ", props)).Ind(indent));
            }
            else
            {
                if (Values.Any())
                {
                    var indentDelta = Values.All(x => x.Inline) ? 1 : 0;

                    _result.Add((Name + ": [").Ind(indent));
                    for (int i = 0; i < Values.Count; i++)
                    {
                        var prop = Values[i];

                        prop.Name = "x";
                        var pr = prop.Draw(indent + indentDelta);
                        pr[0] = "".Ind(indent + indentDelta) + pr[0].Substring((indent + indentDelta) * 4 + 3);

                        if (prop != Values.Last())
                            pr[pr.Count - 1] = pr[pr.Count - 1] + ",";

                        if (i == 0 && !prop.Inline)
                        {
                            pr.RemoveAt(0);
                            _result[_result.Count - 1] = _result[_result.Count - 1] + "{";
                        }
                        _result.AddRange(pr);
                    }
                    if (Values.Any(x => !x.Inline))
                        _result[_result.Count - 1] = _result[_result.Count - 1] + "]";
                    else
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
}
