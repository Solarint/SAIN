using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using System;
using System.Text;
using UnityEngine;

namespace SAIN.Layers;

public static class DebugOverlay
{
    public static void AddBaseInfo(BotComponent bot, BotOwner botOwner, StringBuilder stringBuilder)
    {
        try
        {
            var debug = SAINPlugin.DebugSettings.Overlay;

            var info = bot.Info;
            if (debug.Overlay_Info)
            {
                stringBuilder.AppendLine($"Name: [{bot.PlayerComponent.Name}] Nickname: [{bot.Player.Profile.Nickname}] Personality: [{info.Personality}] Type: [{info.Profile.WildSpawnType}] PowerLevel: [{info.Profile.PowerLevel}]");
                stringBuilder.AppendLine($"In Combat [{bot.IsInCombat}] Layers Active [{bot.SAINLayersActive}]", Color.white);
                var enemy = bot.GoalEnemy;
                if (enemy != null)
                {
                    stringBuilder.AppendLine($"Goal Enemy: [{enemy.EnemyName}] Distance: [{enemy.RealDistance.Round100()}] IsVisible: [{enemy.IsVisible}]");
                    stringBuilder.AppendLine($"VisiblePathPoint: [{enemy.VisiblePathPoint}] Dist2Bot: [{enemy.VisiblePathPointDistanceToBot}] DistToLN: [{enemy.VisiblePathPointDistanceToEnemyLastKnown}]");
                    stringBuilder.AppendLine($"Path: [{enemy.Path.PathToEnemyStatus}] Nodes: [{enemy.Path.AllPathNodeCount}] corners: [{enemy.Path.PathCorners?.Length}]");
                    stringBuilder.AppendLine($" Enemy Statuses: " +
                        $"VULN: [{enemy.Status.VulnerableAction}] " +
                        $"GREN: [{enemy.Status.EnemyHasGrenadeOut}] " +
                        $"HEAL: [{enemy.Status.EnemyIsHealing}] " +
                        $"LOOT:[{enemy.Status.EnemyIsLooting}] " +
                        $"REL:[{enemy.Status.EnemyIsReloading}] " +
                        $"LOOK:[{enemy.Status.EnemyLookAtMe}] " +
                        $"SUR:[{enemy.Status.EnemyUsingSurgery}] " +
                        $"HEAR:[{enemy.Status.HeardRecently}] " +
                        $"SHOTATME:[{enemy.Status.ShotAtMe}:{enemy.Status.ShotAtMeRecently}] " +
                        $"SHOTME:[{enemy.Status.ShotMe}]" +
                        $"POINT:[{enemy.Status.PointingWeaponAtMe}]");
                }
                else
                {
                    stringBuilder.AppendLine($"Goal Enemy: [null]");
                }
                stringBuilder.AppendLabeledValue("Steering", $"{bot.Steering.CurrentSteerPriority} : {bot.Steering.EnemySteerDir}", Color.white, Color.yellow);
                stringBuilder.AppendLine(decisionInfo(bot));
                stringBuilder.AppendLabeledValue("DogFight Status", $"{bot.Mover.DogFight.Status}", Color.white, Color.yellow);
                string poseInfo = $"Pose [{bot.Mover.Pose.PoseValue.LastSmoothedValue}:{bot.Mover.Pose.PoseValue.TargetValue}]";
                string speedInfo = $"Speed [{bot.Mover.Pose.SpeedValue.LastSmoothedValue} : {bot.Mover.Pose.SpeedValue.TargetValue}]";
                stringBuilder.AppendLine($"[{poseInfo}] [{speedInfo}]", Color.white);
                stringBuilder.AppendLabeledValue("NavMeshStatus", $"{bot.Transform.NavData.Status}", Color.white, Color.yellow);
                stringBuilder.AppendLabeledValue("TimeSinceOnNavMesh", $"{Time.time - bot.Transform.NavData.TimeLastOnNavMesh}", Color.white, Color.yellow);

                if (debug.Overlay_Info_Expanded)
                {
                    AddMoveData(bot, stringBuilder);
                    stringBuilder.AppendLine($"" +
                        $"Lean [{bot.Mover.Lean.LeanAngleValue.LastSmoothedValue}:{bot.Mover.Lean.LeanAngleValue.TargetValue}] " +
                        $"CanLeanByState:{bot.Mover.Lean.CanLeanByState} " +
                        $"CurrentLeanSetting:{bot.Mover.Lean.LeanDirection}",
                        Color.white);
                    //stringBuilder.AppendLine($"AILimit [{bot.CurrentAILimit}] : HumanDist: [{bot.AILimit.ClosestPlayerDistanceSqr.Sqrt().Round10()}]");
                    stringBuilder.AppendLine($"Suppression Num: [{bot.Suppression?.SuppressionNumber}] State: [{bot.Suppression?.CurrentState}] Last State: [{bot.Suppression?.LastState}]");
                    stringBuilder.AppendLine($"CoverPoints: [{bot.Cover.CoverPoints.Count}] : StartSearchDelay [{info.TimeBeforeSearch}] : Hold Ground Time [{info.HoldGroundDelay}]");
                    stringBuilder.AppendLine($"Indoors? {bot.Memory.Location.IsIndoors} EnvironmentID: {bot.Player?.AIData.EnvironmentId} In Bunker? {bot.PlayerComponent.AIData.PlayerLocation.InBunker}");
                    var members = bot.Squad.SquadInfo?.Members;
                    if (members != null && members.Count > 1)
                    {
                        stringBuilder.AppendLine($"Squad Personality: [{bot.Squad.SquadInfo.SquadPersonality}]");
                    }
                }
            }

            if (debug.Overlay_Decisions)
            {
                stringBuilder.AppendLine($"Main Decisn [{bot.Decision.CurrentCombatDecision}] : Last [{bot.Decision.PreviousCombatDecision}]");
                stringBuilder.AppendLine($"Squad Decisn [{bot.Decision.CurrentSquadDecision}] : Last [{bot.Decision.PreviousSquadDecision}]");
                stringBuilder.AppendLine($"Self Decisn [{bot.Decision.CurrentSelfDecision}] : Last [{bot.Decision.PreviousSelfDecision}]");
                stringBuilder.AppendLine($"DecisionReasons");
                var decisions = bot.Decision.EnemyDecisions.DecisionReasons;
                stringBuilder.Append(decisions);
            }

            if (debug.Overlay_EnemyLists)
            {
                var enemyCon = bot.EnemyController;
                var known = enemyCon.KnownEnemies;
                var vis = enemyCon.VisibleEnemies;
                var los = enemyCon.EnemiesInLineOfSight;
                stringBuilder.AppendLine($"EnemyList[bots/human]: " +
                    $"Known[{known.Bots}/{known.Humans}] " +
                    $"Visible[{vis.Bots}/{vis.Humans}] " +
                    $"InLOS[{los.Bots}/{los.Humans}]");
            }

            if (debug.OverLay_AimInfo)
            {
                if (bot.BotOwner.AimingManager.CurrentAiming is BotAimingClass aimClass)
                {
                    stringBuilder.AppendLine($"AimData: Status [{bot.Aim.AimStatus}] " +
                        $"Last Aim Time: [{bot.Aim.LastAimTime}]");
                    stringBuilder.AppendLine($"AimOffsetMagnitude [{((bot.BotOwner.AimingManager.CurrentAiming.RealTargetPoint - bot.BotOwner.AimingManager.CurrentAiming.EndTargetPoint).magnitude).Round100()}] " +
                        $"Friendly Fire Status [{bot.FriendlyFire.FriendlyFireStatus}] " +
                        $"No Bush ESP Status: [{bot.NoBushESP.NoBushESPActive}]");
                }
            }

            if (debug.Overlay_EnemyInfo)
            {
                Enemy infoToShow = getEnemy2Show(bot);
                if (infoToShow != null)
                {
                    CreateEnemyInfo(stringBuilder, infoToShow);
                }
            }

            if (debug.Overlay_Search)
            {
                var enemyDecisions = bot.Decision.EnemyDecisions;
                var shallSearch = enemyDecisions.DebugShallSearch;
                if (shallSearch != null)
                {
                    if (shallSearch == true)
                        stringBuilder.AppendLabeledValue("Searching",
                            $"Current State: {bot.Search.CurrentState} " +
                            $"Next: {bot.Search.NextState} " +
                            $"Last: {bot.Search.LastState}",
                            Color.white, Color.yellow, true);

                    var reasons = enemyDecisions.DebugSearchReasons;
                    var wantReasons = reasons.WantSearchReasons;
                    stringBuilder.AppendLabeledValue("Want Search Reasons",
                        $"[WantToSearchReason : {wantReasons.WantToSearchReason}] " +
                        $"[NotWantToSearchReason: {wantReasons.NotWantToSearchReason}] " +
                        $"[CantStartReason: {wantReasons.CantStartReason}]",
                        Color.white, Color.yellow, true);

                    if (reasons.NotSearchReason != SearchReasonsStruct.ENotSearchReason.None)
                        stringBuilder.AppendLabeledValue("Not Search Reason",
                            $"{reasons.NotSearchReason}",
                            Color.white, Color.yellow, true);

                    if (!reasons.PathCalcFailReason.IsNullOrEmpty())
                        stringBuilder.AppendLabeledValue("CalcPath Fail Reason",
                            $"{reasons.PathCalcFailReason}",
                            Color.white, Color.yellow, true);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    public static void AddMoveData(BotComponent bot, StringBuilder stringBuilder)
    {
        var moveData = bot.Mover.ActivePath;
        if (moveData == null) return;
        stringBuilder.AppendLabeledValue("Move Status", $"{moveData.Status}", Color.white, Color.yellow);
        stringBuilder.AppendLabeledValue("Moving", $"{moveData.Moving}", Color.white, Color.yellow);
        //stringBuilder.AppendLabeledValue("Crawling", $"{moveData.Crawling}", Color.white, Color.yellow);
        stringBuilder.AppendLabeledValue("Running", $"{moveData.Running}", Color.white, Color.yellow);
        stringBuilder.AppendLabeledValue("Want To Sprint : Reason", $"{moveData.WantToSprint} : {moveData.SprintReason}", Color.white, Color.yellow);
        stringBuilder.AppendLabeledValue("Sprint Status", $"{moveData.CurrentSprintStatus}", Color.white, Color.yellow);
        //stringBuilder.AppendLabeledValue("Move CornerCount", $"{moveData.PathCorners.Count}", Color.white, Color.yellow);
        //stringBuilder.AppendLabeledValue("Move CurrentIndex", $"{moveData.CurrentIndex}", Color.white, Color.yellow);
        //stringBuilder.AppendLabeledValue("Move Corner Distance", $"{moveData.CurrentCornerDistanceSqr.Sqrt()}", Color.white, Color.yellow);
        //stringBuilder.AppendLabeledValue("Time Since Start Corner", $"{Time.time - moveData.GetCurrentCorner().TimeStarted}", Color.white, Color.yellow);
    }

    private static Enemy getEnemy2Show(BotComponent bot)
    {
        var debug = SAINPlugin.DebugSettings.Overlay;
        Enemy mainPlayer = null;
        if (debug.OverLay_AlwaysShowMainPlayerInfo)
            foreach (var enemy in bot.EnemyController.Enemies.Values)
                if (enemy?.EnemyPlayer.IsYourPlayer == true)
                    mainPlayer = enemy;

        Enemy closestHuman = null;
        if (debug.OverLay_AlwaysShowClosestHumanInfo)
        {
            float closest = float.MaxValue;
            foreach (var enemy in bot.EnemyController.Enemies.Values)
            {
                if (enemy == null) continue;
                if (enemy.IsAI) continue;
                if (enemy.RealDistance < closest)
                {
                    closest = enemy.RealDistance;
                    closestHuman = enemy;
                }
            }
        }

        Enemy infoToShow = mainPlayer ?? closestHuman ?? bot.GoalEnemy;
        return infoToShow;
    }

    private static string decisionInfo(BotComponent sain)
    {
        string decisionInfo = string.Empty;
        switch (sain.ActiveLayer)
        {
            case ESAINLayer.None:
                break;

            case ESAINLayer.AvoidThreat:
            case ESAINLayer.Combat:
                decisionInfo = $"MainDcsn: [{sain.Decision.CurrentCombatDecision}] : Layer [{sain.ActiveLayer}]";
                break;

            case ESAINLayer.Squad:
                decisionInfo = $"SqdDcsn: [{sain.Decision.CurrentSquadDecision}] : Layer [{sain.ActiveLayer}]";
                break;

            case ESAINLayer.Extract:
                decisionInfo = $"Extract: [{sain.Memory.Extract.ExtractReason}][{sain.Memory.Extract.ExtractStatus}] : Layer [{sain.ActiveLayer}]";
                break;

            default:
                break;
        }
        return decisionInfo;
    }

    private static bool _expandedEnemyInfo => SAINPlugin.DebugSettings.Overlay.Overlay_EnemyInfo_Expanded;

    private static void CreateEnemyInfo(StringBuilder stringBuilder, Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        stringBuilder.AppendLine($"EnemyData: " +
            $"Name [{enemy.EnemyPlayer?.Profile.Nickname}] " +
            $"RealDistance [{enemy.RealDistance}] " +
            $"Power [{enemy.EnemyPlayer?.AIData?.PowerOfEquipment}]");

        stringBuilder.AppendLine($"Visible [{enemy.IsVisible}] Seen [{enemy.Seen}]");

        stringBuilder.AppendLine($"Aim/Scatter Multi [{enemy.Aim.AimAndScatterMultiplier}]");
        stringBuilder.AppendLabeledValue("LastGainSightResult", $"{(enemy.Vision.LastGainSightResult).Round100()}", Color.white, Color.yellow, true);
        /*
        float highestPercent = getPercentSpotted(enemy, out var partType);
        if (highestPercent > 0)
            stringBuilder.AppendLabeledValue("Percent Spotted", $"{partType} : {highestPercent}", Color.white, Color.yellow, true);
        */

        addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastKnownPlace, "Last Known Position");
        if (enemy.Seen && _expandedEnemyInfo)
        {
            addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastSeenPlace, "Last Seen");
        }

        if (_expandedEnemyInfo)
        {
            stringBuilder.AppendLine($"HorizAngle [{enemy.Vision.Angles.AngleToEnemyHorizontalSigned.Round100()}] VertiAngle [{enemy.Vision.Angles.AngleToEnemyVerticalSigned.Round100()}]");
            stringBuilder.AppendLine($"GainSightMod [{EnemyGainSightClass.GetGainSightModifier(enemy).Round1000()}] VisionDistance [{(enemy.Bot.BotOwner.Settings.FileSettings.Core.VisibleDistance + enemy.Vision._visionDistance.Value).Round100()}]");
        }
        stringBuilder.AppendLine();
        
        stringBuilder.AppendLabeledValue("Can Be Seen", $"{enemy.Vision.Angles.CanBeSeen} : {enemy.Vision.EnemyParts.CanBeSeen}", Color.white, Color.yellow, true);
        stringBuilder.AppendLabeledValue("Can Shoot", $"{enemy.CanShoot}", Color.white, Color.yellow, true);
        stringBuilder.AppendLabeledValue("In Line of Sight", $"{enemy.InLineOfSight}", Color.white, Color.yellow, true);
        if (_expandedEnemyInfo)
        {
            var parts = enemy.Vision.EnemyParts.Parts.Values;
            int losCount = 0;
            int visCount = 0;
            int shootCount = 0;
            int partCount = 0;
            foreach (var part in parts)
            {
                partCount++;
                if (part.LineOfSight) losCount++;
                if (part.CanBeSeen) visCount++;
                if (part.CanShoot) shootCount++;
            }
            stringBuilder.AppendLabeledValue("Body Part Vision", $"[{partCount},{losCount},{visCount},{shootCount}]", Color.white, Color.yellow, true);
        }
        stringBuilder.AppendLine();

        stringBuilder.AppendLine($"Heard [{enemy.Heard}] Recently? [{enemy.Status.HeardRecently}]");
        if (enemy.Heard && _expandedEnemyInfo)
        {
            addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastHeardPlace, "Last Heard");
        }
    }

    /*
    private static float getPercentSpotted(Enemy enemy, out BodyPartType partType)
    {

        float highestPercent = enemy.EnemyInfo.BodyData().Value?.GetVisibilityLevel() ?? 0f;
        partType = BodyPartType.body;
        foreach (var part in enemy.EnemyInfo.AllActiveParts)
        {
            float percent = part.Value.GetVisibilityLevel();
            if (percent > highestPercent)
            {
                highestPercent = percent;
                partType = part.Key.BodyPartType;
            }
        }

        highestPercent = Mathf.Clamp(highestPercent.Round100(), 0f, 100f);
        return highestPercent;
    }
    */

    private static void addPlaceInfo(StringBuilder stringBuilder, EnemyPlace place, string name)
    {
        if (place != null)
        {
            stringBuilder.AppendLine($"{name} Data");
            stringBuilder.AppendLabeledValue("Time Since Updated", $"{place.TimeSincePositionUpdated.Round100()}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Enemy Distance", $"{place.DistanceToEnemyRealPosition.Round100()}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Bot Distance", $"{place.DistanceToBot.Round100()}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Searched", $"Personal: {place.HasArrivedPersonal} / Squad: {place.HasArrivedSquad}", Color.white, Color.yellow, true);
            stringBuilder.AppendLine();
        }
    }
}