using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity
{
    public class MosKsProfile : ProjectProfileBase
    {
        public override string Name { get { return "MosKs"; } }

        public override string IconPath { get { return "\"content/modules/MosKs/icons64/meterUnit.png\""; } }

        public override ViewType ViewType { get { return ViewType.EAS; } }

        public override string HrefPrefix { get { return "#"; } }
    }
}
