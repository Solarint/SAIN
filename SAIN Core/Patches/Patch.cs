using Aki.Reflection.Patching;
using ChatShared;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.HandBook;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Weather;
using HarmonyLib;
using JetBrains.Annotations;
using SAIN.Config;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

namespace SAIN.Patches
{
    public class TemplatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            _PropertySetter = AccessTools.PropertySetter(typeof(BotOwner), "Property");
            _Property = AccessTools.Property(typeof(BotOwner), "Property2");

            return AccessTools.Method(typeof(BotOwner), "EFTMethod");
        }

        private static MethodInfo _PropertySetter;
        private static PropertyInfo _Property;

        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return;
        }
    }
    public class TemplatePatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            _PropertySetter = AccessTools.PropertySetter(typeof(BotOwner), "Property");
            _Property = AccessTools.Property(typeof(BotOwner), "Property2");

            return AccessTools.Method(typeof(BotOwner), "EFTMethod");
        }

        private static MethodInfo _PropertySetter;
        private static PropertyInfo _Property;

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            return false;
        }
    }
}
