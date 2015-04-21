using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity.BarsOptions
{
    public class ViewOptions
    {
        public string Namespace;
        public string Title;

        public bool EditingDisabled;

        public bool DynamicFilter;

        public bool TreeGrid;

        public string SelectionModel = "RowSelectionModel";
    }
}