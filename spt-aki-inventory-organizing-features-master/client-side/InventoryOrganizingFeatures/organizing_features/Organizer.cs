using EFT.InventoryLogic;
using HarmonyLib;
using InventoryOrganizingFeatures.Reflections;
using InventoryOrganizingFeatures.Reflections.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static InventoryOrganizingFeatures.OrganizedContainer;
using static InventoryOrganizingFeatures.Reflections.Extensions.LocaleHelper;

namespace InventoryOrganizingFeatures
{
    internal class Organizer
    {
        public const string OrganizeTag = "@o";
        public const char OrganizeTagSeparator = '|';
        public const char OrganizeTagEnd = ';';
        public static Regex OrganizeRegex = new(OrganizeTag + " (.*?)" + OrganizeTagEnd);

        public static Handbook Handbook { get; set; } = null;
        
        public static void Organize(LootItemClass topLevelItem, InventoryControllerClass controller)
        {
            //foreach (var grid in topLevelItem.Grids) - needs reflection since Grids is a GClass2163 (per SPT-AKI 3.5.3)
            //foreach (var grid in ReflectionHelper.GetFieldValue<object[]>(topLevelItem, "Grids"))
            foreach (var grid in topLevelItem.RGrids())
            {
                // reflect grid.Items 
                var organizedContainers = grid.Items.Where(IsOrganized).Select(item => new OrganizedContainer((LootItemClass)item, topLevelItem, controller)).ToList();
                organizedContainers.Sort();
                //var inc = 0;
                foreach (var container in organizedContainers)
                {
                    //NotificationManagerClass.DisplayMessageNotification($"Container #{inc}: {string.Join(", ",container.Params)}", duration: EFT.Communications.ENotificationDurationType.Infinite);
                    LogNotif($"Organized Container: {container.TargetItem.RLocalizedName()}");
                    container.Organize();
                    //inc++;
                }
            }
        }

        private static void LogNotif(string message)
        {
            if (Plugin.EnableLogs) Plugin.GlobalLogger.LogMessage(message);
        }

        public static bool IsOrganized(Item item)
        {
            if (!item.TryGetItemComponent(out TagComponent tagComponent)) return false;
            if (!item.IsContainer) return false;
            return IsOrganized(tagComponent.Name);
        }
        public static bool IsOrganized(string tagName)
        {
            return ParseOrganizeParams(tagName).Length > 0;
        }

        private static bool ContainsSeparate(string tagName, string findTag)
        {
            if (tagName.Contains(findTag))
            {
                // check char before tag
                int beforeTagIdx = tagName.IndexOf(findTag) - 1;
                if (beforeTagIdx >= 0)
                {
                    if (tagName[beforeTagIdx] != ' ') return false;
                }
                // check char after tag
                int afterTagIdx = tagName.IndexOf(findTag) + findTag.Length;
                if (afterTagIdx <= tagName.Length - 1)
                {
                    if (tagName[afterTagIdx] != ' ') return false;
                }
                return true;
            }
            return false;
        }

        public static string[] ParseOrganizeParams(Item item)
        {
            if (!IsOrganized(item)) return new string[0];
            if (!item.TryGetItemComponent(out TagComponent tagComponent)) return new string[0];
            return ParseOrganizeParams(tagComponent.Name);
        }

        public static string[] ParseOrganizeParams(string tagName)
        {
            string organizeStr = OrganizeRegex.Match(tagName).Value;
            if (string.IsNullOrEmpty(organizeStr))
            {
                // If full organize regex match not found - check shortcut is used
                if (ContainsSeparate(tagName, OrganizeTag))
                {
                    return new string[] { ParamDefault };
                }
                return new string[0];
            }

            var result = organizeStr
                .Substring(OrganizeTag.Length + 1) // remove the tag
                .TrimEnd(OrganizeTagEnd) // remove the closing semicolon
                .Trim() // trim spaces
                .Split(OrganizeTagSeparator) // split by defined separator
                .Select(param => param.Trim()) // trim every param
                .Where(param => !string.IsNullOrEmpty(param)) // filter out empty left-over params
                .ToArray();

            // If all params are negated - add default category param
            if (result.Length > 0 && GetCategoryParams(result).AddRangeToArray(GetNameParams(result)).All(IsNegatedParam))
            {
                // LINQ Prepend with converison is pretty slow, but since this isn't a loop it should be fine.
                // Maybe change to something else later.
                result = result.Prepend(ParamDefault).ToArray();
            }
            // If params contain only FoundInRaid or NotFoundInRaid param then add default category param to the beginning.
            if (result.Length == 1 && (result.Contains(ParamFoundInRaid) || result.Contains(ParamNotFoundInRaid)))
            {
                result = result.Prepend(ParamDefault).ToArray();
            }
            if (result.Length < 1) result = result.Prepend(ParamDefault).ToArray();
            return result;
        }
    }

}
