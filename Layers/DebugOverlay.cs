using EFT;
using HarmonyLib;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Search;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SAIN.Layers
{
    public static class DebugOverlay
    {
        static DebugOverlay()
        {
            TimeToAim = AccessTools.Field(Helpers.HelpersGClass.AimDataType, "float_7");
            timeAiming = AccessTools.Field(Helpers.HelpersGClass.AimDataType, "float_5");

            int count = 0;
            foreach (var name in _overlayNames) {
                _overlays.Add(count, name);
                count++;
            }
        }

        private static FieldInfo TimeToAim;
        private static FieldInfo timeAiming;

        private static readonly Dictionary<int, string> _overlays = new Dictionary<int, string>();

        private static readonly string[] _overlayNames =
        {
            "Bot Info",
            "Bot Properties",
            "Current Enemy Info",
            "All Enemies Info",
        };

        private static bool _changedOverlay;

        public static void UpdateSelectedOverlay()
        {
            //if (_changedOverlay &&
            //    SAINPlugin.NextDebugOverlay.Value.IsUp() &&
            //    SAINPlugin.PreviousDebugOverlay.Value.IsUp()) {
            //    _changedOverlay = false;
            //}
            //if (_changedOverlay) {
            //    return;
            //}
            //
            //if (SAINPlugin.NextDebugOverlay.Value.IsDown()) {
            //    changeOverlay(true);
            //    _changedOverlay = true;
            //    return;
            //}
            //
            //if (SAINPlugin.PreviousDebugOverlay.Value.IsDown()) {
            //    changeOverlay(false);
            //    _changedOverlay = true;
            //    return;
            //}
        }

        private static void changeOverlay(bool next)
        {
            int count = _overlays.Count - 1;
            if (next) {
                _selectedOverlay++;
                if (_selectedOverlay > count) {
                    _selectedOverlay = 0;
                }
            }
            else {
                _selectedOverlay--;
                if (_selectedOverlay < 0) {
                    _selectedOverlay = count;
                }
            }
            Logger.LogDebug($"{_selectedOverlay} : {next}");
        }

        private static int _selectedOverlay;

        public static void AddBaseInfo(BotComponent bot, BotOwner botOwner, StringBuilder stringBuilder)
        {
            UpdateSelectedOverlay();

            try {
                var debug = SAINPlugin.DebugSettings.Overlay;

                var info = bot.Info;
                if (debug.Overlay_Info) {
                    stringBuilder.AppendLine($"Name: [{bot.Person.Name}] Nickname: [{bot.Player.Profile.Nickname}] Personality: [{info.Personality}] Type: [{info.Profile.WildSpawnType}] PowerLevel: [{info.Profile.PowerLevel}]");
                    stringBuilder.AppendLine(decisionInfo(bot));
                    stringBuilder.AppendLabeledValue("Steering", $"{bot.Steering.CurrentSteerPriority} : {bot.Steering.EnemySteerDir}", Color.white, Color.yellow);
                    stringBuilder.AppendLine($"AILimit [{bot.CurrentAILimit}] : HumanDist: [{bot.AILimit.ClosestPlayerDistanceSqr.Sqrt().Round10()}]");
                    stringBuilder.AppendLine();
                    if (debug.Overlay_Info_Expanded) {
                        stringBuilder.AppendLine($"CoverPoints: [{bot.Cover.CoverPoints.Count}] : StartSearchDelay [{info.TimeBeforeSearch}] : Hold Ground Time [{info.HoldGroundDelay}]");
                        stringBuilder.AppendLine($"Suppression Num: [{bot.Suppression?.SuppressionNumber}] IsSuppressed: [{bot.Suppression?.IsSuppressed}] IsHeavySuppressed: [{bot.Suppression?.IsHeavySuppressed}]");
                        stringBuilder.AppendLine($"Indoors? {bot.Memory.Location.IsIndoors} EnvironmentID: {bot.Player?.AIData.EnvironmentId} In Bunker? {bot.PlayerComponent.AIData.PlayerLocation.InBunker}");
                        var members = bot.Squad.SquadInfo?.Members;
                        if (members != null && members.Count > 1) {
                            stringBuilder.AppendLine($"Squad Personality: [{bot.Squad.SquadInfo.SquadPersonality}]");
                        }
                    }
                    stringBuilder.AppendLine();
                }

                if (debug.Overlay_Decisions) {
                    stringBuilder.AppendLine($"Main Decisn [{bot.Decision.CurrentCombatDecision}] : Last [{bot.Decision.PreviousCombatDecision}]");
                    stringBuilder.AppendLine($"Squad Decisn [{bot.Decision.CurrentSquadDecision}] : Last [{bot.Decision.PreviousSquadDecision}]");
                    stringBuilder.AppendLine($"Self Decisn [{bot.Decision.CurrentSelfDecision}] : Last [{bot.Decision.PreviousSelfDecision}]");
                    stringBuilder.AppendLine($"DecisionReasons");
                    var decisions = bot.Decision.EnemyDecisions.DecisionReasons;
                    stringBuilder.Append(decisions);
                }

                if (debug.Overlay_EnemyLists) {
                    var lists = bot.EnemyController.EnemyLists;
                    var known = lists.GetEnemyList(EEnemyListType.Known);
                    var vis = lists.GetEnemyList(EEnemyListType.Visible);
                    var los = lists.GetEnemyList(EEnemyListType.InLineOfSight);
                    var threats = lists.GetEnemyList(EEnemyListType.ActiveThreats);
                    stringBuilder.AppendLine($"EnemyList[bots/human]: " +
                        $"Known[{known.Bots}/{known.Humans}] " +
                        $"Visible[{vis.Bots}/{vis.Humans}] " +
                        $"InLOS[{los.Bots}/{los.Humans}] " +
                        $"ActvThreat [{threats.Bots}/{threats.Humans}]");
                    stringBuilder.AppendLine();
                }

                if (debug.OverLay_AimInfo) {
                    if (bot.BotOwner.AimingManager.CurrentAiming != null) {
                        stringBuilder.AppendLine($"AimData: Status [{bot.Aim.AimStatus}] " +
                            $"Last Aim Time: [{bot.Aim.LastAimTime}] " +
                            $"AimingTime [{timeAiming.GetValue(bot.BotOwner.AimingManager.CurrentAiming)}] " +
                            $"TimeToFnsh: [{TimeToAim.GetValue(bot.BotOwner.AimingManager.CurrentAiming)}]");
                        stringBuilder.AppendLine($"AimOffsetMagnitude [{((bot.BotOwner.AimingManager.CurrentAiming.RealTargetPoint - bot.BotOwner.AimingManager.CurrentAiming.EndTargetPoint).magnitude).Round100()}] " +
                            $"Friendly Fire Status [{bot.FriendlyFire.FriendlyFireStatus}] " +
                            $"No Bush ESP Status: [{bot.NoBushESP.NoBushESPActive}]");
                        stringBuilder.AppendLine();
                    }
                }

                if (debug.Overlay_EnemyInfo) {
                    Enemy infoToShow = getEnemy2Show(bot);
                    if (infoToShow != null) {
                        CreateEnemyInfo(stringBuilder, infoToShow);
                        stringBuilder.AppendLine();
                    }
                }

                if (debug.Overlay_Search) {
                    var enemyDecisions = bot.Decision.EnemyDecisions;
                    var shallSearch = enemyDecisions.DebugShallSearch;
                    if (shallSearch != null) {
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
            catch (Exception ex) {
                Logger.LogError(ex);
            }
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
            if (debug.OverLay_AlwaysShowClosestHumanInfo) {
                float closest = float.MaxValue;
                foreach (var enemy in bot.EnemyController.Enemies.Values) {
                    if (enemy == null) continue;
                    if (enemy.IsAI) continue;
                    if (enemy.RealDistance < closest) {
                        closest = enemy.RealDistance;
                        closestHuman = enemy;
                    }
                }
            }

            Enemy infoToShow = mainPlayer ?? closestHuman ?? bot.Enemy;
            return infoToShow;
        }

        private static string decisionInfo(BotComponent sain)
        {
            string decisionInfo = string.Empty;
            switch (sain.ActiveLayer) {
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
            if (enemy == null) {
                return;
            }

            stringBuilder.AppendLine($"EnemyData: " +
                $"Name [{enemy.EnemyPlayer?.Profile.Nickname}] " +
                $"RealDistance [{enemy.RealDistance}] " +
                $"Power [{enemy.EnemyIPlayer?.AIData?.PowerOfEquipment}]");

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Visible [{enemy.IsVisible}] Seen [{enemy.Seen}]");
            stringBuilder.AppendLine($"Illuminated [{enemy.Vision.Illuminated}] LightLevel [{enemy.Vision.IlluminationLevel}]");
            stringBuilder.AppendLine();

            stringBuilder.AppendLine($"Aim/Scatter Multi [{enemy.Aim.AimAndScatterMultiplier}]");
            stringBuilder.AppendLabeledValue("Time To Spot", $"{enemy.Vision.LastGainSightResult.Round100()}", Color.white, Color.yellow, true);
            float highestPercent = getPercentSpotted(enemy, out var partType);
            if (highestPercent > 0)
                stringBuilder.AppendLabeledValue("Percent Spotted", $"{partType} : {highestPercent}", Color.white, Color.yellow, true);

            addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastKnownPlace, "Last Known Position");
            if (enemy.Seen && _expandedEnemyInfo) {
                addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastSeenPlace, "Last Seen");
            }

            if (_expandedEnemyInfo) {
                stringBuilder.AppendLine($"HorizAngle [{enemy.Vision.Angles.AngleToEnemyHorizontalSigned.Round100()}] VertiAngle [{enemy.Vision.Angles.AngleToEnemyVerticalSigned.Round100()}]");
                stringBuilder.AppendLine($"GainSightMod [{enemy.Vision.GainSightCoef.Round100()}] VisionDistance [{(enemy.Bot.BotOwner.Settings.FileSettings.Core.VisibleDistance + enemy.Vision.VisionDistance).Round100()}]");
            }
            stringBuilder.AppendLine();

            stringBuilder.AppendLabeledValue("Can Shoot", $"{enemy.Vision.VisionChecker.EnemyParts.CanShoot}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("In Line of Sight", $"{enemy.InLineOfSight}", Color.white, Color.yellow, true);
            if (_expandedEnemyInfo) {
                var parts = enemy.Vision.VisionChecker.EnemyParts.Parts.Values;
                int visCount = 0;
                int partCount = 0;
                int notChecked = 0;
                foreach (var part in parts) {
                    if (part.TimeSinceLastVisionCheck > 2f) {
                        notChecked++;
                        continue;
                    }
                    partCount++;
                    if (part.LineOfSight) visCount++;
                }
                stringBuilder.AppendLabeledValue("Body Parts", $"In LOS: {visCount} : Checked: {partCount} : Not Checked: {notChecked}", Color.white, Color.yellow, true);
            }
            stringBuilder.AppendLine();

            stringBuilder.AppendLine($"Heard [{enemy.Heard}] Recently? [{enemy.Status.HeardRecently}]");
            if (enemy.Heard && _expandedEnemyInfo) {
                addPlaceInfo(stringBuilder, enemy.KnownPlaces.LastHeardPlace, "Last Heard");
            }
        }

        private static float getPercentSpotted(Enemy enemy, out BodyPartType partType)
        {
            float highestPercent = enemy.EnemyInfo.BodyData().Value?.GetVisibilityLevel() ?? 0f;
            partType = BodyPartType.body;
            foreach (var part in enemy.EnemyInfo.AllActiveParts) {
                float percent = part.Value.GetVisibilityLevel();
                if (percent > highestPercent) {
                    highestPercent = percent;
                    partType = part.Key.BodyPartType;
                }
            }

            highestPercent = Mathf.Clamp(highestPercent.Round100(), 0f, 100f);
            return highestPercent;
        }

        private static void addPlaceInfo(StringBuilder stringBuilder, EnemyPlace place, string name)
        {
            if (place != null) {
                stringBuilder.AppendLine($"{name} Data");
                stringBuilder.AppendLabeledValue("Time Since Updated", $"{place.TimeSincePositionUpdated.Round100()}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Enemy Distance", $"{place.DistanceToEnemyRealPosition.Round100()}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Bot Distance", $"{place.DistanceToBot.Round100()}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Searched", $"Personal: {place.HasArrivedPersonal} / Squad: {place.HasArrivedSquad}", Color.white, Color.yellow, true);
                stringBuilder.AppendLine();
            }
        }
    }
}