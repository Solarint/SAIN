using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN_Audio.Helpers;
using SAIN_Helpers;
using UnityEngine;
using static SAIN_Audio.Configs.SoundConfig;

namespace SAIN_Audio.Components
{
    public class SolarintAudio : MonoBehaviour
    {
        /// <summary>
        /// Sets the BotOwner component and creates a LogSource for SolarintAudio.
        /// </summary>
        private void Awake()
        {
            bot = GetComponent<BotOwner>();
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SolarintAudio));
            }
        }

        /// <summary>
        /// Subscribes to the OnSoundPlayed event of the GClass629 singleton instance. 
        /// </summary>
        private void OnEnable()
        {
            Singleton<GClass629>.Instance.OnSoundPlayed += HearSound;
        }

        /// <summary>
        /// Unsubscribes from the OnSoundPlayed event of the GClass629 singleton instance.
        /// </summary>
        private void OnDisable()
        {
            Singleton<GClass629>.Instance.OnSoundPlayed -= HearSound;
        }

        /// <summary>
        /// Checks if the bot is alive before continuing and creates a new sound object.
        /// </summary>
        private void HearSound(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (bot.IsDead)
            {
                return;
            }

            // Check if the bot is alive before continuing
            if (bot?.GetPlayer?.HealthController?.IsAlive == false || bot?.GetPlayer == null || bot == null)
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

        /// <summary>
        /// Handles the sound heard by the AI bot.
        /// </summary>
        /// <param name="player">The player that made the sound.</param>
        /// <param name="position">The position of the sound.</param>
        /// <param name="power">The power of the sound.</param>
        /// <param name="type">The type of sound.</param>
        private void EnemySoundHeard(IAIDetails player, Vector3 position, float power, AISoundType type)
        {
            if (bot.IsDead)
            {
                return;
            }

            if (player != null)
            {
                if (bot.GetPlayer == player.GetPlayer)
                {
                    return;
                }

                bool wasHeard = DoIHearSound(player, position, power, type, out float distance, true);
                if (!wasHeard)
                {
                    return;
                }

                if (bot.GetPlayer.HealthStatus == ETagStatus.BadlyInjured)
                {
                    power *= 0.66f;
                }
                if (bot.Mover.Sprinting)
                {
                    power *= 0.66f;
                }
                else if (bot.Mover.IsMoving && bot.Mover.DestMoveSpeed > 0.8f)
                {
                    power *= 0.85f;
                }

                // Checks the sound again after applying modifiers
                wasHeard = DoIHearSound(player, position, power, type, out distance, false);
                if (!wasHeard)
                {
                    //if (DebugSolarintSound.Value) Logger.LogDebug($"{bot.name} heard a sound since they are in a state where its harder to hear, they didn't notice it");
                    return;
                }

                bool gunsound = type == AISoundType.gun || type == AISoundType.silencedGun;
                bool flag = false;
                try
                {
                    if (!bot.BotsGroup.IsEnemy(player))
                    {
                        if (gunsound && bot.BotsGroup.Neutrals.ContainsKey(player))
                        {
                            flag = true;
                        }

                        if (!flag && bot.BotsGroup.Enemies.ContainsKey(player))
                        {
                            return;
                        }
                    }
                    if (flag)
                    {
                        bot.BotsGroup.LastSoundsController.AddNeutralSound(player, position);
                        return;
                    }
                }
                catch
                {
                    if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to Add Neutral Sound");
                }

                if (gunsound)
                {
                    if (distance < 5f)
                    {
                        bot.Memory.Spotted(false, null, null);
                    }

                    ReactToSound(player, position, distance, wasHeard, type);
                    return;
                }

                if (IsSoundClose(distance))
                {
                    //if (DebugSolarintSound.Value) Logger.LogDebug($"{bot.name} heard a close sound");
                    ReactToSound(player, position, distance, wasHeard, type);
                    return;
                }
            }
            else
            {
                if (DebugSolarintSound.Value) Logger.LogDebug($"{bot.name} heard a sound but enemy is null");
                float distance = (bot.Transform.position - position).magnitude;
                ReactToSound(null, position, distance, distance < power, type);
            }
        }

        /// <summary>
        /// React to sound by adding a point to search, setting panic point, and saying phrases.
        /// </summary>
        private void ReactToSound(IAIDetails enemy, Vector3 pos, float power, bool wasHeard, AISoundType type)
        {
            if (bot.IsDead)
            {
                return;
            }

            bool hasenemy = enemy != null;
            var goalEnemy = bot.Memory.GoalEnemy;

            if (hasenemy && enemy.AIData.IsAI && bot.BotsGroup.Contains(enemy.AIData.BotOwner))
            {
                return;
            }

            float bulletfeeldistance = 500f;
            Vector3 shotPosition = bot.Transform.position - pos;
            float shooterDistance = shotPosition.magnitude;
            bool isGunSound = type == AISoundType.gun || type == AISoundType.silencedGun;
            bool bulletfelt = shooterDistance < bulletfeeldistance;

            if (wasHeard)
            {
                if (!isGunSound)
                {
                    var groupTalk = bot.BotsGroup.GroupTalk;
                    if (HeardNoiseTime < Time.time && groupTalk.CanSay(bot, EPhraseTrigger.NoisePhrase))
                    {
                        HeardNoiseTime = Time.time + 60f;
                        bot.BotTalk.Say(EPhraseTrigger.NoisePhrase);

                        if (DebugSolarintSound.Value)
                            Logger.LogDebug($"{bot.Profile.Nickname} Said Phrase: [NoisePhrase]");
                    }
                    else if (SayRatTime < Time.time && groupTalk.CanSay(bot, EPhraseTrigger.Rat))
                    {
                        SayRatTime = Time.time + 120f;
                        bot.BotTalk.Say(EPhraseTrigger.Rat);

                        if (DebugSolarintSound.Value)
                            Logger.LogDebug($"{bot.Profile.Nickname} Said Phrase: [Rat]");
                    }
                    else
                    {
                        EPhraseTrigger[] options = new EPhraseTrigger[] {
                           EPhraseTrigger.OnBreath,
                           EPhraseTrigger.OnSix,
                           EPhraseTrigger.Covering,
                           EPhraseTrigger.CoverMe,
                           EPhraseTrigger.OnEnemyConversation,
                           EPhraseTrigger.OnFight,
                           EPhraseTrigger.OnMutter,
                           EPhraseTrigger.Regroup,
                           EPhraseTrigger.Silence,
                           EPhraseTrigger.Gogogo
                        };

                        System.Random rnd = new System.Random();
                        int index = rnd.Next(options.Length);

                        bot.BotTalk.Say(options[index]);

                        if (DebugSolarintSound.Value)
                            Logger.LogDebug($"{bot.Profile.Nickname} Said Phrase: [{options[index]}]");
                    }
                }

                float dispersion = (type == AISoundType.gun) ? shooterDistance / 100f : shooterDistance / 50f;

                float num2 = SAIN_Math.Random(-dispersion, dispersion);
                float num3 = SAIN_Math.Random(-dispersion, dispersion);

                Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);

                try
                {
                    var placeForChecktest = bot.BotsGroup.AddPointToSearch(vector, power, bot, true);
                    //bot.BotTalk.TrySay(EPhraseTrigger.Attention);
                }
                catch
                {
                    if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to AddPointToSearch 1");
                }

                if (DebugSolarintSound.Value)
                {
                    //Logger.LogDebug($"{bot.name} heard a sound, dispersion: [{-dispersion}, {dispersion}]");
                    //vector.y -= 0.35f;
                    //DebugDrawer.Sphere(vector, 0.1f, Color.white, 2f);
                }

                if (shooterDistance < bot.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
                {
                    bot.LookData.ResetUpdateTime();
                }

                //OnEnemySounHearded?.Invoke(vector, bulletDistance, type);

                if (hasenemy && bulletfelt && isGunSound)
                {
                    if (shooterDistance >= 20f && !IsShotFiredatMe(shotPosition.magnitude, enemy.LookDirection, shotPosition))
                    {
                        //if (DebugSolarintSound.Value) Logger.LogDebug($"{bot.name} heard gunshot but it was far and not fired at them.");

                        try
                        {
                            var placeForCheck = bot.BotsGroup.AddPointToSearch(pos, power, bot, true);
                            bot.Memory.SetPanicPoint(placeForCheck, false);
                        }
                        catch
                        {
                            if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to AddPointToSearch 2");
                        }
                    }
                    else
                    {
                        Vector3 to = pos + enemy.LookDirection;
                        bool soundclose = IsSoundClose(pos, to, 20f);

                        try
                        {
                            var placeForCheck = bot.BotsGroup.AddPointToSearch(pos, power, bot, true);
                            bot.Memory.SetPanicPoint(placeForCheck, soundclose);
                        }
                        catch
                        {
                            if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to AddPointToSearch 3");
                        }

                        if (soundclose)
                        {
                            if (bot.Memory.IsInCover) bot.Memory.BotCurrentCoverInfo.AddShootDirection();

                            try
                            {
                                bot.Memory.SetUnderFire();
                            }
                            catch
                            {
                                if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to Set Underfire");
                            }

                            if (shooterDistance > 80f) bot.BotTalk.Say(EPhraseTrigger.SniperPhrase, false, null);
                        }
                    }
                }

                if (!bot.Memory.GoalTarget.HavePlaceTarget() && bot?.Memory?.GoalEnemy == null)
                {
                    bot.BotsGroup.CalcGoalForBot(bot);
                    return;
                }
            }
            else if (isGunSound && bulletfelt && hasenemy)
            {
                Vector3 to2 = pos + enemy.LookDirection;
                if (IsSoundClose(pos, to2, 5f))
                {
                    if (bot.Memory.IsInCover) bot.Memory.Spotted(false, null, null);

                    try
                    {
                        var placeForCheck2 = bot.BotsGroup.AddPointToSearch(pos, power, bot, true);
                        bot.Memory.SetPanicPoint(placeForCheck2, false);

                        if (DebugSolarintSound.Value) Logger.LogDebug($"{bot.name} heard a close gunshot!");
                    }
                    catch
                    {
                        if (DebugSolarintSound.Value) Logger.LogWarning($"Error trying to AddPointToSearch 4");
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the sound is close to the bot.
        /// </summary>
        /// <param name="from">The start point of the sound.</param>
        /// <param name="to">The end point of the sound.</param>
        /// <param name="maxSDist">The maximum distance the sound can be from the bot.</param>
        /// <returns>True if the sound is close enough to the bot, false otherwise.</returns>
        private bool IsSoundClose(Vector3 from, Vector3 to, float maxDist)
        {
            Vector3 projectionPoint = GetProjectionPoint(bot.Position + Vector3.up, from, to);

            bool closeSound = (projectionPoint - bot.Position).magnitude < maxDist;

            if (DebugSolarintSound.Value && closeSound)
            {
                SAIN_Helpers.DebugDrawer.Sphere(projectionPoint, 0.1f, Color.red, 3f);
            }

            return closeSound;
        }

        /// <summary>
        /// Calculates the projection point of a given point onto a line defined by two other points.
        /// </summary>
        /// <param name="p">The point to project.</param>
        /// <param name="p1">The first point defining the line.</param>
        /// <param name="p2">The second point defining the line.</param>
        /// <returns>
        /// The projection point of the given point onto the line.
        /// </returns>
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

        /// <summary>
        /// Checks if a shot has been fired at the bot.
        /// </summary>
        /// <param name="bDist">Distance of the bullet.</param>
        /// <param name="dir">Direction of the bot.</param>
        /// <param name="bDir">Direction of the bullet.</param>
        /// <returns>True if a shot has been fired at the bot, false otherwise.</returns>
        /// 
        private bool IsShotFiredatMe(float bDist, Vector3 dir, Vector3 bDir)
        {
            if (bot.IsDead)
            {
                return false;
            }

            float bulletdirectionangle = 10f;
            float num = Vector3.Angle(dir, bDir);

            if (Vector3.Dot(dir, bDir) < 0f || num > bulletdirectionangle || bDist * Mathf.Tan(num * 0.017453292f) > 23f)
            {
                return false;
            }

            if (DebugSolarintSound.Value)
            {
                GameObject lineObject = new GameObject();
                GameObject lineObjectSegment = SAIN_Helpers.DebugDrawer.Line(dir, bDir, 0.01f, Color.blue, 2f);
                lineObjectSegment.transform.parent = lineObject.transform;
            }

            return true;
        }

        /// <summary>
        /// Checks if a sound is close to the bot based on the distance from the sound.
        /// </summary>
        /// <param name="d">The distance from the sound.</param>
        /// <returns>True if the sound is close to the bot, false otherwise.</returns>
        private bool IsSoundClose(float d)
        {
            //Check if the bot is dead
            if (bot.IsDead)
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

            return SAIN_Math.Random(0f, 1f) < num3;
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
            if (bot.IsDead)
            {
                distance = 0f;
                return false;
            }

            distance = (bot.Transform.position - position).magnitude;

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
            Vector3 botheadpos = bot.MyHead.position;
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
                    if (DebugOcclusion.Value) Logger.LogDebug($"Raycast Check. Heard?: {distance < occlusion}: For [{bot.name}]: from [{player.GetPlayer.name}] new reduced power: [{occlusion}] because modifier = [{finalmodifier}]");

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
            if (bot.IsDead || enemy.GetPlayer == null)
            {
                return 1f;
            }

            int botlocation = bot.AIData.EnvironmentId;
            int enemylocation = enemy.GetPlayer.AIData.EnvironmentId;

            return botlocation == enemylocation ? 1f : 0.66f;
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
            if (bot.IsDead)
            {
                return 1f;
            }

            if (raycasttimer < Time.time)
            {
                raycasttimer = Time.time + 0.25f;

                occlusionmodifier = 1f;

                LayerMask mask = LayerMaskClass.HighPolyWithTerrainNoGrassMask;

                // Create a RaycastHit array and set it to the Physics.RaycastAll
                RaycastHit[] hits = Physics.RaycastAll(enemypos, (botpos - enemypos).normalized, Vector3.Distance(enemypos, botpos), mask);

                int hitCount = 0;

                // Loop through each hit in the hits array
                for (int i = 0; i < hits.Length; i++)
                {
                    // Check if the hit collider is not the same as the gameObject
                    if (hits[i].collider.gameObject != gameObject)
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

                if (DebugOcclusion.Value)
                {
                    // Loop through each hit and create a sphere at the hit point and draw a line between the starting point and each hit point
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Vector3 hitPoint = hits[i].point;
                        float sphereSize = 0.2f;
                        SAIN_Helpers.DebugDrawer.Sphere(hitPoint, sphereSize, Color.green, 2f);
                    }

                    GameObject lineObject = new GameObject();
                    float lineWidth = 0.05f;
                    GameObject lineObjectSegment = SAIN_Helpers.DebugDrawer.Line(enemypos, botpos, lineWidth, Color.green, 2f);
                    lineObjectSegment.transform.parent = lineObject.transform;
                }

                return occlusionmodifier;
            }

            return occlusionmodifier;
        }

        private sealed class NewSound
        {
            /// <summary>
            /// Checks if the bot is dead and if not, calls the EnemySoundHeard method of the SolarintAudio_0 object with the given parameters. 
            /// </summary>
            internal void HearSound()
            {
                if (!SolarintAudio_0.bot.IsDead)
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

        private BotOwner bot;

        private float occlusionmodifier = 1f;

        private float raycasttimer = 0f;

        private float SayRatTime = 0f;

        private float HeardNoiseTime = 0f;

        public delegate void GDelegate4(Vector3 vector, float bulletDistance, AISoundType type);

        public event GDelegate4 OnEnemySounHearded;

        /// <summary>
        /// Not Used. Checks if the given player is part of the given BotGroupClass.
        /// </summary>
        /// <param name="botBotsGroup">The BotGroupClass to check.</param>
        /// <param name="sourcePlayer">The Player to check.</param>
        /// <returns>True if the Player is part of the BotGroupClass, false otherwise.</returns>
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