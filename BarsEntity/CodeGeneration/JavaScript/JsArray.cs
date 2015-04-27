using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                if (item is string) jsArray.AddString((string)item); else
                if (item is bool) jsArray.AddBoolean((bool)item); else
                if (item is int) jsArray.AddScalar(((int)item).ToString());
                else
                {
                    if (item is Array)
                        jsArray.Add(JsArray.FromArray((object[])item));
                    if (item.GetType().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0 &&
                        item.GetType().FullName.Contains("AnonymousType"))
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
}
