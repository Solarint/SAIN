using BepInEx.Logging;
using SAIN.Helpers;
using EFT;
using UnityEngine;
using static SAIN.UserSettings.SoundConfig;
using Comfort.Common;
using EFT.InventoryLogic;
using SAIN.Components;

namespace SAIN.Classes
{
    public class HearingSensorClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Singleton<GClass635>.Instance.OnSoundPlayed += HearSound;
        }

        public void OnDestroy()
        {
            Singleton<GClass635>.Instance.OnSoundPlayed -= HearSound;
        }

        public void HearSound(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (BotOwner == null || SAIN == null) return;

            if (!SAIN.GameIsEnding)
            {
                EnemySoundHeard(player, position, power, type);
            }
        }

        private void EnemySoundHeard(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (player != null)
            {
                if (BotOwner.ProfileId == player.ProfileId)
                {
                    return;
                }

                if (CheckSoundHeardAfterModifiers(player, position, power, type, out float distance))
                {
                    bool gunsound = type == AISoundType.gun || type == AISoundType.silencedGun;

                    if (!BotOwner.BotsGroup.IsEnemy(player) && BotOwner.BotsGroup.Neutrals.ContainsKey(player))
                    {
                        //BotOwner.BotsGroup.LastSoundsController.AddNeutralSound(player, position);
                        return;
                    }

                    if (gunsound || IsSoundClose(distance))
                    {
                        if (distance < 5f)
                        {
                            // Note: This can throw an exception sometimes, we'll just ignore it for now
                            try
                            {
                                BotOwner.Memory.Spotted(false, null, null);
                            }
                            catch { }
                        }

                        ReactToSound(player, position, distance, true, type);
                    }
                }
            }
            else
            {
                float distance = (BotOwner.Transform.position - position).magnitude;
                ReactToSound(null, position, distance, distance < power, type);
            }
        }

        private bool CheckSoundHeardAfterModifiers(IAIDetails player, Vector3 position, float power, AISoundType type, out float distance)
        {
            float range = power;
            bool wasHeard = DoIHearSound(player, position, range, type, out distance, true);

            if (wasHeard)
            {
                if (SAIN.Equipment.HasEarPiece)
                {
                    range *= 1.33f;
                }
                else
                {
                    range *= 0.9f;
                }
                if (SAIN.Equipment.HasHeavyHelmet)
                {
                    range *= 0.66f;
                }
                if (SAIN.HealthStatus == ETagStatus.Dying)
                {
                    range *= 0.55f;
                }
                var move = BotOwner.GetPlayer.MovementContext;
                float speed = move.ClampedSpeed / move.MaxSpeed;
                if (BotOwner.GetPlayer.IsSprintEnabled && speed >= 0.9f)
                {
                    range *= 0.66f;
                }
                else if (speed > 0.66f)
                {
                    range *= 0.85f;
                }
                else if (speed <= 0.1f)
                {
                    range *= 1.25f;
                }

                if (DebugSound.Value)
                {
                    Logger.LogDebug($" Sound modifier results before clamp: Original:[{power}] Modified:[{range}] Sprinting? [{BotOwner.GetPlayer.IsSprintEnabled}] MyMoveSpeed 0 to 1: [{speed}] Health: [{SAIN.HealthStatus}] Heavy Helmet? [{SAIN.Equipment.HasHeavyHelmet}] Headphones? [{SAIN.Equipment.HasEarPiece}]");
                }

                range = Mathf.Clamp(range, power / 3f, power * 1.33f);

                if (DebugSound.Value)
                {
                    Logger.LogDebug($" Sound modifier results post clamp: Original: [{power}] Modified: [{range}] ");
                }

                return DoIHearSound(player, position, range, type, out distance, false);
            }

            return false;
        }

        private void ReactToSound(IAIDetails person, Vector3 pos, float power, bool wasHeard, AISoundType type)
        {
            if (person != null && person.AIData.IsAI && BotOwner.BotsGroup.Contains(person.AIData.BotOwner))
            {
                return;
            }

            float bulletfeeldistance = 500f;
            Vector3 shooterDirection = BotOwner.Transform.position - pos;
            float shooterDistance = shooterDirection.magnitude;
            bool isGunSound = type == AISoundType.gun || type == AISoundType.silencedGun;
            bool bulletfelt = shooterDistance < bulletfeeldistance;

            if (wasHeard)
            {
                float dispersion = (type == AISoundType.gun) ? shooterDistance / 50f : shooterDistance / 20f;

                float num2 = EFTMath.Random(-dispersion, dispersion);
                float num3 = EFTMath.Random(-dispersion, dispersion);

                Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);

                try
                {
                    BotOwner.BotsGroup.AddPointToSearch(vector, power, BotOwner);
                }
                catch (System.Exception) { }

                if (shooterDistance < BotOwner.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
                {
                    BotOwner.LookData.ResetUpdateTime();
                }

                if (person != null && isGunSound)
                {
                    Vector3 to = pos + person.LookDirection;
                    bool soundclose = IsSoundClose(out var firedAtMe, pos, to, 10f);

                    if (soundclose && firedAtMe)
                    {
                        try
                        {
                            SAIN.UnderFireFromPosition = vector;
                            BotOwner.Memory.SetUnderFire();
                        }
                        catch (System.Exception) { }


                        if (shooterDistance > 50f)
                        {
                            SAIN.Talk.Say(EPhraseTrigger.SniperPhrase);
                        }
                    }
                }

                if (!BotOwner.Memory.GoalTarget.HavePlaceTarget() && BotOwner.Memory.GoalEnemy == null)
                {
                    BotOwner.BotsGroup.CalcGoalForBot(BotOwner);
                    return;
                }
            }
            else if (person != null && isGunSound && bulletfelt)
            {
                Vector3 to = pos + person.LookDirection;
                bool soundclose = IsSoundClose(out var firedAtMe, pos, to, 10f);

                if (firedAtMe && soundclose)
                {
                    var estimate = GetEstimatedPoint(pos);

                    SAIN.UnderFireFromPosition = estimate;

                    try
                    {
                        BotOwner.BotsGroup.AddPointToSearch(estimate, 50f, BotOwner);
                    }
                    catch (System.Exception) { }
                }
            }
        }

        public LastHeardSound LastHeardSound { get; private set; }

        private Vector3 GetEstimatedPoint(Vector3 source)
        {
            Vector3 randomPoint = Random.onUnitSphere * (Vector3.Distance(source, BotOwner.Transform.position) / 5f);
            randomPoint.y = Mathf.Clamp(randomPoint.y, -5f, 5f);
            return source + randomPoint;
        }

        private bool IsSoundClose(out bool firedAtMe, Vector3 from, Vector3 to, float maxDist)
        {
            var projectionPoint = GetProjectionPoint(BotOwner.Position + Vector3.up, from, to);

            bool closeSound = (projectionPoint - BotOwner.Position).magnitude < maxDist;

            var direction = projectionPoint - from;

            firedAtMe = !Physics.Raycast(from, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);

            if (DebugSolarintSound.Value && closeSound && firedAtMe)
            {
                DebugGizmos.SingleObjects.Sphere(projectionPoint, 0.1f, Color.red, true, 2f);
                DebugGizmos.SingleObjects.Line(from, projectionPoint, Color.red, 0.025f, true, 2f);
            }

            return closeSound;
        }

        public static Vector3 GetProjectionPoint(Vector3 p, Vector3 p1, Vector3 p2)
        {
            //CalculateRecoil the difference between the z-coordinates of points p1 and p2
            float num = p1.z - p2.z;

            //If the difference is 0, return a vector with the x-coordinate of point p and the y and z-coordinates of point p1
            if (num == 0f)
            {
                return new Vector3(p.x, p1.y, p1.z);
            }

            //CalculateRecoil the difference between the x-coordinates of points p1 and p2
            float num2 = p2.x - p1.x;

            //If the difference is 0, return a vector with the x-coordinate of point p1 and the y and z-coordinates of point p
            if (num2 == 0f)
            {
                return new Vector3(p1.x, p1.y, p.z);
            }

            //CalculateRecoil the values of num3, num4, and num5
            float num3 = p1.x * p2.z - p2.x * p1.z;
            float num4 = num2 * p.x - num * p.z;
            float num5 = -(num2 * num3 + num * num4) / (num2 * num2 + num * num);

            //Return a vector with the calculated x-coordinate, the y-coordinate of point p1, and the calculated z-coordinate
            return new Vector3(-(num3 + num2 * num5) / num, p1.y, num5);
        }

        private bool IsSoundClose(float d)
        {
            //Set the close hearing and far hearing variables
            float closehearing = 10f;
            float farhearing = 50f;

            //Check if the Distance is less than or equal to the close hearing
            if (d <= closehearing)
            {
                return true;
            }

            if (d > farhearing)
            {
                return false;
            }

            float num = farhearing - closehearing;

            //CalculateRecoil the difference between the Distance and close hearing
            float num2 = d - closehearing;

            //CalculateRecoil the ratio of the difference between the Distance and close hearing to the difference between the far hearing and close hearing
            float num3 = 1f - num2 / num;

            return EFTMath.Random(0f, 1f) < num3;
        }

        public bool DoIHearSound(IAIDetails enemy, Vector3 position, float power, AISoundType type, out float distance, bool withOcclusionCheck)
        {
            distance = (BotOwner.Transform.position - position).magnitude;

            // Is sound within hearing Distance at all?
            if (distance < power)
            {
                if (!withOcclusionCheck)
                {
                    return distance < power;
                }
                // if so, is sound blocked by obstacles?
                if (OcclusionCheck(enemy, position, power, distance, type, out float occludedpower))
                {
                    return distance < occludedpower;
                }
            }

            // Sound not heard
            distance = 0f;
            return false;
        }

        private bool OcclusionCheck(IAIDetails player, Vector3 position, float power, float distance, AISoundType type, out float occlusion)
        {
            // Raise up the vector3's to match head level
            Vector3 botheadpos = BotOwner.MyHead.position;
            //botheadpos.y += 1.3f;
            if (type == AISoundType.step)
            {
                position.y += 0.1f;
            }

            // Check if the source is the player
            bool isrealplayer = player.IsYourPlayer;

            // Checks if something is within line of sight
            if (Physics.Raycast(botheadpos, (botheadpos - position).normalized, power, LayerMaskClass.HighPolyWithTerrainNoGrassMask))
            {
                // If the sound source is the player, raycast and find number of collisions
                if (isrealplayer)
                {
                    // Check if the sound originates from an environment other than the BotOwner's
                    float environmentmodifier = EnvironmentCheck(player);

                    // Raycast check
                    float finalmodifier = RaycastCheck(botheadpos, position, environmentmodifier);

                    // Reduce occlusion for unsuppressed guns
                    if (type == AISoundType.gun) finalmodifier = Mathf.Sqrt(finalmodifier);

                    // Apply Modifier
                    occlusion = power * finalmodifier;

                    // Debug
                    if (DebugOcclusion.Value) Logger.LogDebug($"Raycast Check. Heard?: {distance < occlusion}: For [{BotOwner.name}]: from [{(player as Player).name}] new reduced power: [{occlusion}] because modifier = [{finalmodifier}]");

                    return distance < occlusion;
                }
                else
                {
                    // Only check environment for bots vs bots
                    occlusion = power * EnvironmentCheck(player);
                    return distance < occlusion;
                }
            }
            else
            {
                // Direct line of sight, no occlusion
                occlusion = distance;
                return false;
            }
        }

        private float EnvironmentCheck(IAIDetails enemy)
        {
            int botlocation = BotOwner.AIData.EnvironmentId;
            int enemylocation = enemy.AIData.EnvironmentId;
            return botlocation == enemylocation ? 1f : 0.66f;
        }

        public float RaycastCheck(Vector3 botpos, Vector3 enemypos, float environmentmodifier)
        {
            if (raycasttimer < Time.time)
            {
                raycasttimer = Time.time + 0.25f;

                occlusionmodifier = 1f;

                LayerMask mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;

                // Create a RaycastHit array and set it to the Physics.RaycastAll
                var direction = botpos - enemypos;
                RaycastHit[] hits = Physics.RaycastAll(enemypos, direction, direction.magnitude, mask);

                int hitCount = 0;

                // Loop through each hit in the hits array
                for (int i = 0; i < hits.Length; i++)
                {
                    // Check if the hitCount is 0
                    if (hitCount == 0)
                    {
                        // If the hitCount is 0, set the occlusionmodifier to 0.8f multiplied by the environmentmodifier
                        occlusionmodifier *= 0.75f * environmentmodifier;
                    }
                    else
                    {
                        // If the hitCount is not 0, set the occlusionmodifier to 0.95f multiplied by the environmentmodifier
                        occlusionmodifier *= 0.9f * environmentmodifier;
                    }

                    // Increment the hitCount
                    hitCount++;
                }
            }

            return occlusionmodifier;
        }

        private ManualLogSource Logger;

        private float occlusionmodifier = 1f;

        private float raycasttimer = 0f;

        public delegate void GDelegate4(Vector3 vector, float bulletDistance, AISoundType type);

        public event GDelegate4 OnEnemySounHearded;
    }
}