using Aki.Reflection.Utils;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Editor
{
    internal static class CursorSettings
    {
        private static readonly MethodInfo setCursorMethod;

        static CursorSettings()
        {
            var cursorType = PatchConstants.EftTypes.Single(x => x.GetMethod("SetCursor") != null);
            setCursorMethod = cursorType.GetMethod("SetCursor");
        }

        public static void SetCursor(ECursorType type)
        {
            setCursorMethod.Invoke(null, new object[] { type });
        }
    }
}
