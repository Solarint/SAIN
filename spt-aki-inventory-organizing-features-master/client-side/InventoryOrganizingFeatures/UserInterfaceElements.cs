using EFT.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InventoryOrganizingFeatures.Organizer;
using InventoryOrganizingFeatures.Reflections;

namespace InventoryOrganizingFeatures
{
    internal static class UserInterfaceElements
    {
        public static Button OrganizeButtonStash { get; set; } = null;
        public static Button OrganizeButtonTrader { get; set; } = null;
        public static Sprite OrganizeSprite { get; set; } = null;

        public static Button SetupOrganizeButton(Button sourceForCloneButton, LootItemClass item, InventoryControllerClass controller)
        {
            var clone = GameObject.Instantiate(sourceForCloneButton, sourceForCloneButton.transform.parent);
            clone.onClick.RemoveAllListeners();

            // - Using async organizing causes issues with UI (index out of bounds)
            // - so there's no reason to use inProgress indicator.
            // Use GridSortPanel's progress indicator.
            //var gridSortPanelSetInProgress = __instance
            //    .GetType()
            //    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            //    .Where(method =>
            //    {
            //        var args = method.GetParameters();
            //        if(args.Length != 1) return false;

            //        var firstParam = args.First();
            //        return firstParam.ParameterType == typeof(bool) && firstParam.Name.Equals("inProgress");
            //    })
            //    .First(); // let it throw exception if somehow method wasn't found.

            clone.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                try
                {
                    //Reflected invoke of ItemUiContext.Instance.ShowMessageWindow() because it returns a GClass2709(as of SPT-AKI 3.5.3);
                    //If used often, should be moved into a special helper method.
                    var showMessageWindowArgs = new object[]
                    {
                        "Do you want to organize all items by tagged containers?",
                        new Action(() =>
                        {
                            //gridSortPanelSetInProgress.Invoke(__instance, new object[] { true });
                            Organize(item, controller);
                            //gridSortPanelSetInProgress.Invoke(__instance, new object[] { false });
                        }),
                        new Action(DoNothing),
                    };
                    var showMessageWindowArgTypes = new Type[]
                    {
                        typeof(string), // description
                        typeof(Action), // acceptAction
                        typeof(Action), // cancelAction
                        typeof(string), // caption
                        typeof(float), // time
                        typeof(bool), // forceShow
                        typeof(TextAlignmentOptions), // alignment
                    };
                    ReflectionHelper.InvokeMethod(
                        ItemUiContext.Instance,
                        "ShowMessageWindow",
                        showMessageWindowArgs,
                        showMessageWindowArgTypes
                    );
                }
                catch (Exception ex)
                {
                    throw Plugin.ShowErrorNotif(ex);
                }
            }));

            // For stash panel
            var childImage = clone.transform.Find("Image");
            if (childImage != null)
            {
                //Logger.LogMessage($"Sprite path: {childImage.GetComponent<UnityEngine.UI.Image>().path}")
                //childImage.GetComponent<UnityEngine.UI.Image>().sprite = OrganizeSprite; - looks badly stretched
                // Just replace the background image with the new one, sort of looks better.
                foreach (Transform child in clone.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                clone.transform.DetachChildren();
                clone.image.sprite = OrganizeSprite;
            }
            else
            {
                // For container view panel
                foreach (Transform child in clone.transform)
                {
                    if (child.name.Equals("SortIcon"))
                    {
                        child.GetComponent<UnityEngine.UI.Image>().sprite = OrganizeSprite;
                    }
                    if (child.name.Equals("Text"))
                    {
                        child.GetComponent<CustomTextMeshProUGUI>().text = "ORG.";
                    }
                }
            }

            clone.gameObject.SetActive(true);

            return clone;
        }

        private const string DefaultInventoryId = "55d7217a4bdc2d86028b456d";
        public static Button SetupTakeOutButton(Button sourceForCloneButton, LootItemClass item, InventoryControllerClass controller)
        {
            var clone = GameObject.Instantiate(sourceForCloneButton, sourceForCloneButton.transform.parent);
            clone.onClick.RemoveAllListeners();

            clone.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                try
                {
                    var showMessageWindowArgs = new object[]
                    {
                        "Do you want to take out all items from this container?",
                        new Action(() =>
                        {
                            // Use reverse organizing with ignoreParams = true
                            // When ignoreParams is true it basically just moves items

                            // Check if parent is DefaultInventoryId. It's applicable on items which are equipped on PMC.
                            var parent = item.Parent.Container.ParentItem;
                            new OrganizedContainer(parent.TemplateId == DefaultInventoryId ? controller.Inventory.Stash :  (LootItemClass)parent, item, controller).Organize(true);
                        }),
                        new Action(DoNothing),
                    };
                    var showMessageWindowArgTypes = new Type[]
                    {
                        typeof(string), // description
                        typeof(Action), // acceptAction
                        typeof(Action), // cancelAction
                        typeof(string), // caption
                        typeof(float), // time
                        typeof(bool), // forceShow
                        typeof(TextAlignmentOptions), // alignment
                    };
                    ReflectionHelper.InvokeMethod(
                        ItemUiContext.Instance,
                        "ShowMessageWindow",
                        showMessageWindowArgs,
                        showMessageWindowArgTypes
                    );
                }
                catch (Exception ex)
                {
                    throw Plugin.ShowErrorNotif(ex);
                }
            }));

            // For container view panel
            foreach (Transform child in clone.transform)
            {
                if (child.name.Equals("SortIcon"))
                {
                    child.GetComponent<UnityEngine.UI.Image>().sprite = OrganizeSprite;
                }
                if (child.name.Equals("Text"))
                {
                    child.GetComponent<CustomTextMeshProUGUI>().text = "\u21e7T/O";
                }
            }
            
            clone.gameObject.SetActive(true);

            return clone;
        }
        private static void DoNothing()
        {
            // Empty method is used to do nothing and close message window,
            // since simply passing null doesn't work.
        }
    }
}
