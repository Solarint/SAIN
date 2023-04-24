using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Audio.Helpers;
using System;
using UnityEngine;
using static SAIN.Audio.Configs.SoundConfig;

namespace SAIN.Audio.Components
{
    public class SolarintAudio : MonoBehaviour
    {
        private float occlusionmodifier = 1f;
        private float raycasttimer = 0f;

        public delegate void GDelegate4(Vector3 vector, float bulletDistance, AISoundType type);
        public event GDelegate4 OnEnemySounHearded;

        private BotOwner botOwner_0;
        private void Awake()
        {
            botOwner_0 = GetComponent<BotOwner>();
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SolarintAudio));
            }
        }
        private void OnEnable()
        {
            Singleton<GClass629>.Instance.OnSoundPlayed += HearSound;
        }
        private void OnDisable()
        {
            Singleton<GClass629>.Instance.OnSoundPlayed -= HearSound;
        }
        private void HearSound(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (botOwner_0.IsDead)
            {
                return;
            }

            // Check if the bot is alive before continuing
            if (botOwner_0?.GetPlayer?.HealthController?.IsAlive == false || botOwner_0?.GetPlayer == null || botOwner_0 == null)
            {
                return;
            }

            NewSound sound = new NewSound()
            {
                SolarintAudio_0 = this,
                player = player,
                position = position,
                power = power,
                type = type
            };

            //if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} sound initialized");

            EnemySoundHeard(sound.player, sound.position, sound.power, sound.type);
        }
        private void EnemySoundHeard(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (botOwner_0.IsDead)
            {
                return;
            }

            if (player != null)
            {
                if (botOwner_0.GetPlayer == player.GetPlayer)
                {
                    return;
                }

                bool wasHeard = DoIHearSound(player, position, power, type, out float distance, true);
                if (!wasHeard)
                {
                    return;
                }

                if (botOwner_0.GetPlayer.HealthStatus == ETagStatus.BadlyInjured)
                {
                    power *= 0.66f;
                }
                if (botOwner_0.Mover.Sprinting)
                {
                    power *= 0.66f;
                }
                else if (botOwner_0.Mover.IsMoving && botOwner_0.Mover.DestMoveSpeed > 0.8f)
                {
                    power *= 0.85f;
                }

                // Checks the sound again after applying modifiers
                wasHeard = DoIHearSound(player, position, power, type, out distance, false);
                if (!wasHeard)
                {
                    if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} heard a sound since they are in a state where its harder to hear, they didn't notice it");
                    return;
                }

                bool gunsound = type == AISoundType.gun || type == AISoundType.silencedGun;
                bool flag = false;
                try
                {
                    if (!botOwner_0.BotsGroup.IsEnemy(player))
                    {
                        if (gunsound && botOwner_0.BotsGroup.Neutrals.ContainsKey(player))
                        {
                            flag = true;
                        }

                        if (!flag && botOwner_0.BotsGroup.Enemies.ContainsKey(player))
                        {
                            return;
                        }
                    }
                    if (flag)
                    {
                        botOwner_0.BotsGroup.LastSoundsController.AddNeutralSound(player, position);
                        return;
                    }
                }
                catch
                {
                }

                if (gunsound)
                {
                    if (distance < 5f)
                    {
                        botOwner_0.Memory.Spotted(false, null, null);
                    }

                    ReactToSound(player, position, distance, wasHeard, type);
                    return;
                }

                if (IsSoundClose(distance))
                {
                    if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} heard a close sound");
                    ReactToSound(player, position, distance, wasHeard, type);
                    return;
                }
            }
            else
            {
                if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} heard a sound but enemy is null");
                float distance = (botOwner_0.Transform.position - position).magnitude;
                ReactToSound(null, position, distance, distance < power, type);
            }
        }
        private float HeardSoundTime = 0f;
        private void ReactToSound(IAIDetails enemy, Vector3 pos, float power, bool wasHeard, AISoundType type)
        {
            if (botOwner_0.IsDead)
            {
                return;
            }

            bool hasenemy = enemy != null;
            var goalEnemy = botOwner_0.Memory.GoalEnemy;

            if (hasenemy && enemy.AIData.IsAI && botOwner_0.BotsGroup.Contains(enemy.AIData.BotOwner))
            {
                return;
            }

            float bulletfeeldistance = 500f;
            Vector3 shotPosition = botOwner_0.Transform.position - pos;
            float shooterDistance = shotPosition.magnitude;
            bool isGunSound = type == AISoundType.gun || type == AISoundType.silencedGun;
            bool bulletfelt = shooterDistance < bulletfeeldistance;

            if (wasHeard)
            {
                try
                {
                    if (!isGunSound && HeardSoundTime < Time.time && (goalEnemy != null || botOwner_0.Memory?.GoalEnemy?.PersonalLastSeenTime < Time.time + 15f))
                    {
                        if (UnityEngine.Random.value < 0.1f)
                        {
                            HeardSoundTime = Time.time + 20f;
                            botOwner_0.BotTalk.Say(EPhraseTrigger.Rat, false, ETagStatus.Combat);
                        }
                        else if (UnityEngine.Random.value < 0.25f && goalEnemy == null)
                        {
                            HeardSoundTime = Time.time + 20f;
                            botOwner_0.BotTalk.Say(EPhraseTrigger.NoisePhrase, false, ETagStatus.Combat);
                        }
                        else if (UnityEngine.Random.value < 0.1f && goalEnemy == null)
                        {
                            HeardSoundTime = Time.time + 20f;
                            botOwner_0.BotTalk.Say(EPhraseTrigger.Silence, false, ETagStatus.Combat);
                        }
                    }
                }
                catch { }

                float dispersion;
                if (type != AISoundType.step && type - AISoundType.silencedGun <= 1)
                {
                    dispersion = shooterDistance / 100f;
                }
                else
                {
                    dispersion = shooterDistance / 50f;
                }
                float num2 = MathHelpers.Random(-dispersion, dispersion);
                float num3 = MathHelpers.Random(-dispersion, dispersion);

                Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);

                try
                {
                    var placeForChecktest = botOwner_0.BotsGroup.AddPointToSearch(vector, power, botOwner_0, true);
                    botOwner_0.BotTalk.TrySay(EPhraseTrigger.Attention);
                }
                catch { }

                if (DebugSolarintSound.Value)
                {
                    Logger.LogDebug($"{botOwner_0.name} heard a sound, dispersion: [{-dispersion}, {dispersion}]");
                    vector.y -= 0.35f;
                    DebugDrawer.Sphere(vector, 0.1f, Color.white, 2f);
                }

                if (shooterDistance < botOwner_0.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
                {
                    botOwner_0.LookData.ResetUpdateTime();
                }

                //OnEnemySounHearded?.Invoke(vector, bulletDistance, type);

                if (hasenemy && bulletfelt && isGunSound)
                {
                    if (shooterDistance >= 20f && !IsShotFiredatMe(shotPosition.magnitude, enemy.LookDirection, shotPosition))
                    {
                        if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} heard gunshot but it was far and not fired at them.");

                        try
                        {
                            var placeForCheck = botOwner_0.BotsGroup.AddPointToSearch(pos, power, botOwner_0, true);
                            botOwner_0.Memory.SetPanicPoint(placeForCheck, false);
                        }
                        catch { }
                    }
                    else
                    {
                        Vector3 to = pos + enemy.LookDirection;
                        bool soundclose = IsSoundClose(pos, to, 20f);

                        try
                        {
                            var placeForCheck = botOwner_0.BotsGroup.AddPointToSearch(pos, power, botOwner_0, true);
                            botOwner_0.Memory.SetPanicPoint(placeForCheck, soundclose);
                        }
                        catch { }

                        if (soundclose)
                        {

                            if (botOwner_0.Memory.IsInCover) botOwner_0.Memory.BotCurrentCoverInfo.AddShootDirection();

                            try
                            {
                                botOwner_0.Memory.SetUnderFire();

                                botOwner_0.BotTalk.Say(EPhraseTrigger.UnderFire, true, ETagStatus.Combat);

                                if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} is under fire!");
                            }
                            catch { }

                            if (shooterDistance > 80f) botOwner_0.BotTalk.Say(EPhraseTrigger.SniperPhrase, false, null);
                        }
                    }
                }

                if (!botOwner_0.Memory.GoalTarget.HavePlaceTarget() && botOwner_0.Memory.GoalEnemy == null)
                {
                    botOwner_0.BotsGroup.CalcGoalForBot(botOwner_0);

                    if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} is calculating goal");

                    return;
                }
            }
            else if (isGunSound && bulletfelt && hasenemy)
            {
                Vector3 to2 = pos + enemy.LookDirection;
                if (IsSoundClose(pos, to2, 20f))
                {
                    if (botOwner_0.Memory.IsInCover) botOwner_0.Memory.Spotted(false, null, null);

                    try
                    {
                        var placeForCheck2 = botOwner_0.BotsGroup.AddPointToSearch(pos, power, botOwner_0, true);
                        botOwner_0.Memory.SetPanicPoint(placeForCheck2, false);

                        if (DebugSolarintSound.Value) Logger.LogDebug($"{botOwner_0.name} heard a close gunshot!");
                    }
                    catch { }
                }
            }
        }
        private bool IsSoundClose(Vector3 from, Vector3 to, float maxSDist)
        {
            Vector3 projectionPoint = GetProjectionPoint(botOwner_0.Position + Vector3.up, from, to);

            if (DebugSolarintSound.Value)
            {
                DebugDrawer.Sphere(projectionPoint, 0.1f, Color.red, 5f);
            }

            return (projectionPoint - botOwner_0.Position).sqrMagnitude < maxSDist;
        }
        public static Vector3 GetProjectionPoint(Vector3 p, Vector3 p1, Vector3 p2)
        {
            float num = p1.z - p2.z;
            if (num == 0f)
            {
                return new Vector3(p.x, p1.y, p1.z);
            }
            float num2 = p2.x - p1.x;
            if (num2 == 0f)
            {
                return new Vector3(p1.x, p1.y, p.z);
            }
            float num3 = p1.x * p2.z - p2.x * p1.z;
            float num4 = num2 * p.x - num * p.z;
            float num5 = -(num2 * num3 + num * num4) / (num2 * num2 + num * num);
            return new Vector3(-(num3 + num2 * num5) / num, p1.y, num5);
        }
        private bool IsShotFiredatMe(float bDist, Vector3 dir, Vector3 bDir)
        {
            if (botOwner_0.IsDead)
            {
                return false;
            }

            float bulletdirectionangle = 10f;

            if (Vector3.Dot(dir, bDir) < 0f)
            {
                return false;
            }

            float num = Vector3.Angle(dir, bDir);

            if (DebugSolarintSound.Value)
            {
                GameObject lineObject = new GameObject();
                GameObject lineObjectSegment = DebugDrawer.Line(dir, bDir, 0.01f, Color.blue, 2f);
                lineObjectSegment.transform.parent = lineObject.transform;
            }

            return num < bulletdirectionangle && bDist * Mathf.Tan(num * 0.017453292f) < 23f;
        }
        private bool IsSoundClose(float d)
        {
            if (botOwner_0.IsDead)
            {
                return false;
            }

            float closehearing = 10f;

            float farhearing = 60f;

            if (d <= closehearing)
            {
                return true;
            }

            if (d > farhearing)
            {
                return false;
            }

            float num = farhearing - closehearing;

            float num2 = d - closehearing;

            float num3 = 1f - num2 / num;

            return MathHelpers.Random(0f, 1f) < num3;
        }
        /// <summary>
        /// Returns True if Sound is heard
        /// </summary>
        /// <param name="enemy">Enemy Information</param>
        /// <param name="position">Sound Position</param>
        /// <param name="power">Audible Range</param>
        /// <param name="type">AISoundType</param>
        /// <param name="distance">Distance to the enemy if sound was heard</param>
        /// <returns></returns>
        public bool DoIHearSound(IAIDetails enemy, Vector3 position, float power, AISoundType type, out float distance, bool withOcclusionCheck)
        {
            /// Returns true if sound is heard
            // he dead bro
            if (botOwner_0.IsDead)
            {
                distance = 0f;
                return false;
            }

            distance = (botOwner_0.Transform.position - position).magnitude;

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
        /// <summary>
        /// Returns True if Sound was heard through occlusion
        /// </summary>
        /// <param name="player">Enemy Information</param>
        /// <param name="position">Sound Source Location</param>
        /// <param name="power">Audible Range</param>
        /// <param name="distance">Distance between bot and source</param>
        /// <param name="type">AISoundType</param>
        /// <param name="occlusion">Occluded Sound Distance</param>
        /// <returns></returns>
        private bool OcclusionCheck(IAIDetails player, Vector3 position, float power, float distance, AISoundType type, out float occlusion)
        {
            // Raise up the vector3's to match head level
            Vector3 botheadpos = botOwner_0.MyHead.position;
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
                    // Check if the sound originates from an environment other than the bot's
                    float environmentmodifier = EnvironmentCheck(player);

                    // Raycast check
                    float finalmodifier = RaycastCheck(botheadpos, position, environmentmodifier);

                    // Reduce occlusion for unsuppressed guns
                    if (type == AISoundType.gun) finalmodifier = Mathf.Sqrt(finalmodifier);

                    // Apply Modifier
                    occlusion = power * finalmodifier;

                    // Debug
                    if (DebugOcclusion.Value) Logger.LogDebug($"Raycast Check. Heard?: {distance < occlusion}: For [{botOwner_0.name}]: from [{player.GetPlayer.name}] new reduced power: [{occlusion}] because modifier = [{finalmodifier}]");

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
        /// <summary>
        /// Checks if an environment differs between 2 entities. Returns 1 if they match
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        private float EnvironmentCheck(IAIDetails enemy)
        {
            if (botOwner_0.IsDead || enemy.GetPlayer == null)
            {
                return 1f;
            }

            int botlocation = botOwner_0.AIData.EnvironmentId;
            int enemylocation = enemy.GetPlayer.AIData.EnvironmentId;

            if (botlocation == enemylocation)
            {
                return 1f;
            }
            else
            {
                return 0.66f;
            }
        }
        /// <summary>
        /// Returns the occluded sound audible distance
        /// </summary>
        /// <param name="botpos"></param>
        /// <param name="enemypos"></param>
        /// <param name="environmentmodifier"></param>
        /// <returns></returns>
        public float RaycastCheck(Vector3 botpos, Vector3 enemypos, float environmentmodifier)
        {
            if (botOwner_0.IsDead)
            {
                return 1f;
            }

            if (raycasttimer < Time.time)
            {
                raycasttimer = Time.time + 0.25f;

                occlusionmodifier = 1f;

                bool firstHit = true;
                LayerMask mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;
                RaycastHit[] hits = Physics.RaycastAll(enemypos, (botpos - enemypos).normalized, Vector3.Distance(enemypos, botpos), mask);
                int hitCount = 0;
                for (int i = 0; i < hits.Length; i++)
                {
                    /*
                    Renderer objectRenderer = hits[i].collider.GetComponent<Renderer>();
                    if (objectRenderer != null)
                    {
                        Material hitMaterial = objectRenderer.material;
                        // Do something with the material
                        Logger.LogDebug("Hit material: " + hitMaterial.name);
                    }
                    */
                    if (hits[i].collider.gameObject != gameObject)
                    {
                        if (firstHit)
                        {
                            occlusionmodifier *= 0.8f * environmentmodifier;
                            firstHit = false;
                        }
                        else
                        {
                            occlusionmodifier *= 0.95f * environmentmodifier;
                        }

                        hitCount++;
                    }
                }

                if (DebugOcclusion.Value)
                {
                    // Loop through each hit and create a sphere at the hit point and draw a line between the starting point and each hit point
                    for (int i = 0; i < hits.Length; i++)
                    {
                        // Create a sphere at the hit point
                        Vector3 hitPoint = hits[i].point;
                        float sphereSize = 0.2f;
                        DebugDrawer.Sphere(hitPoint, sphereSize, Color.green, 2f);
                    }

                    // Draw a line between the starting point and each hit point
                    GameObject lineObject = new GameObject();
                    float lineWidth = 0.05f;
                    GameObject lineObjectSegment = DebugDrawer.Line(enemypos, botpos, lineWidth, Color.green, 2f);
                    lineObjectSegment.transform.parent = lineObject.transform;
                }

                return occlusionmodifier;
            }
            return occlusionmodifier;
        }
        private sealed class NewSound
        {
            internal void HearSound()
            {
                if (!SolarintAudio_0.botOwner_0.IsDead)
                {
                    SolarintAudio_0.EnemySoundHeard(player, position, power, type);
                }
            }

            public SolarintAudio SolarintAudio_0;

            public IAIDetails player;

            public Vector3 position;

            public float power;

            public AISoundType type;
        }
        protected static ManualLogSource Logger { get; private set; }
        private static class MathHelpers
        {
            public static float Random(float a, float b)
            {
                float num = (float)random_0.NextDouble();
                return a + (b - a) * num;
            }
            private static readonly System.Random random_0 = new System.Random();
        }
        private bool GroupAudio(BotGroupClass botBotsGroup, Player sourcePlayer)
        {
            for (int i = 0; i < botBotsGroup.MembersCount; i++)
            {
                if (botBotsGroup.Member(i).GetPlayer == sourcePlayer)
                {
                    return true;
                }
            }
            for (int j = 0; j < botBotsGroup.AllyCount; j++)
            {
                if (botBotsGroup.Ally(j).GetPlayer == sourcePlayer)
                {
                    return true;
                }
            }
            return false;
        }
    }
}