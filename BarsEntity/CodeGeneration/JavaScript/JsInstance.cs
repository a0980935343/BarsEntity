using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public class JsInstance : JsFunctionCall
    {
        public JsInstance()
        {
            Instance = true;
        }

        public JsInstance(string func, params object[] @params)
            : this()
        {
            Function = func;
            foreach (var param in @params)
            {
                AddParam(param);
            }
        }
    }
}
