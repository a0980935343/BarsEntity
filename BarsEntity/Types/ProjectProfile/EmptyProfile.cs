using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity
{
    public class EmptyProfile : ProjectProfileBase
    {
        public override string Name { get { return ""; } }

        public override string IconPath { get { return "\"baseIcon\""; } }
    }
}
