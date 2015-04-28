using System;
using System.Collections.Generic;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public abstract class JsProperty
    {
        protected List<string> _result = new List<string>();

        public JsProperty Parent = null;

        public string Name;

        public bool Inline = true;

        public abstract List<string> Draw(int indent);
    }
}
