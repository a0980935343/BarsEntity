﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
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