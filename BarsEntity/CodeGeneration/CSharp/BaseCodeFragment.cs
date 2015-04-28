using System;
using System.Collections.Generic;

namespace Barsix.BarsEntity.CodeGeneration.CSharp
{
    public abstract class BaseCodeFragment
    {
        public string Name;

        public List<BaseCodeFragment> NestedValues = new List<BaseCodeFragment>();

        public abstract List<string> Generate(int indent);
    }

    public class CodeFragment : BaseCodeFragment
    {
        public string Access = "public";
        public bool IsStatic;
        public string Summary;

        public List<string> Attributes = new List<string>();
        
        public CodeFragment Public { get { Access = "public"; return this; } }
        public CodeFragment Protected { get { Access = "protected"; return this; } }
        public CodeFragment Private { get { Access = "private"; return this; } }
        public CodeFragment Static { get { IsStatic = true; return this; } }

        public override List<string> Generate(int indent)
        {
            throw new NotImplementedException();
        }
    }

    public class ClassCodeFragment : CodeFragment
    {
        public bool IsVirtual;
        public bool IsOverride;
        public string Type;
        
        public ClassCodeFragment Virtual { get { IsVirtual = true; return this; } }
        public ClassCodeFragment Override { get { IsOverride = true; return this; } }
    }
}
