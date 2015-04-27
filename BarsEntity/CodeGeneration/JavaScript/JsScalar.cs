using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration.JavaScript
{
    public class JsScalar : JsProperty
    {
        public string Value;

        public override List<string> Draw(int indent)
        {
            if (Name == "")
                return new List<string> { Value.Ind(indent) };
            else
                return new List<string> { (Name + ": " + Value).Ind(indent) };
        }

        public static JsScalar New(string name, string value) { return new JsScalar() { Name = name, Value = value }; }
        public static JsScalar String(string name, string value) { return new JsScalar() { Name = name, Value = value.Q("'") }; }
        public static JsScalar Boolean(string name, bool value) { return new JsScalar() { Name = name, Value = value.ToString().ToLower() }; }
        public static JsScalar Local(string name, string value) { return new JsScalar() { Name = name, Value = "lc('{0}')".F(value) }; }
        public static JsScalar Number(string name, long value) { return new JsScalar() { Name = name, Value = value.ToString() }; }

        public static JsScalar New(string value) { return new JsScalar() { Value = value }; }
        public static JsScalar String(string value) { return new JsScalar() { Value = value.Q("'") }; }
        public static JsScalar Boolean(bool value) { return new JsScalar() { Value = value.ToString().ToLower() }; }
        public static JsScalar Local(string value) { return new JsScalar() { Value = "lc('{0}')".F(value) }; }
        public static JsScalar Number(long value) { return new JsScalar() { Value = value.ToString() }; }
    }
}
