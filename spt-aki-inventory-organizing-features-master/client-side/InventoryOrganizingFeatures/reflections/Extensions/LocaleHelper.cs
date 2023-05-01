using EFT;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;

namespace InventoryOrganizingFeatures.Reflections.Extensions
{
    /// <summary>
    /// Reflection of a string Localization extension class with Localized, LocalizedName and all that jazz.
    /// Can be extended as needed.
    /// As of SPT-AKI 3.5.5 reference is GClass1761, For 3.5.2-3.5.4 it's GClass1758.
    /// 
    /// All reflected methods follow the original naming, but have an R-prefix
    /// </summary>
    internal static class LocaleHelper
    {
        public static Type ReflectedType { get; }
        static LocaleHelper()
        {
            var stringExtensionMethods = new string[] {
                "FormatSeparate",
                "Localized",
                "LocalizeAreaName",
                "LocalizedEnum",
                "LocalizedName",
                "LocalizedShortName",
                "LocalizedShort",
                "ParseLocalization"
            };
            ReflectedType = ReflectionHelper.FindClassTypeByMethodNames(stringExtensionMethods);
        }

        public static string RLocalized(this string key, string prefix = null)
        {
            return ReflectedType.InvokeMethod<string>("Localized", new object[] { key, prefix }, new Type[] { typeof(string), typeof(string) });
            //return InvokeMethod<string>("Localized", new object[] { key, prefix }, new Type[] { typeof(string), typeof(string) });
        }

        // there is also a string[] Localized() but even this one is not needed. Might remove later.
        public static string RLocalized(this string key, EStringCase strCase)
        {
            return ReflectedType.InvokeMethod<string>("Localized", new object[] { key, strCase }, new Type[] { typeof(string), typeof(EStringCase) });
            //return InvokeMethod<string>("Localized", new object[] { key, strCase }, new Type[] { typeof(string), typeof(EStringCase) });
        }

        public static string RLocalizedName(this Item item)
        {
            return ReflectedType.InvokeMethod<string>("LocalizedName", new object[] { item }, new Type[] { typeof(Item) });
            //return InvokeMethod<string>("LocalizedName", new object[] { item }, new Type[] { typeof(Item) });
        }
    }
}
