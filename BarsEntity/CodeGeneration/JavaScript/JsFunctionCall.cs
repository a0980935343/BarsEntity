using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public class JsFunctionCall : JsProperty
    {
        public string Function;

        protected bool Instance;

        public List<JsProperty> Params = new List<JsProperty>();

        public JsFunctionCall()
        { 
        }

        public JsFunctionCall(string func, IEnumerable<object> @params)
        {
            Function = func;
            foreach (var param in @params)
            {
                AddParam(param);
            }
        }
        
        public JsFunctionCall AddParam(object prop)
        {
            if (prop is JsProperty)
            {
                Params.Add(prop as JsProperty);
            }
            else
            if (prop is string)
            {
                Params.Add(JsScalar.String((string)prop));
            }
            else
            if (prop is int)
            {
                Params.Add(JsScalar.Number((int)prop));
            }
            else
            if (prop is bool)
            {
                Params.Add(JsScalar.Boolean((bool)prop));
            }
            else
                Params.Add(prop.ToJs());
            return this;
        }

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
                        bool shift = param is JsObject || param is JsArray || param is JsFunctionCall || param is JsFunction;
                        var pr = param.Draw(indent + (!shift ? 1 : 0));

                        row = row + pr.First().Substring(indent * 4 + (!shift ? 4 : 0) + 3);

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
}
