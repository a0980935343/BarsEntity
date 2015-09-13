using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barsix.BarsEntity
{
    using EnvDTE;

    public static class CodeObjectExt
    {
        public static string GetSummary(string comment)
        {
            if (comment == null)
                return string.Empty;

            return comment.Trim().Replace("\r", "").Replace("\n", "").Untag("doc").Trim().Untag("summary").Trim();
        }
    }
}
