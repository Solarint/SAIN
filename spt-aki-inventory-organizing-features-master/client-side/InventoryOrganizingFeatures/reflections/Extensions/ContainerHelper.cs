using EFT;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;

namespace InventoryOrganizingFeatures.Reflections.Extensions
{
    /// <summary>
    /// Reflection of a Container Helper class with CanAccept method.
    /// Can be extended as needed.
    /// As of SPT-AKI 3.5.2-3.5.4 it's GClass2174.
    /// 
    /// All reflected methods follow the original naming and use Reflected Classes.
    /// </summary>
    internal static class ContainerHelper
    {
        public static Type ReflectedType { get; set; }
        static ContainerHelper()
        {
            var stringExtensionMethods = new string[] {
                "CheckItemExcludedFilter",
                "CheckItemFilter",
                "CanAccept"
            };
            ReflectedType = ReflectionHelper.FindClassTypeByMethodNames(stringExtensionMethods);
        }

        public static bool CanAccept(this Grid grid, Item item)
        {
            return ReflectedType.InvokeMethod<bool>("CanAccept", new object[] { grid.ReflectedInstance, item }, new Type[] { typeof(IContainer), typeof(Item) });
        }
    }
}
