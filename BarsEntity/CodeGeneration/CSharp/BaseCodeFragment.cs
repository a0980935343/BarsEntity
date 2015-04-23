﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.CodeGeneration
{
    public abstract class BaseCodeFragment
    {
        public string Name;

        public List<BaseCodeFragment> NestedValues = new List<BaseCodeFragment>();

        public abstract IList<string> Generate(int indent);
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

        public override IList<string> Generate(int indent)
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