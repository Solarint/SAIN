using Aki.Reflection.Patching;
using EFT.HandBook;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using InventoryOrganizingFeatures.Reflections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using static InventoryOrganizingFeatures.Locker;
using static InventoryOrganizingFeatures.Organizer;
using static InventoryOrganizingFeatures.OrganizedContainer;
using static InventoryOrganizingFeatures.UserInterfaceElements;
using InventoryOrganizingFeatures.Reflections.Extensions;
using TMPro;

namespace InventoryOrganizingFeatures
{
    internal class PostEditTagWindowShow : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EditTagWindow), "Show", new Type[] { typeof(TagComponent), typeof(Action), typeof(Action), typeof(Action<string, int>) });
        }

        [PatchPrefix]
        private static void PatchPrefix(ref EditTagWindow __instance, ref DefaultUIButton ____saveButtonSpawner, ValidationInputField ____tagInput)
        {
            try
            {
                ____tagInput.characterLimit = 256;
                ____saveButtonSpawner.OnClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                {
                    try
                    {
                        string notifMsg = "";
                        if (IsSortLocked(____tagInput.text)) notifMsg += "This item is Sort Locked.";
                        if (IsMoveLocked(____tagInput.text))
                        {
                            if (notifMsg.Length > 0) notifMsg += "\n";
                            notifMsg += "This item is Move Locked.";
                        }
                        if (IsOrganized(____tagInput.text))
                        {
                            if (notifMsg.Length > 0) notifMsg += "\n";
                            // Add pretty notification output
                            var orgParams = ParseOrganizeParams(____tagInput.text);
                            var categoryParams = GetCategoryParams(orgParams);
                            var nameParams = GetNameParams(orgParams);

                            notifMsg += "This item's tag has following organize params:";
                            if (HasOrderParam(orgParams))
                            {
                                notifMsg += $"\n  -  Order #{GetOrderParam(orgParams).GetValueOrDefault()}";
                            }
                            if (HasParamDefault(orgParams))
                            {
                                notifMsg += $"\n  -  Category: default container categories";
                            }
                            else if (categoryParams.Length > 0)
                            {
                                notifMsg += $"\n  -  Category: {string.Join(", ", categoryParams)}";
                            }

                            if (nameParams.Length > 0)
                            {
                                notifMsg += $"\n  -  Name: {string.Join(", ", nameParams)}";
                            }

                            if (HasParamFoundInRaid(orgParams))
                            {
                                notifMsg += "\n  -  Only \"Found in raid\".";
                            }
                            else if (HasParamNotFoundInRaid(orgParams))
                            {
                                notifMsg += "\n  -  Only \"Not found in raid.\"";
                            }
                        }
                        if (notifMsg.Length > 0) NotificationManagerClass.DisplayMessageNotification(notifMsg);
                    }
                    catch (Exception ex)
                    {
                        throw Plugin.ShowErrorNotif(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }

        }
    }

    // Static SortClass.Sort() checks if Item.CurrentLocation is null
    // so preventing sort locked items from being removed
    // makes the sort method ignore them.
    internal class PreGridClassRemoveAll : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Find the Grid class (Per STP-AKI 3.5.5 it's a GClass2166)
            var gridClassMethods = new string[] { "FindItem", "GetItemsInRect", "FindLocationForItem", "Add", "AddItemWithoutRestrictions", "Remove", "RemoveAll", "CanReplace" };
            return AccessTools.Method(ReflectionHelper.FindClassTypeByMethodNames(gridClassMethods), "RemoveAll");
        }

        // Conditional reimplementation of original method, but with tweaks to ignore "Sort/Move Locked" items
        // Since the Sort method has a check for "item.CurrentAddress == null", it simply won't touch these items if they weren't removed
        [PatchPrefix]
        private static bool PatchPrefix(ref object __instance)
        {
            try
            {
                // Dynamically find static SortClass
                var sortClassMethods = new string[] { "Sort", "ApplyItemToRevolverDrum", "ApplySingleItemToAddress", "Fold", "CanRecode", "CanFold" };
                var sortClassType = ReflectionHelper.FindClassTypeByMethodNames(sortClassMethods);
                var callerClassType = new StackTrace().GetFrame(2).GetMethod().ReflectedType;
                // If method is being called from the static SortClass - run patched code, if not - run default code.
                if (callerClassType != sortClassType) return true;

                var itemCollection = __instance.GetPropertyValue<IEnumerable<KeyValuePair<Item, LocationInGrid>>>("ItemCollection");
                //if (!__instance.ItemCollection.Any())
                if (!itemCollection.Any())
                {
                    return false;
                }

                var itemCollectionRemove = itemCollection.GetMethod("Remove");
                var gridSetLayout = __instance.GetMethod("SetLayout");

                foreach (var kvp in itemCollection.Where(pair => !IsSortLocked(pair.Key)).ToList())
                {
                    //kvp.Deconstruct(out Item item, out LocationInGrid locationInGrid); - uses a GClass781 extension
                    var item = kvp.Key;
                    var locationInGrid = kvp.Value;
                    //__instance.ItemCollection.Remove(item, __instance);
                    itemCollectionRemove.Invoke(itemCollection, new object[] { item, __instance });
                    //__instance.SetLayout(item, locationInGrid, false);
                    gridSetLayout.Invoke(__instance, new object[] { item, locationInGrid, false });
                }

                var lastLineMethod = __instance // look for method with generic name, called on the last line of RemoveAll()
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(method =>
                    {
                        return method.ReturnType == typeof(void)
                        && method.GetMethodBody().LocalVariables.Count == 6
                        && method.GetMethodBody().LocalVariables.All(variable => variable.LocalType == typeof(int));
                    })
                    .First(); // let it throw exception if somehow the method wasn't found.
                lastLineMethod.Invoke(__instance, null);
                //NotificationManagerClass.DisplayMessageNotification("Ran the RemoveAll patch");
                return false;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PreItemViewOnBeginDrag : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemView), "OnBeginDrag");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref ItemView __instance)
        {
            try
            {
                // Don't execute original event handler if item IsMoveLocked, otherwise execute.
                if (IsMoveLocked(__instance.Item)) return false;
                return true;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PreItemViewOnPointerDown : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemView), "OnPointerDown");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref ItemView __instance)
        {
            try
            {
                // Don't execute original event handler if item IsMoveLocked, otherwise execute.
                if (IsMoveLocked(__instance.Item)) return false;
                return true;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PostGetFailedProperty : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(AccessTools.Method(typeof(ItemUiContext), "QuickFindAppropriatePlace").ReturnType, "Failed");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref object __instance, ref bool __result)
        {
            try
            {
                if (__instance == null) return;

                // Make sure to only execute if called for ItemView, OnClick method.
                var callerMethod = new StackTrace().GetFrame(2).GetMethod();
                if (callerMethod.Name.Equals("OnClick") && callerMethod.ReflectedType == typeof(ItemView))
                {
                    Item item = null;
                    var traverse = Traverse.Create(__instance);
                    // When __instance is just moved (GClass2441 - SPT AKI 3.5.5)
                    if (traverse.Property("Item").PropertyExists())
                        item ??= __instance.GetPropertyValueOrDefault<Item>("Item");
                    // When __instance is moved and merged(stacked) (GClass2443 - SPT AKI 3.5.5)
                    if (traverse.Field("Item").FieldExists())
                        item ??= __instance.GetFieldValueOrDefault<Item>("Item");
                    if (item == null)
                    {
                        Logger.LogWarning($"InventoryOrganizingFeatures Error | Patch@ {__instance.GetType()} Getter of Property \"Failed\": Item is still null. Skipping patch.");
                        NotificationManagerClass.DisplayWarningNotification($"InventoryOrganizingFeatures Error | {__instance.GetType()} Item is still null. You should probably send your bepinex logs to mod developer.");
                        return;
                    }; // null safety
                    if (item.TryGetItemComponent(out TagComponent tagComp))
                    {
                        if (IsMoveLocked(tagComp.Name)) __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PreQuickFindAppropriatePlace : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), "QuickFindAppropriatePlace");
        }

        [PatchPrefix]
        private static void PatchPrefix(Item item, ref bool displayWarnings)
        {
            try
            {
                // Don't display warnings if item IsMoveLocked
                if (IsMoveLocked(item)) displayWarnings = false;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PostGridSortPanelShow : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridSortPanel), "Show");
        }

        [PatchPostfix]
        private static void PatchPostfix(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            try
            {
                var callerClassType = new StackTrace().GetFrame(2).GetMethod().ReflectedType;
                //NotificationManagerClass.DisplayMessageNotification($"Caller class {callerClassType.Name}");

                // For Stash panel
                // TraderDealScreen - when opening trader
                // SimpleStashPanel - in stash
                // GridWindow - when opening a container
                if (callerClassType == typeof(SimpleStashPanel))
                {
                    PatchForSimpleStashPanel(__instance, controller, item, ____button);
                    return;
                }

                if (callerClassType == typeof(TraderDealScreen))
                {
                    PatchForTraderDealScreen(__instance, controller, item, ____button);
                    return;
                }

                // For Container view panel (caller class is GridWindow)
                // Hoping container panel disposes children properly.
                PatchForOtherCases(__instance, controller, item, ____button);
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }

        private static void PatchForSimpleStashPanel(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            if (OrganizeButtonStash != null)
                if (!OrganizeButtonStash.IsDestroyed()) return;
            OrganizeButtonStash = SetupOrganizeButton(____button, item, controller);
        }

        private static void PatchForTraderDealScreen(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            if (OrganizeButtonTrader != null)
                if (!OrganizeButtonTrader.IsDestroyed()) return;
            OrganizeButtonTrader = SetupOrganizeButton(____button, item, controller);
        }

        // Hopefully the "other" cases are only GridViewPanels(if I remember the name correctly)
        private static void PatchForOtherCases(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            // Setup Organize button
            var orgbtn = SetupOrganizeButton(____button, item, controller);
            orgbtn.transform.parent.GetChild(orgbtn.transform.parent.childCount - 2).SetAsLastSibling();
            // Setup Take Out button
            var takeoutbtn = SetupTakeOutButton(____button, item, controller);
            takeoutbtn.transform.parent.GetChild(orgbtn.transform.parent.childCount - 2).SetAsLastSibling();

        }
    }

    internal class PostSimpleStashPanelClose : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SimpleStashPanel), "Close");
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            try
            {
                if (OrganizeButtonStash == null) return;
                if (OrganizeButtonStash.IsDestroyed()) return;

                OrganizeButtonStash.gameObject.SetActive(false);
                GameObject.Destroy(OrganizeButtonStash);

                // Might need it.
                //GameObject.DestroyImmediate(OrganizeButton);
                //OrganizeButton = null;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PostTraderDealScreenClose : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderDealScreen), "Close");
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            try
            {
                if (OrganizeButtonTrader == null) return;
                if (OrganizeButtonTrader.IsDestroyed()) return;

                OrganizeButtonTrader.gameObject.SetActive(false);
                GameObject.Destroy(OrganizeButtonTrader);

                // Might need it.
                //GameObject.DestroyImmediate(OrganizeButton);
                //OrganizeButton = null;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PostMenuScreenInit : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuScreen), "Init");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref DefaultUIButton ____hideoutButton)
        {
            try
            {
                if (OrganizeSprite != null) return;
                //OrganizeSprite = AccessTools.Field(____hideoutButton.GetType(), "_iconSprite").GetValue(____hideoutButton) as Sprite;
                OrganizeSprite = ____hideoutButton.GetFieldValue<Sprite>("_iconSprite");
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    internal class PostInitHanbook : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuTaskBar), "InitHandbook");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref object handbook)
        {
            try
            {
                Organizer.Handbook ??= new Handbook(handbook);
                //Logger.LogMessage($"Elements: {Organizer.Handbook.NodesTree.Count}");
                //var search = Organizer.Handbook.FindNode("5751496424597720a27126da");
                //if (search != null)
                //{
                //    Logger.LogMessage($"Found: {search.Data.Name.Localized()}");
                //    Logger.LogMessage($"Categories: {string.Join(" > ", search.Category.Select(cat => cat.Localized()))}");
                //}
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }

    //internal class PreGClass2429QuickFindAppropriatePlace : ModulePatch
    //{
    //    private static string[] sortClassMethods = new string[] { "Sort", "ApplyItemToRevolverDrum", "ApplySingleItemToAddress", "Fold", "CanRecode", "CanFold" };
    //    private static Type sortClassType = ReflectionHelper.FindClassTypeByMethodNames(sortClassMethods);
    //    private static int callCounter = 0;
    //    protected override MethodBase GetTargetMethod()
    //    {
    //        return AccessTools.Method(sortClassType, "QuickFindAppropriatePlace");
    //    }

    //    [PatchPostfix]
    //    private static void PatchPostfix(Item item, TraderControllerClass controller, IEnumerable<LootItemClass> targets, object order, bool simulate)
    //    {
    //        ++callCounter;
    //        Logger.LogMessage($"!!! CALL NUMBER #{callCounter}");
    //        Logger.LogMessage($"-- Item");
    //        Logger.LogMessage($"Name: {item.RLocalizedName()}");

    //        Logger.LogMessage($"-- Controller");
    //        Logger.LogMessage($"Name: {controller.Name}");
    //        Logger.LogMessage($"ContainerName: {controller.ContainerName}");

    //        Logger.LogMessage($"-- Targets");
    //        foreach(var target in targets)
    //        {
    //            Logger.LogMessage($"Item name: {target.RLocalizedName()}");
    //        }

    //        Logger.LogMessage($"-- Order");
    //        Logger.LogMessage($"Value?: {order}");
    //        Logger.LogMessage($"Type: {order.GetType()}");

    //        Logger.LogMessage($"-- Simulate?");
    //        Logger.LogMessage($"Value: {simulate}");

    //    }
    //}
}
