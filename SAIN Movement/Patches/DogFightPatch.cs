using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Audio.Movement.Helpers;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Audio.Movement.Config.DebugConfig;
using static SAIN_Audio.Movement.Config.DogFighterConfig;
using SAIN_Helpers;

namespace SAIN_Audio.Movement.Patches
{
    public class DogFight
    {
        public class Start : ModulePatch
        {
            private static PropertyInfo _DogFightProperty;
            private static MethodInfo _DogFightState;
            protected override MethodBase GetTargetMethod()
            {
                _DogFightProperty = AccessTools.Property(typeof(BotOwner), "DogFight");

                _DogFightState = AccessTools.PropertySetter(_DogFightProperty.PropertyType, "DogFightState");

                return AccessTools.Method(_DogFightProperty.PropertyType, "ShallStartCauseHavePlace");
            }

            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref NavMeshPath ___navMeshPath_0)
            {
                var goalEnemy = ___botOwner_0.Memory.GoalEnemy;
                if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.Distance < 25f)
                {
                    if (Backup(___botOwner_0, ___navMeshPath_0, out Vector3 trgPos))
                    {
                        _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.dogFight });
                        return true;
                    }
                }
                return false;
            }
        }
        public class ManualUpdate : ModulePatch
        {
            private static PropertyInfo _DogFightProperty;
            private static MethodInfo _DogFightState;
            protected override MethodBase GetTargetMethod()
            {
                _DogFightProperty = AccessTools.Property(typeof(BotOwner), "DogFight");

                _DogFightState = AccessTools.PropertySetter(_DogFightProperty.PropertyType, "DogFightState");

                return AccessTools.Method(_DogFightProperty.PropertyType, "ManualUpdate");
            }
            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref NavMeshPath ___navMeshPath_0, ref float ___float_2)
            {
                var goalEnemy = ___botOwner_0.Memory.GoalEnemy;
                if (goalEnemy == null)
                {
                    _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.none });
                    return false;
                }
                if (___botOwner_0.Memory.BotCurrentCoverInfo.UseDogFight(3f))
                {
                    _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.dogFight });
                    return false;
                }
                switch (_DogFightProperty.GetValue(___botOwner_0))
                {
                    case BotDogFightStatus.none:
                        if (goalEnemy.IsVisible && goalEnemy.Distance < 25f)
                        {
                            if (Backup(___botOwner_0, ___navMeshPath_0, out Vector3 vector))
                            {
                                _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.dogFight });
                                return false;
                            }
                            _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.shootFromPlace });
                            return false;
                        }
                        break;
                    case BotDogFightStatus.dogFight:
                        if (goalEnemy.Distance > ___botOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_OUT)
                        {
                            _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.none });
                            return false;
                        }
                        break;
                    case BotDogFightStatus.shootFromPlace:
                        if (___float_2 < Time.time)
                        {
                            ___float_2 = Time.time + 1f;
                            if (Backup(___botOwner_0, ___navMeshPath_0, out Vector3 vector))
                            {
                                _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.none });
                            }
                        }
                        if (goalEnemy.Distance > 25f)
                        {
                            _DogFightState.Invoke(___botOwner_0, new object[] { BotDogFightStatus.none });
                        }
                        break;
                    default:
                        return false;
                }
                return false;
            }
        }
        public class Fight : ModulePatch
        {
            private static PropertyInfo _DogFightProperty;
            private static MethodInfo _DogFightState;
            private static MethodInfo _DogFightStateSetter;
            protected override MethodBase GetTargetMethod()
            {
                _DogFightProperty = AccessTools.Property(typeof(BotOwner), "DogFight");

                _DogFightState = AccessTools.PropertySetter(_DogFightProperty.PropertyType, "DogFightState");

                return AccessTools.Method(_DogFightProperty.PropertyType, "Fight");
            }
            [PatchPrefix]
            public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_1, ref bool ___bool_0, ref NavMeshPath ___navMeshPath_0)
            {
                BotOwner bot = ___botOwner_0;
                SainMemory sain = bot.gameObject.GetComponent<SainMemory>();

                if (!DodgeToggle.Value || bot.Memory.GoalEnemy == null)
                {
                    return true;
                }

                //Check if Scav Can Dodge value is set to true, need to expand and do this better
                if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault))
                {
                    return true;
                }

                if (sain.DodgeTimer < Time.time)
                {
                    sain.DodgeTimer = Time.time + UnityEngine.Random.Range(0.5f, 1f);

                    if (bot.Memory.GoalEnemy.CanShoot)
                    {
                        if (Dodge.ExecuteDodge(bot))
                        {
                            if (DebugDodge.Value) Logger.LogInfo($"[{bot.name}] Dodged and is shooting");

                            _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.shootFromPlace });

                            return false;
                        }
                    }
                }
                if (___float_1 < Time.time)
                {
                    var goalEnemy = bot.Memory.GoalEnemy;

                    if (goalEnemy == null)
                    {
                        ___bool_0 = false;
                        return false;
                    }

                    ___float_1 = Time.time + 0.2f;

                    if (!___bool_0 && Backup(bot, ___navMeshPath_0, out Vector3 position))
                    {
                        bot.GoToPoint(position, true, -1f, false, true, true);
                        return false;
                    }

                    if (goalEnemy.CanShoot)
                    {
                        ___bool_0 = false;
                        _DogFightStateSetter.Invoke(bot, new object[] { BotDogFightStatus.shootFromPlace });
                        return false;
                    }

                    ___bool_0 = true;

                    bot.MoveToEnemyData.TryMoveToEnemy(goalEnemy.CurrPosition);
                }
                return false;
            }
        }
        // Method_2
        private static bool CheckPathLength(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }
        // Method_1
        public static bool Backup(BotOwner bot, NavMeshPath navMeshPath_0, out Vector3 trgPos)
        {
            Vector3 a = -SAIN_Math.NormalizeFastSelf(bot.Memory.GoalEnemy.Direction);
            trgPos = Vector3.zero;
            float num = 0f;
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(bot.Position + a * 2f / 2f, out navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;
                Vector3 a2 = trgPos - bot.Position;
                float magnitude = a2.magnitude;
                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;
                    num = magnitude;
                    if (NavMesh.SamplePosition(bot.Position + a3 * 2f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;
                        num = (trgPos - bot.Position).magnitude;
                    }
                }
            }
            if (num != 0f && num > bot.Settings.FileSettings.Move.REACH_DIST)
            {
                navMeshPath_0.ClearCorners();
                if (NavMesh.CalculatePath(bot.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];
                    return CheckPathLength(navMeshPath_0, num);
                }
            }
            return false;
        }
    }
}
