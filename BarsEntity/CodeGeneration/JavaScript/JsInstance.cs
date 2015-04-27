using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
