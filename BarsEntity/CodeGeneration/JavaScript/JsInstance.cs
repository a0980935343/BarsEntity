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
    }
}
