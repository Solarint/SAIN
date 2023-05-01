using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryOrganizingFeatures.Reflections
{
    /// <summary>
    /// Reflection of "ItemTransactionHelper" class. As of SPT-AKI 3.5.5 reference is GClass2429
    /// </summary>
    internal static class ItemTransactionHelper
    {
        public static Type ReflectedType { get; }
        static ItemTransactionHelper()
        {
            var classMethods = new string[] {
                "Sort",
                "QuickFindAppropriatePlace",
                "TransferOrMerge",
                "TransferMax",
                "Merge",
                "ApplyItemToRevolverDrum",
                "ApplySingleItemToAddress",
                "Fold",
                "CanRecode",
                "CanFold"
            };
            ReflectedType = ReflectionHelper.FindClassTypeByMethodNames(classMethods);
        }

        // Usually simulate = true
        public static object Move(Item item, ItemAddress to, TraderControllerClass itemController, bool simulate = false)
        {
            // No parameter types needed, only one "Move" method present in the class, at least for the time being.
            return ReflectedType.InvokeMethod("Move", new object[] {item, to, itemController, simulate});
        }

        // Usually simulate = true
        public static object TransferOrMerge(Item item, Item targetItem, TraderControllerClass itemController, bool simulate)
        {
            return ReflectedType.InvokeMethod("TransferOrMerge", new object[] { item, targetItem, itemController, simulate });
        }

    }
}
