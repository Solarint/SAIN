using Aki.Reflection.Patching;
using BepInEx.Logging;
using EFT.InventoryLogic;
using Newtonsoft.Json.UnityConverters;
using System.ComponentModel;
using System.Security.Cryptography;
using IContainer = EFT.InventoryLogic.IContainer;

namespace InventoryOrganizingFeatures
{

    internal static class Locker
    {
        public const string MoveLockTag = "@ml";
        public const string SortLockTag = "@sl";

        public static ISession Session { get; set; } = null;

        public static bool IsMoveLocked(Item item)
        {
            return item.TryGetItemComponent(out TagComponent tagComponent) && IsMoveLocked(tagComponent.Name);
        }

        public static bool IsMoveLocked(string tagName)
        {
            return ContainsSeparate(tagName, MoveLockTag);
        }

        public static bool IsSortLocked(Item item)
        {
            return item.TryGetItemComponent(out TagComponent tagComponent) && (IsSortLocked(tagComponent.Name) || IsMoveLocked(tagComponent.Name));
        }

        public static bool IsSortLocked(string tagName)
        {
            return ContainsSeparate(tagName, SortLockTag) || IsMoveLocked(tagName);
        }

        private static bool ContainsSeparate(string tagName, string findTag)
        {
            if(tagName.Contains(findTag))
            {
                // check char before tag
                int beforeTagIdx = tagName.IndexOf(findTag) - 1;
                if(beforeTagIdx >= 0)
                {
                    if (tagName[beforeTagIdx] != ' ') return false;
                }
                // check char after tag
                int afterTagIdx = tagName.IndexOf(findTag) + findTag.Length;
                if(afterTagIdx <= tagName.Length - 1)
                {
                    if (tagName[afterTagIdx] != ' ') return false;
                }
                return true;
            }
            return false;
        }
    }

}
