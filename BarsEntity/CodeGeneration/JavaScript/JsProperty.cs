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
}
