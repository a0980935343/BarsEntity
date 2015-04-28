using System;
using System.Collections.Generic;
using System.Linq;

namespace Barsix.BarsEntity
{
    public static class CollectionExt
    {
        public static void AddDistinct(this ICollection<string> collection, string item)
        {
            if (!string.IsNullOrWhiteSpace(item) && !collection.Any(x => x == item))
                collection.Add(item);
        }
    }
}
