using BepInEx.Logging;
using static InventoryOrganizingFeatures.Reflections.Extensions.LocaleHelper;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InventoryOrganizingFeatures.Reflections.Extensions;
using InventoryOrganizingFeatures.Reflections;

namespace InventoryOrganizingFeatures
{
    internal class OrganizedContainer : IComparable<OrganizedContainer>
    {
        public static ManualLogSource Logger = null;

        public const string ParamDefault = "--default";
        public const string ParamFoundInRaid = "--fir";
        public const string ParamNotFoundInRaid = "--not-fir";
        public static string[] DoubleDashParams = { ParamDefault, ParamFoundInRaid, ParamNotFoundInRaid };

        public const string NameParamPrefix = "n:";
        public const char NotParamPrefix = '!';
        public const string OrderParamPrefix = "#:";

        public string[] Params { get; }
        public LootItemClass TargetItem { get; }
        public InventoryControllerClass Controller { get; }
        public LootItemClass SourceItem { get; }
        public List<Item> ValidItems
        {
            get
            {
                LogNotif($"Parent name: {SourceItem.RLocalizedName()}");
                List<Item> result = new List<Item>();
                foreach (var grid in SourceItem.RGrids())
                {
                    // ToList is important, since when organizing we can accidentally affect the iterated enumerable.
                    result.AddRange(grid.Items.Where(ItemFitsParams).ToList());
                }
                return result;
            }
        }

        public List<Item> AllItems
        {
            get
            {
                List<Item> result = new List<Item>();
                foreach (var grid in SourceItem.RGrids())
                {
                    // ToList is important, since when organizing we can accidentally affect the iterated enumerable.
                    result.AddRange(grid.Items.ToList());
                }
                return result;
            }
        }

        public OrganizedContainer(LootItemClass targetItem, LootItemClass sourceItem, InventoryControllerClass controller)
        {
            TargetItem = targetItem;
            Controller = controller;
            SourceItem = sourceItem;
            Params = Organizer.ParseOrganizeParams(targetItem);
        }

        private void LogNotif(string message)
        {
            if (Plugin.EnableLogs) Logger.LogMessage(message);
        }



        // Reflections are done by a static ReflectionHelper which uses a cache
        // so using reflections in loop doesn't hurt performance.
        public void Organize(bool ignoreParams = false)
        {

            var validItems = ignoreParams ? AllItems : ValidItems;
            LogNotif($"Valid items: {validItems.Count}");
            //GClass2463 inventoryChanges = new GClass2463(TopLevelItem, Controller);
            var targetGrids = TargetItem.RGrids(); // take it out of the loop 
            foreach (var validItem in validItems)
            {
                foreach (var grid in targetGrids)
                {
                    // First try "TransferOrMerge" with any other item
                    // If stackable ofcourse.
                    if (validItem.StackMaxSize > 1)
                    {
                        // Get only items which can be stacked with validItem
                        // Since grid items won't be removed, only perhaps added - there's no need to make a .toList() copy.
                        foreach (var gridItem in grid.Items.Where(item => CanBeStacked(validItem, item)))
                        {
                            LogNotif($"StackMerging validItem.StackObjectsCount BEFORE:{validItem.StackObjectsCount}");
                            if (validItem.StackObjectsCount < 1) break; // break if validItem ran out of items in stack

                            var transferMergeResult = ItemTransactionHelper.TransferOrMerge(validItem, gridItem, Controller, true);
                            if (transferMergeResult.GetPropertyValue<bool>("Failed"))
                            {
                                LogNotif($"StackMerging FAILED TRANSACT AT ITEM COUNT:{validItem.StackObjectsCount}");
                                LogNotif($"StackMerging FAILED TRANSACT AT TARGET ITEM COUNT:{gridItem.StackObjectsCount}");
                                continue; // skip iteration if transaction failed
                            }

                            _ = Controller.InvokeMethod("RunNetworkTransaction", new object[] { transferMergeResult.GetFieldValue("Value") });

                            LogNotif($"StackMerging validItem.StackObjectsCount AFTER:{validItem.StackObjectsCount}");
                        }
                    }

                    // Second, after stack related work - try to place and item if it still has elements in stack.
                    if (validItem.StackObjectsCount < 1) continue; // skip validItem iteration if has no elements in stack left

                    var location = grid.FindLocationForItem(validItem); // actually returns a GClass, but it inherits from ItemAddress
                    if (location == null) continue; // spik validItem if couldn't fit item

                    // In reference (OnClick from ItemView) simulate = true was used.
                    var moveResult = ItemTransactionHelper.Move(validItem, (ItemAddress)location, Controller, true);
                    if (moveResult.GetPropertyValue<bool>("Failed")) continue; // skip iteration if transaction failed

                    // Use reflective invoke, because original method uses a GInerface as parameter
                    _ = Controller.InvokeMethod("RunNetworkTransaction", new object[] { moveResult.GetFieldValue("Value") });

                    LogNotif("Executed one iteration of organizing.");
                }
            }
        }

        public static bool CanBeStacked(Item item, Item itemToStackWith)
        {
            return item.SpawnedInSession == itemToStackWith.SpawnedInSession // prevent stacking non-fir with fir items which removes the fir status
                && item.TemplateId == itemToStackWith.TemplateId
                && itemToStackWith.StackObjectsCount < itemToStackWith.StackMaxSize;
        }

        public bool ItemFitsParams(Item item)
        {
            // If item is Move Locked - ignore it
            if (Locker.IsMoveLocked(item)) return false;

            // FIR check
            if (ParamsContainFoundInRaid && !item.SpawnedInSession) return false;
            if (ParamsContainNotFoundInRaid && item.SpawnedInSession) return false;

            return CanAccept(item) && ItemPassesCategoryConditions(item) && ItemPassesNameConditions(item);
        }

        // Reference GClass2174.CanAccept or just IContainer.CheckItemFilter
        public bool CanAccept(Item item)
        {
            return TargetItem.RGrids().Any(grid => grid.CanAccept(item));
        }

        private bool ItemPassesCategoryConditions(Item item)
        {
            return ItemFitsPositiveCategoryParams(item) && ItemFitsNegatedCategoryParams(item);
        }

        private bool ItemPassesNameConditions(Item item)
        {
            return ItemFitsPositiveNameParams(item) && ItemFitsNegatedNameParams(item);
        }

        private bool ItemFitsPositiveCategoryParams(Item item)
        {
            if (PositiveCategoryParams.Length < 1) return true;
            if (ParamsContainDefault) return true;
            var node = Organizer.Handbook.FindNode(item.TemplateId);
            if (node == null)
            {
                //if (CanAccept(item))
                //{
                //    Logger.LogWarning($"InventoryOrganizingFeatures Warning: Coudln't find {item.LocalizedName()} in handbook. Perhaps it's a modded item? It's not a critical error, just a warning.");
                //    //NotificationManagerClass.DisplayWarningNotification($"InventoryOrganizingFeatures Warning: Coudln't find {item.LocalizedName()} in handbook. Perhaps it's a modded item?");
                //}
                return false;
            }
            return PositiveCategoryParams.Any(param => node.CategoryContains(param));
        }

        private bool ItemFitsNegatedCategoryParams(Item item)
        {
            if (NegatedCategoryParams.Length < 1) return true;
            var node = Organizer.Handbook.FindNode(item.TemplateId);
            if (node == null)
            {
                //if (CanAccept(item))
                //{
                //    Logger.LogWarning($"InventoryOrganizingFeatures Warning: Coudln't find {item.LocalizedName()} in handbook. Perhaps it's a modded item? It's not a critical error, just a warning.");
                //    //NotificationManagerClass.DisplayWarningNotification($"InventoryOrganizingFeatures Warning: Coudln't find {item.LocalizedName()} in handbook. Perhaps it's a modded item?");
                //}
                return false;
            }
            return NegatedCategoryParams.All(param => !node.CategoryContains(param));
        }

        private bool ItemFitsPositiveNameParams(Item item)
        {
            if (PositiveNameParams.Length < 1) return true;
            return PositiveNameParams.Any(param => item.RLocalizedName().ToLower().Contains(param.ToLower()));
        }

        private bool ItemFitsNegatedNameParams(Item item)
        {
            if (NegatedNameParams.Length < 1) return true;
            return NegatedNameParams.All(param => !item.RLocalizedName().ToLower().Contains(param.ToLower()));
        }

        public static bool IsPositiveParam(string param)
        {
            return !param.StartsWith(NotParamPrefix.ToString());
        }

        public static bool IsNegatedParam(string param)
        {
            return param.StartsWith(NotParamPrefix.ToString());
        }

        public static bool IsDoubleDashParam(string param)
        {
            return DoubleDashParams.Any(ddp => ddp.Equals(param.ToLower()));
        }

        public static bool IsNameParam(string param)
        {
            return param.StartsWith(NameParamPrefix);
        }

        public static bool IsCategoryParam(string param)
        {
            return !IsDoubleDashParam(param) && !IsNameParam(param) && !IsOrderParam(param);
        }

        public static bool IsOrderParam(string param)
        {
            return param.StartsWith(OrderParamPrefix);
        }

        private int? Order
        {
            get
            {
                var orderStr = Params.Where(IsOrderParam).Select(param => param.Substring(OrderParamPrefix.Length).Trim()).FirstOrDefault();
                if (orderStr == null) return null;
                if (int.TryParse(orderStr, out int result))
                {
                    return result;
                }
                return null;
            }
        }

        private string[] NonDoubleDashParams
        {
            get
            {
                return Params.Where(param => !IsDoubleDashParam(param)).ToArray();
            }
        }

        private string[] CategoryParams
        {
            get
            {
                return Params.Where(IsCategoryParam).Select(param => param.Trim()).ToArray();
            }
        }

        private string[] PositiveCategoryParams
        {
            get
            {
                return CategoryParams.Where(IsPositiveParam).ToArray();
            }
        }

        private string[] NegatedCategoryParams
        {
            get
            {
                return CategoryParams.Where(IsNegatedParam).Select(param => param.TrimStart(NotParamPrefix)).ToArray();
            }
        }

        private string[] NameParams
        {
            get
            {
                return Params.Where(IsNameParam).Select(param => param.Substring(NameParamPrefix.Length).Trim()).ToArray();
            }
        }

        private string[] PositiveNameParams
        {
            get
            {
                return NameParams.Where(IsPositiveParam).ToArray();
            }
        }

        private string[] NegatedNameParams
        {
            get
            {
                return NameParams.Where(IsNegatedParam).Select(param => param.TrimStart(NotParamPrefix)).ToArray();
            }
        }

        private bool ParamsContainDefault
        {
            get
            {
                return Params.Any(param => param.Equals(ParamDefault)) || CategoryParams.Length < 1;
            }
        }

        private bool ParamsContainFoundInRaid
        {
            get
            {
                return Params.Any(param => param.Equals(ParamFoundInRaid));
            }
        }
        private bool ParamsContainNotFoundInRaid
        {
            get
            {
                return Params.Any(param => param.Equals(ParamNotFoundInRaid));
            }
        }



        public static string[] GetCategoryParams(string[] parameters)
        {
            return parameters.Where(IsCategoryParam).Select(param => param.Trim()).ToArray();
        }

        public static string[] GetNameParams(string[] parameters)
        {
            return parameters.Where(IsNameParam).Select(param => param.Substring(NameParamPrefix.Length).Trim()).ToArray();
        }

        public static int? GetOrderParam(string[] parameters)
        {
            var orderStr = parameters.Where(IsOrderParam).Select(param => param.Substring(OrderParamPrefix.Length).Trim()).FirstOrDefault();
            if (orderStr == null) return null;
            if (int.TryParse(orderStr, out int result))
            {
                return result;
            }
            return null;
        }

        public static bool HasParamDefault(string[] parameters)
        {
            return parameters.Any(param => param.Equals(ParamDefault)) || GetCategoryParams(parameters).Length < 1;
        }

        public static bool HasParamFoundInRaid(string[] parameters)
        {
            return parameters.Any(param => param.Equals(ParamFoundInRaid));
        }
        public static bool HasParamNotFoundInRaid(string[] parameters)
        {
            return parameters.Any(param => param.Equals(ParamNotFoundInRaid));
        }

        public static bool HasOrderParam(string[] parameters)
        {
            return parameters.Any(IsOrderParam);
        }

        private bool DoesntHaveOnlyNameParams
        {
            get
            {
                return CategoryParams.Length > 0;
            }
        }

        private bool DoesntHaveOnlyDoubleDashParams
        {
            get
            {
                return CategoryParams.Length > 0 || NameParams.Length > 0;
            }
        }

        private bool HasOnlyNegatedNameParams
        {
            get
            {
                return PositiveNameParams.Length < 1 && NegatedNameParams.Length > 0;
            }
        }

        private bool HasOnlyCategoryParams
        {
            get
            {
                return CategoryParams.Length > 0 && NameParams.Length < 1;
            }
        }

        private bool HasOnlyNegatedCategoryParams
        {
            get
            {
                return PositiveCategoryParams.Length < 1 && NegatedCategoryParams.Length > 0;
            }
        }

        private bool HasOnlyDefaultCategory
        {
            get
            {
                return ParamsContainDefault && CategoryParams.Length < 1 && NameParams.Length < 1;
            }
        }

        private bool HasDefaultAndOneFirParam
        {
            get
            {
                return ParamsContainDefault && HasOneOfFirParams;
            }
        }

        private bool HasOneOfFirParams
        {
            get
            {
                return ParamsContainFoundInRaid ^ ParamsContainNotFoundInRaid;
            }
        }

        private bool HasExplicitOrder { get { return Order != null; } }

        public int CompareTo(OrganizedContainer instance)
        {
            //NotificationManagerClass.DisplayMessageNotification($"Item order #{Order}", EFT.Communications.ENotificationDurationType.Infinite);
            // Check for explicit order
            var hasExplicitOrderCmp = instance.HasExplicitOrder.CompareTo(HasExplicitOrder);
            if (hasExplicitOrderCmp == 0)
            {
                // If both have explicit orders use them for ordering
                if (HasExplicitOrder && instance.HasExplicitOrder)
                {
                    //NotificationManagerClass.DisplayMessageNotification($"Reached order comparison", EFT.Communications.ENotificationDurationType.Infinite);
                    return ((int)Order).CompareTo((int)instance.Order);
                }

                // In all other cases follow implicit ordering rules

                // If contains default category - later in order
                var containsOnlyDefaultCmp = HasOnlyDefaultCategory.CompareTo(instance.HasOnlyDefaultCategory);
                if (containsOnlyDefaultCmp == 0)
                {
                    //NotificationManagerClass.DisplayMessageNotification($"Both contain default category", EFT.Communications.ENotificationDurationType.Infinite);

                    // If contains default category and one of the fir params - earlier in order
                    var onlyOneFirParam = instance.HasDefaultAndOneFirParam.CompareTo(HasDefaultAndOneFirParam);
                    if (onlyOneFirParam == 0)
                    {
                        //NotificationManagerClass.DisplayMessageNotification($"Have both fir params or don't have any at all", EFT.Communications.ENotificationDurationType.Infinite);

                        // If has only double dash params - later in order
                        var notOnlyDoubleDashParams = DoesntHaveOnlyDoubleDashParams.CompareTo(instance.DoesntHaveOnlyDoubleDashParams);
                        if (notOnlyDoubleDashParams == 0) // if both are false - code below will still return 0, since category and name params count will be the same for both, a 0.
                        {
                            // How to compare when both have something other than double dash params
                            //NotificationManagerClass.DisplayMessageNotification($"Both have something more than double dashes or maybe not", EFT.Communications.ENotificationDurationType.Infinite);
                            // If doesn't have just name params - later in order. In this case default category(all acceptable items) is used.
                            var notOnlyNamesParams = DoesntHaveOnlyNameParams.CompareTo(instance.DoesntHaveOnlyNameParams);
                            if (notOnlyNamesParams == 0)
                            {
                                // If both have something other than just name params. That means category params.
                                if (DoesntHaveOnlyNameParams && instance.DoesntHaveOnlyNameParams)
                                {
                                    //NotificationManagerClass.DisplayMessageNotification($"Both have more than name params", EFT.Communications.ENotificationDurationType.Infinite);

                                    // If has only category params - later in order
                                    var onlyCategoryParams = HasOnlyCategoryParams.CompareTo(instance.HasOnlyCategoryParams);
                                    if (onlyCategoryParams == 0)
                                    {
                                        // If both have only category params
                                        if (HasOnlyCategoryParams && instance.HasOnlyCategoryParams)
                                        {
                                            //NotificationManagerClass.DisplayMessageNotification($"Reached only category comparison", EFT.Communications.ENotificationDurationType.Infinite);
                                            return CompareByCategoryParamsTo(instance);
                                        }
                                        // If both have category and name params
                                        else
                                        {
                                            // Compare by categories first because they are the most wide.
                                            // This will make sure that narrower category condition is prioritized.
                                            var categoryCmp = CompareByCategoryParamsTo(instance);
                                            if (categoryCmp == 0)
                                            {
                                                // Since user most likely won't write many name params,
                                                // they are semantically narrower than categories, by default.
                                                // So here finally compare by name params;
                                                return CompareByNameParamsTo(instance);
                                            }
                                            return categoryCmp;
                                        }
                                    }
                                    return onlyCategoryParams;
                                }
                                // If both have only name params
                                else
                                {
                                    //NotificationManagerClass.DisplayMessageNotification($"Reached only name comparison", EFT.Communications.ENotificationDurationType.Infinite);
                                    return CompareByNameParamsTo(instance);
                                }
                            }
                            //NotificationManagerClass.DisplayMessageNotification($"Returnin notOnlyNamesParams", EFT.Communications.ENotificationDurationType.Infinite);
                            return notOnlyNamesParams;
                        }
                        //NotificationManagerClass.DisplayMessageNotification($"Returnin notOnlyDoubleDashParams", EFT.Communications.ENotificationDurationType.Infinite);
                        return notOnlyDoubleDashParams;
                    }
                    return onlyOneFirParam;
                }
                //NotificationManagerClass.DisplayMessageNotification($"Returning containsDefaultCmp", EFT.Communications.ENotificationDurationType.Infinite);
                return containsOnlyDefaultCmp;
            }
            return hasExplicitOrderCmp;
        }

        public int CompareByCategoryParamsTo(OrganizedContainer instance)
        {
            // If only negated category params - later in order
            var onlyNegatedCategories = HasOnlyNegatedCategoryParams.CompareTo(instance.HasOnlyNegatedCategoryParams);
            if (onlyNegatedCategories == 0)
            {
                // If both have only negated category params
                if (HasOnlyNegatedCategoryParams && instance.HasOnlyNegatedCategoryParams)
                {
                    // If has more negated params - earlier in order
                    var negatedCategoryParamsCmp = instance.NegatedCategoryParams.Length.CompareTo(NegatedCategoryParams.Length);
                    if (negatedCategoryParamsCmp == 0)
                    {
                        return CompareByFirParamsTo(instance);
                    }
                    return negatedCategoryParamsCmp;
                }
                // For all other cases. When only category params present, and it's both positive and negated - negated params can be ignored.
                else
                {
                    // If has more name params - later in order
                    var positiveCategoryParamsCmp = PositiveCategoryParams.Length.CompareTo(instance.PositiveCategoryParams.Length);
                    if (positiveCategoryParamsCmp == 0)
                    {
                        return CompareByFirParamsTo(instance);
                    }
                    return PositiveCategoryParams.Length.CompareTo(instance.PositiveCategoryParams.Length);
                }
            }
            return onlyNegatedCategories;
        }

        public int CompareByNameParamsTo(OrganizedContainer instance)
        {
            // if has only negated name params - later in order
            var onlyNegatedNameParams = HasOnlyNegatedNameParams.CompareTo(instance.HasOnlyNegatedNameParams);
            if (onlyNegatedNameParams == 0)
            {
                // If both have only negated name params
                if (HasOnlyNegatedNameParams && instance.HasOnlyNegatedNameParams)
                {
                    //NotificationManagerClass.DisplayMessageNotification($"Checking if has less negated name params - {instance.NegatedNameParams.Length.CompareTo(NegatedNameParams.Length)}", EFT.Communications.ENotificationDurationType.Infinite);
                    // If has more negated params - earlier in order
                    var negatedNameParamsCmp = instance.NegatedNameParams.Length.CompareTo(NegatedNameParams.Length);
                    if (negatedNameParamsCmp == 0)
                    {
                        return CompareByFirParamsTo(instance);
                    }
                    return negatedNameParamsCmp;
                }
                // For all other cases. When only name params present, and it's both positive and negated - negated params can be ignored.
                else
                {
                    //NotificationManagerClass.DisplayMessageNotification($"Checking if has more positive name params - {PositiveNameParams.Length.CompareTo(instance.PositiveNameParams.Length)}", EFT.Communications.ENotificationDurationType.Infinite);
                    // If has more name params - later in order
                    var positiveNameParamsCmp = PositiveNameParams.Length.CompareTo(instance.PositiveNameParams.Length);
                    if (positiveNameParamsCmp == 0)
                    {
                        return CompareByFirParamsTo(instance);
                    }
                    return positiveNameParamsCmp;
                }
            }
            return onlyNegatedNameParams;
        }

        public int CompareByFirParamsTo(OrganizedContainer instance)
        {
            // If has one of the --fir params - earlier in order
            return instance.HasOneOfFirParams.CompareTo(HasOneOfFirParams);
        }
    }

}
