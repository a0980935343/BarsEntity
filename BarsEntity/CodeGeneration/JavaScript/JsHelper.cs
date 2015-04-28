using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public static class JsHelper
    {
        public static JsProperty ToJs(this object obj)
        {
            if (obj is Array)
                return JsArray.FromArray((object[])obj);

            if (obj.GetType().IsAnonymous())
                return JsObject.FormObject(obj);

            return null;
        }
    }

    public enum JsStyle
    { 
        Inline
    }
}
