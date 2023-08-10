using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Helpers
{
    internal class ListHelpers
    {
        public static bool ClearCache<T>(List<T> list)
        {
            if (list != null && list.Count > 0)
            {
                list.Clear();
                return true;
            }
            return false;
        }

        public static bool ClearCache<T, V>(Dictionary<T, V> list)
        {
            if (list != null && list.Count > 0)
            {
                list.Clear();
                return true;
            }
            return false;
        }
    }
}
