using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class DebugOverlaySettings : SAINSettingsBase<DebugOverlaySettings>, ISAINSettings
    {
        public bool Overlay_Info = true;
        public bool Overlay_Info_Expanded = false;
        public bool Overlay_Search = true;
        public bool Overlay_EnemyLists = false;
        public bool Overlay_EnemyInfo = true;
        public bool Overlay_EnemyInfo_Expanded = false;
        public bool Overlay_Decisions = false;
        public bool OverLay_AimInfo = false;
        public bool OverLay_AlwaysShowClosestHumanInfo = false;
        public bool OverLay_AlwaysShowMainPlayerInfo = false;
    }

    public class DebugGizmoSettings : SAINSettingsBase<DebugGizmoSettings>, ISAINSettings
    {
        [Name("Draw Debug Gizmos")]
        public bool DrawDebugGizmos;

        [Name("Draw Transform Gizmos")]
        public bool DrawTransformGizmos;

        [Name("Draw Line of Sight Checks")]
        public bool DrawLineOfSightGizmos;

        [Name("Draw Volumetric Light Gizmos")]
        public bool DrawLightGizmos;

        [Name("Draw Door Links")]
        public bool DrawDoorLinks;

        [Name("Draw Recoil Gizmos")]
        public bool DebugDrawRecoilGizmos = false;

        [Name("Draw Aim Gizmos")]
        public bool DebugDrawAimGizmos = false;

        [Name("Draw Blind Corner Raycasts")]
        public bool DebugDrawBlindCorner = false;

        [Name("Draw Debug Suppression Points")]
        [Hidden]
        public bool DebugDrawProjectionPoints = false;

        [Name("Draw Search Peek Start and End Gizmos")]
        public bool DebugSearchGizmos = false;

        [Name("Draw Debug Path Safety Tester")]
        [Hidden]
        [JsonIgnore]
        public bool DebugDrawSafePaths = false;

        [Name("Path Safety Tester")]
        [Hidden]
        [JsonIgnore]
        public bool DebugEnablePathTester = false;

        [Hidden]
        [JsonIgnore]
        public bool DebugMovementPlan = false;
    }

    public class DebugLogSettings : SAINSettingsBase<DebugLogSettings>, ISAINSettings
    {
        [Name("Global Debug Mode")]
        public bool GlobalDebugMode;

        [Name("Global Performance Profiling Mode")]
        [Description("Enables function sampling for Unity Profiling.")]
        public bool GlobalProfilingToggle;

        [Name("Test Bot Sprint Pathfinder")]
        public bool ForceBotsToRunAround;

        [Name("Test Bot Crawling")]
        public bool ForceBotsToTryCrawl;

        [Name("Test Grenade Throw")]
        public bool TestGrenadeThrow;

        [Name("Draw Debug Labels")]
        public bool DrawDebugLabels;

        [Name("Debug External")]
        public bool DebugExternal;

        [Name("Debug Recoil Calculations")]
        public bool DebugRecoilCalculations = false;

        [Name("Debug Aim Calculations")]
        public bool DebugAimCalculations = false;

        [Name("Debug Hearing Calc Results")]
        public bool DebugHearing = false;

        [Name("Debug Extracts")]
        public bool DebugExtract = false;

        [Name("Collect and Export Bot Layer and Brain Info")]
        [Hidden]
        [JsonIgnore]
        public bool CollectBotLayerBrainInfo = false;
    }

    public class DebugSettings : SAINSettingsBase<DebugSettings>, ISAINSettings
    {
        public DebugLogSettings Logs = new DebugLogSettings();
        public DebugGizmoSettings Gizmos = new DebugGizmoSettings();
        public DebugOverlaySettings Overlay = new DebugOverlaySettings();

        public override void Init(List<ISAINSettings> list)
        {
            list.Add(Logs);
            list.Add(Gizmos);
            list.Add(Overlay);
        }
    }
}