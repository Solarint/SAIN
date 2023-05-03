using BepInEx.Logging;
using EFT;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.SAIN_Math;
using Movement.Helpers;
using SAIN_Helpers;

namespace Movement.Components
{
    public class BotDecision : MonoBehaviour
    {
        private readonly BotOwner bot;
        public BotDecision(BotOwner botOwner_0)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotDecision));
            bot = botOwner_0;
        }

        protected static ManualLogSource Logger { get; private set; }

        public bool GetDecision()
        {
            if (Execute.Heal())
            {
                return true;
            }
            if (Execute.CancelReload())
            {
                return true;
            }
            if (Execute.Reload())
            {
                return true;
            }
            if (Execute.SprintWhileReload())
            {
                return true;
            }
            return false;
        }

        public DoDecision Execute { get; private set; }

        public class DoDecision
        {
            public DoDecision(BotOwner bot)
            {
                this.bot = bot;
                ShouldI = new ShouldBot(bot);
            }

            private readonly BotOwner bot;
            public ShouldBot ShouldI { get; private set; }

            public bool Heal()
            {
                if (bot.Medecine.Using)
                {
                    return true;
                }

                if (ShouldI.Heal())
                {
                    if (CheckEnemyDistance(out Vector3 movepoint))
                    {
                        bot.Medecine.RefreshCurMeds();
                        if (bot.Medecine.FirstAid.ShallStartUse())
                        {
                            bot.Medecine.FirstAid.TryApplyToCurrentPart(null, null);
                            bot.GoToPoint(movepoint, false, -1f, false, true, true);
                            Logger.LogDebug($"Healing");
                            return true;
                        }
                    }
                }

                return false;
            }

            public bool CancelReload()
            {
                if (ShouldI.CancelReload())
                {
                    bot.WeaponManager.Reload.TryStopReload();
                    Logger.LogDebug($"Stopped reload to shoot");
                    return true;
                }
                return false;
            }

            public bool Reload()
            {
                if (ShouldI.Reload())
                {
                    Logger.LogDebug($"Reloading");
                    bot.WeaponManager.Reload.TryReload();
                    return true;
                }
                return false;
            }

            public bool SprintWhileReload()
            {
                if (ShouldI.Reload() && ShouldI.Sprint())
                {
                    CheckEnemyDistance(out Vector3 position);
                    var coverPoint = bot.Covers.FindClosestPoint(position, false);
                    bot.Steering.LookToMovingDirection();
                    bot.Mover.Sprint(true);
                    bot.Sprint(true);
                    bot.GoToPoint(coverPoint.Position, false, -1f, false, true, true);

                    if (!bot.WeaponManager.Reload.Reloading)
                        bot.WeaponManager.Reload.Reload();

                    Logger.LogDebug($"Sprint Reloading");
                    return true;
                }
                return false;
            }

            public bool CheckEnemyDistance(out Vector3 trgPos)
            {
                Vector3 a = -NormalizeFastSelf(bot.Memory.GoalEnemy.Direction);

                trgPos = Vector3.zero;

                float num = 0f;
                if (NavMesh.SamplePosition(bot.Position + a * 2f / 2f, out NavMeshHit navMeshHit, 1f, -1))
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
                    NavMeshPath navMeshPath_0 = new NavMeshPath();
                    if (NavMesh.CalculatePath(bot.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                    {
                        trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];

                        return CheckStraightDistance(navMeshPath_0, num);
                    }
                }
                return false;
            }

            private bool CheckStraightDistance(NavMeshPath path, float straighDist)
            {
                return path.CalculatePathLength() < straighDist * 1.2f;
            }
        }

        public class ShouldBot
        {
            public ShouldBot(BotOwner bot)
            {
                this.bot = bot;
            }

            private readonly BotOwner bot;

            public bool Heal()
            {
                if (bot.WeaponManager.Reload.Reloading)
                {
                    bot.WeaponManager.Reload.TryStopReload();
                    return false;
                }

                if (bot?.Medecine?.FirstAid != null)
                {
                    if (bot.Medecine.FirstAid.IsBleeding)
                    {
                        if (bot.Medecine.FirstAid.HaveSmth2Use)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool Reload()
            {
                if (bot?.WeaponManager?.Reload == null || bot?.WeaponManager?.CurrentWeapon == null)
                    return false;

                if (!bot.WeaponManager.Reload.Reloading && bot.WeaponManager.Reload.CanReload(false))
                {
                    if (!bot.WeaponManager.HaveBullets)
                        return true;

                    int currentAmmo = bot.WeaponManager.Reload.BulletCount;
                    int maxAmmo = bot.WeaponManager.Reload.MaxBulletCount;
                    float ammoRatio = (float)currentAmmo / maxAmmo; // Cast one of the integers to a float before dividing
                    bool lowAmmo = ammoRatio < 0.3f;

                    if (lowAmmo && bot.Memory.GoalEnemy != null)
                    {
                        if (bot.Memory.GoalEnemy.PersonalLastSeenTime + 2f < Time.time)
                        {
                            Logger.LogDebug($"Ammo: [{currentAmmo}] MaxAmmo: [{maxAmmo}] Ratio: [{ammoRatio}]");
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool Sprint()
            {
                if (!bot.WeaponManager.HaveBullets)
                {
                    if (bot.Memory.GoalEnemy != null && bot.Memory.GoalEnemy.IsVisible)
                    {
                        if (bot.Mover != null && bot.Steering != null)
                            return true;
                    }
                }
                return false;
            }

            public bool CancelReload()
            {
                if (bot?.Memory?.GoalEnemy == null || bot?.WeaponManager?.CurrentWeapon == null)
                    return false;

                if (!Physics.Raycast(bot.MyHead.position, (bot.MyHead.position - bot.Memory.GoalEnemy.CurrPosition), (bot.MyHead.position - bot.Memory.GoalEnemy.CurrPosition).magnitude - 0.25f, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    if (bot.WeaponManager.Reload.Reloading)
                    {
                        if (bot.WeaponManager.HaveBullets)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public bool Attack()
            {
                return false;
            }
        }
    }
}