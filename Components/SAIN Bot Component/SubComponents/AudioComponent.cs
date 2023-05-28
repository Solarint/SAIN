using BepInEx.Logging;
using SAIN.Helpers;
using EFT;
using UnityEngine;
using static SAIN.UserSettings.SoundConfig;

namespace SAIN.Components
{
    public class AudioComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        public void HearSound(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (SAIN == null || !SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            EnemySoundHeard(player, position, power, type);
        }

        private void EnemySoundHeard(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (BotOwner.IsDead)
            {
                return;
            }

            if (player != null)
            {
                if (BotOwner.GetPlayer == player.GetPlayer)
                {
                    return;
                }

                if (CheckSoundHeardAfterModifiers(player, position, power, type, out float distance))
                {
                    bool gunsound = type == AISoundType.gun || type == AISoundType.silencedGun;
                    bool flag = false;

                    if (!BotOwner.BotsGroup.IsEnemy(player))
                    {
                        if (gunsound && BotOwner.BotsGroup.Neutrals.ContainsKey(player))
                        {
                            flag = true;
                        }

                        if (!flag && BotOwner.BotsGroup.Enemies.ContainsKey(player))
                        {
                            return;
                        }
                    }
                    if (flag)
                    {
                        BotOwner.BotsGroup.LastSoundsController.AddNeutralSound(player, position);
                        return;
                    }

                    if (gunsound || IsSoundClose(distance))
                    {
                        if (distance < 5f)
                        {
                            BotOwner.Memory.Spotted(false, null, null);
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
                if (BotOwner.GetPlayer.HealthStatus == ETagStatus.Dying)
                {
                    range *= 0.66f;
                }
                if (BotOwner.Mover.Sprinting)
                {
                    range *= 0.66f;
                }
                else if (BotOwner.Mover.IsMoving && BotOwner.Mover.DestMoveSpeed > 0.8f)
                {
                    range *= 0.85f;
                }

                return DoIHearSound(player, position, range, type, out distance, false);
            }

            return false;
        }

        private void ReactToSound(IAIDetails person, Vector3 pos, float power, bool wasHeard, AISoundType type)
        {
            if (BotOwner.IsDead)
            {
                return;
            }

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
                SAIN.LastSoundHeardPosition = pos;

                float dispersion = (type == AISoundType.gun) ? shooterDistance / 50f : shooterDistance / 20f;

                float num2 = EFT_Math.Random(-dispersion, dispersion);
                float num3 = EFT_Math.Random(-dispersion, dispersion);

                Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);

                try
                {
                    BotOwner.BotsGroup.AddPointToSearch(vector, power, BotOwner);
                }
                catch (System.Exception)
                {

                }

                if (shooterDistance < BotOwner.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
                {
                    //BotOwner.LookData.ResetUpdateTime();
                }

                if (person != null && isGunSound)
                {
                    Vector3 to = pos + person.LookDirection;
                    bool soundclose = IsSoundClose(out var firedAtMe, pos, to, 10f);

                    SAIN.UnderFireFromPosition = vector;

                    //BotOwner.Memory.SetPanicPoint(placeForCheck, soundclose && firedAtMe);

                    if (soundclose && firedAtMe)
                    {
                        try
                        {
                            BotOwner.Memory.SetUnderFire();
                        }
                        catch (System.Exception)
                        {

                        }


                        if (shooterDistance > 80f)
                        {
                            SAIN.Talk.Talk.Say(EPhraseTrigger.SniperPhrase);
                        }
                        else
                        {
                            SAIN.Talk.Talk.Say(EPhraseTrigger.UnderFire);
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
                    catch (System.Exception)
                    {

                    }

                    //BotOwner.Memory.SetPanicPoint(placeForCheck2, false);
                }
            }
        }

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
            //Calculate the difference between the z-coordinates of points p1 and p2
            float num = p1.z - p2.z;

            //If the difference is 0, return a vector with the x-coordinate of point p and the y and z-coordinates of point p1
            if (num == 0f)
            {
                return new Vector3(p.x, p1.y, p1.z);
            }

            //Calculate the difference between the x-coordinates of points p1 and p2
            float num2 = p2.x - p1.x;

            //If the difference is 0, return a vector with the x-coordinate of point p1 and the y and z-coordinates of point p
            if (num2 == 0f)
            {
                return new Vector3(p1.x, p1.y, p.z);
            }

            //Calculate the values of num3, num4, and num5
            float num3 = p1.x * p2.z - p2.x * p1.z;
            float num4 = num2 * p.x - num * p.z;
            float num5 = -(num2 * num3 + num * num4) / (num2 * num2 + num * num);

            //Return a vector with the calculated x-coordinate, the y-coordinate of point p1, and the calculated z-coordinate
            return new Vector3(-(num3 + num2 * num5) / num, p1.y, num5);
        }

        private bool IsSoundClose(float d)
        {
            //Check if the BotOwner is dead
            if (BotOwner.IsDead)
            {
                return false;
            }

            //Set the close hearing and far hearing variables
            float closehearing = 10f;
            float farhearing = 60f;

            //Check if the distance is less than or equal to the close hearing
            if (d <= closehearing)
            {
                return true;
            }

            if (d > farhearing)
            {
                return false;
            }

            float num = farhearing - closehearing;

            //Calculate the difference between the distance and close hearing
            float num2 = d - closehearing;

            //Calculate the ratio of the difference between the distance and close hearing to the difference between the far hearing and close hearing
            float num3 = 1f - num2 / num;

            return EFT_Math.Random(0f, 1f) < num3;
        }

        public bool DoIHearSound(IAIDetails enemy, Vector3 position, float power, AISoundType type, out float distance, bool withOcclusionCheck)
        {
            if (BotOwner.IsDead)
            {
                distance = 0f;
                return false;
            }

            distance = (BotOwner.Transform.position - position).magnitude;

            // Is sound within hearing distance at all?
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
            bool isrealplayer = player.GetPlayer.name == "PlayerSuperior(Clone)";

            // Checks if something is within line of sight
            if (Physics.Raycast(botheadpos, (botheadpos - position).normalized, power, LayerMaskClass.HighPolyWithTerrainNoGrassMask))
            {
                // If the sound source is the player, raycast and find number of collisions
                if (isrealplayer && RaycastOcclusion.Value)
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
                    if (DebugOcclusion.Value) Logger.LogDebug($"Raycast Check. Heard?: {distance < occlusion}: For [{BotOwner.name}]: from [{player.GetPlayer.name}] new reduced power: [{occlusion}] because modifier = [{finalmodifier}]");

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
            if (BotOwner.IsDead || enemy.GetPlayer == null)
            {
                return 1f;
            }

            int botlocation = BotOwner.AIData.EnvironmentId;
            int enemylocation = enemy.GetPlayer.AIData.EnvironmentId;

            return botlocation == enemylocation ? 1f : 0.66f;
        }

        public float RaycastCheck(Vector3 botpos, Vector3 enemypos, float environmentmodifier)
        {
            if (BotOwner.IsDead)
            {
                return 1f;
            }

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
                        occlusionmodifier *= 0.8f * environmentmodifier;
                    }
                    else
                    {
                        // If the hitCount is not 0, set the occlusionmodifier to 0.95f multiplied by the environmentmodifier
                        occlusionmodifier *= 0.95f * environmentmodifier;
                    }

                    // Increment the hitCount
                    hitCount++;
                }
            }

            return occlusionmodifier;
        }

        private SAINComponent SAIN;

        private ManualLogSource Logger;

        private BotOwner BotOwner => SAIN.BotOwner;

        private float occlusionmodifier = 1f;

        private float raycasttimer = 0f;

        public delegate void GDelegate4(Vector3 vector, float bulletDistance, AISoundType type);

        public event GDelegate4 OnEnemySounHearded;
    }
}