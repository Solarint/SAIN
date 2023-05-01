using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI.Ragfair;
using SAIN_Helpers;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static SAIN_Grenades.Configs.GrenadeConfigs;

namespace SAIN_Grenades.Components
{
    public class GrenadeComponent : MonoBehaviour
    {
        private BotOwner Bot;
        protected static ManualLogSource Logger { get; private set; }

        /// <summary>
        /// Sets up the GrenadeComponent by creating a LogSource, getting the Player component, and calculating the BotDifficultyModifier if the Player is an AI.
        /// </summary>
        private void Awake()
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GrenadeComponent));

            Bot = GetComponent<BotOwner>();
        }

        /// <summary>
        /// Checks if the player is an AI, alive, within a certain distance, and if the limiter has been reached before creating a new ThrownGrenade.
        /// </summary>
        /// <param name="grenade">The grenade being thrown.</param>
        /// <param name="position">The position of the grenade.</param>
        /// <param name="force">The force of the grenade.</param>
        /// <param name="mass">The mass of the grenade.</param>
        public void GrenadeThrown(Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            if (Bot.IsDead == false)
            {
                float grenadeDistance = (position - Bot.Transform.position).magnitude;

                if (grenadeDistance < 100f)
                {
                    new ThrownGrenade(Bot, grenade, position, force, mass);
                }
            }
        }

        /// <summary>
        /// Represents a thrown grenade in a game.
        /// </summary>
        public class ThrownGrenade
        {
            /// <summary>
            /// Creates a new GrenadeTracker and initializes it with the given parameters.
            /// </summary>
            /// <param name="bot">The BotOwner associated with the GrenadeTracker.</param>
            /// <param name="grenade">The Grenade associated with the GrenadeTracker.</param>
            /// <param name="position">The position of the GrenadeTracker.</param>
            /// <param name="force">The force of the GrenadeTracker.</param>
            /// <param name="mass">The mass of the GrenadeTracker.</param>
            /// <returns>
            /// A new GrenadeTracker with the given parameters.
            /// </returns>
            public ThrownGrenade(BotOwner bot, Grenade grenade, Vector3 position, Vector3 force, float mass)
            {
                Vector3 DangerPoint = FindDanger.Point(position, force, mass);

                GrenadeTracker ActiveTracker = bot.gameObject.AddComponent<GrenadeTracker>();

                ActiveTracker.Initialize(bot, grenade, DangerPoint);
            }

            /// <summary>
            /// This class is used to track the position of a grenade in a 3D environment.
            /// </summary>
            private class GrenadeTracker : MonoBehaviour
            {
                private BotOwner BotOwner_0;

                private Grenade Grenade_0;

                private Vector3 DangerPoint;

                private float BotDifficultyModifier;

                /// <summary>
                /// Initializes the GrenadeTracker with the given BotOwner, Grenade, and Vector3 danger. If the grenade is within 10f of the BotOwner, the ReactToGrenade coroutine is started, otherwise the VisualTracker coroutine is started.
                /// </summary>
                public void Initialize(BotOwner bot, Grenade grenade, Vector3 danger)
                {
                    BotDifficultyModifier = BotModifier.Calculate(bot);

                    BotOwner_0 = bot;

                    Grenade_0 = grenade;

                    DangerPoint = danger;

                    if (DidIHearGrenade(grenade.transform.position, bot.Transform.position, 8f))
                    {
                        StartCoroutine(ReactToGrenade());
                    }
                    else
                    {
                        StartCoroutine(VisualTracker());
                    }
                }

                /// <summary>
                /// Coroutine that checks if the grenade is visible and reacts to it if it is.
                /// </summary>
                private IEnumerator VisualTracker()
                {
                    while (true)
                    {
                        if (BotOwner_0.IsDead || Grenade_0 == null)
                        {
                            yield break;
                        }

                        if (IsGrenadeVisible(Grenade_0.transform.position, BotOwner_0.Transform.position))
                        {
                            StartCoroutine(ReactToGrenade());

                            yield break;
                        }

                        yield return new WaitForSeconds(0.1f);
                    }
                }

                /// <summary>
                /// Coroutine that makes the bot react to a grenade.
                /// </summary>
                private IEnumerator ReactToGrenade()
                {
                    float reactionTime = GetReactionTime();

                    if (DebugLogs.Value)
                        Logger.LogInfo($"{BotOwner_0.name} Has Seen Grenade! Reaction Time is [{reactionTime}] because their difficulty modifier is [{BotDifficultyModifier}]");

                    yield return new WaitForSeconds(reactionTime);

                    BotReaction();

                    if (DebugLogs.Value)
                        Logger.LogDebug($"{BotOwner_0.name} Is reacting to grenade at {Time.time}");

                    yield break;
                }

                /// <summary>
                /// Adds a grenade danger to the BotOwner_0's BewareGrenade and draws a debug line between the BotOwner_0's position and the grenade's position.
                /// </summary>
                private void BotReaction()
                {
                    if (Grenade_0 != null && !BotOwner_0.IsDead)
                    {
                        BotOwner_0.BewareGrenade.AddGrenadeDanger(DangerPoint, Grenade_0);

                        GrenadeDebug.DrawBasic(BotOwner_0.Transform.position, Grenade_0.transform.position, 5f);
                    }
                }

                /// <summary>
                /// Calculates the reaction time of the bot based on the base reaction time, difficulty modifier, and a random range.
                /// </summary>
                /// <returns>
                /// The calculated reaction time, clamped between the minimum and maximum reaction times.
                /// </returns>
                private float GetReactionTime()
                {
                    float reactionTime = BaseReactionTime.Value;
                    reactionTime *= BotDifficultyModifier;
                    reactionTime *= UnityEngine.Random.Range(0.75f, 1.25f);

                    float min = MinimumReactionTime.Value;
                    float max = MaximumReactionTime.Value;

                    return Mathf.Clamp(reactionTime, min, max);
                }

                /// <summary>
                /// Checks if a grenade is visible from a given player position.
                /// </summary>
                /// <param name="grenadePosition">The position of the grenade.</param>
                /// <param name="playerPosition">The position of the player.</param>
                /// <returns>True if the grenade is visible, false otherwise.</returns>
                private bool IsGrenadeVisible(Vector3 grenadePosition, Vector3 playerPosition)
                {
                    grenadePosition.y += 0.05f;

                    Vector3 direction = (playerPosition - grenadePosition).normalized;
                    float distance = (playerPosition - grenadePosition).magnitude;

                    if (!Physics.Raycast(grenadePosition, direction, distance, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        return true;
                    }

                    return false;
                }

                /// <summary>
                /// Checks if the player is within a certain distance of a grenade.
                /// </summary>
                /// <param name="grenadePosition">The position of the grenade.</param>
                /// <param name="playerPosition">The position of the player.</param>
                /// <param name="distance">The distance to check.</param>
                /// <returns>True if the player is within the distance, false otherwise.</returns>
                private bool DidIHearGrenade(Vector3 grenadePosition, Vector3 playerPosition, float distance)
                {
                    if ((grenadePosition - playerPosition).magnitude < distance)
                        return true;

                    return false;
                }
            }
        }

        /// <summary>
        /// Generates a modifier for bot reaction times.
        /// </summary>
        private class BotModifier
        {
            /// <summary>
            /// Calculates the difficulty modifier for a given player.
            /// </summary>
            /// <param name="player">The player to calculate the difficulty modifier for.</param>
            /// <returns>The difficulty modifier for the given player.</returns>
            public static float Calculate(BotOwner bot)
            {
                var settings = bot.Profile.Info.Settings;

                if (settings != null)
                {
                    return GetDifficultyMod(settings.Role, settings.BotDifficulty);
                }

                return 1f;
            }

            /// <summary>
            /// Calculates the difficulty modifier for a given WildSpawnType and BotDifficulty.
            /// </summary>
            /// <param name="bottype">The WildSpawnType of the bot.</param>
            /// <param name="difficulty">The BotDifficulty of the bot.</param>
            /// <returns>The difficulty modifier for the given WildSpawnType and BotDifficulty.</returns>
            private static float GetDifficultyMod(WildSpawnType bottype, BotDifficulty difficulty)
            {
                float modifier = 1f;
                if (IsBoss(bottype))
                {
                    modifier = 0.85f;
                }
                else if (IsFollower(bottype))
                {
                    modifier = 0.95f;
                }
                else
                {
                    switch (bottype)
                    {
                        case WildSpawnType.assault:
                            modifier *= 1.25f;
                            break;

                        case WildSpawnType.pmcBot:
                            modifier *= 1.1f;
                            break;

                        case WildSpawnType.cursedAssault:
                            modifier *= 1.35f;
                            break;

                        case WildSpawnType.exUsec:
                            modifier *= 1.1f;
                            break;

                        default:
                            modifier *= 0.75f;
                            break;
                    }
                }

                switch (difficulty)
                {
                    case BotDifficulty.easy:
                        modifier *= 1.25f;
                        break;

                    case BotDifficulty.normal:
                        modifier *= 1.0f;
                        break;

                    case BotDifficulty.hard:
                        modifier *= 0.85f;
                        break;

                    case BotDifficulty.impossible:
                        modifier *= 0.75f;
                        break;

                    default:
                        modifier *= 1f;
                        break;
                }

                return modifier;
            }

            /// <summary>
            /// Checks if the given WildSpawnType is a boss type.
            /// </summary>
            /// <param name="bottype">The WildSpawnType to check.</param>
            /// <returns>True if the given WildSpawnType is a boss type, false otherwise.</returns>
            private static bool IsBoss(WildSpawnType bottype)
            {
                WildSpawnType[] bossTypes = new WildSpawnType[0];
                foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
                { // loop over all enum values
                    if (type.ToString().StartsWith("boss"))
                    {
                        Array.Resize(ref bossTypes, bossTypes.Length + 1);
                        bossTypes[bossTypes.Length - 1] = type;
                    }
                }
                if (bossTypes.Contains(bottype))
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Checks if the given WildSpawnType is a follower type.
            /// </summary>
            /// <param name="bottype">The WildSpawnType to check.</param>
            /// <returns>True if the given WildSpawnType is a follower type, false otherwise.</returns>
            private static bool IsFollower(WildSpawnType bottype)
            {
                WildSpawnType[] followerTypes = new WildSpawnType[0];
                foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
                {
                    if (type.ToString().StartsWith("follower"))
                    {
                        Array.Resize(ref followerTypes, followerTypes.Length + 1);
                        followerTypes[followerTypes.Length - 1] = type;
                    }
                }
                if (followerTypes.Contains(bottype))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// This class is used to find Danger Points from a thrown grenade
        /// </summary>
        private class FindDanger
        {

            /// <summary>
            /// Finds the danger point given a position, force, and mass.
            /// </summary>
            /// <param name="position">The starting position.</param>
            /// <param name="force">The force applied.</param>
            /// <param name="mass">The mass of the object.</param>
            /// <returns>The danger point.</returns>
            public static Vector3 Point(Vector3 position, Vector3 force, float mass)
            {
                force /= mass;

                Vector3 vector = CalculateForce(position, force);

                Vector3 midPoint = (position + vector) / 2f;

                CheckThreePoints(position, midPoint, vector, out Vector3 result);

                GrenadeDebug.DrawDanger(result, position, force);

                return result;
            }

            /// <summary>
            /// Checks if three points are connected without any obstacles in between.
            /// </summary>
            /// <param name="from">The starting point.</param>
            /// <param name="midPoint">The middle point.</param>
            /// <param name="target">The target point.</param>
            /// <param name="hitPos">The hit position.</param>
            /// <returns>True if the three points are connected, false otherwise.</returns>
            private static bool CheckThreePoints(Vector3 from, Vector3 midPoint, Vector3 target, out Vector3 hitPos)
            {
                Vector3 direction = midPoint - from;
                if (Physics.Raycast(new Ray(from, direction), out RaycastHit raycastHit, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    hitPos = raycastHit.point;
                    return false;
                }

                Vector3 direction2 = midPoint - target;
                if (Physics.Raycast(new Ray(midPoint, direction2), out raycastHit, direction2.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    hitPos = raycastHit.point;
                    return false;
                }

                hitPos = target;
                return true;
            }

            /// <summary>
            /// Calculates a vector based on a given force vector and a starting vector.
            /// </summary>
            /// <param name="from">The starting vector.</param>
            /// <param name="force">The force vector.</param>
            /// <returns>
            /// The calculated vector.
            /// </returns>
            private static Vector3 CalculateForce(Vector3 from, Vector3 force)
            {
                Vector3 v = new Vector3(force.x, 0f, force.z);

                Vector2 vector = new Vector2(v.magnitude, force.y);

                float num = 2f * vector.x * vector.y / GClass560.Core.G;

                if (vector.y < 0f)
                {
                    num = -num;
                }

                return SAIN_Math.NormalizeFastSelf(v) * num + from;
            }
        }

        /// <summary>
        /// Class to debug grenade related issues.
        /// </summary>
        private class GrenadeDebug
        {
            /// <summary>
            /// Draws a basic debug line, sphere and line between two Vector3 points.
            /// </summary>
            /// <param name="botPos">The first Vector3 point.</param>
            /// <param name="grenadePos">The second Vector3 point.</param>
            /// <param name="lifetime">The lifetime of the debug draw.</param>
            public static void DrawBasic(Vector3 botPos, Vector3 grenadePos, float lifetime)
            {
                if (DebugDrawTools.Value)
                {
                    SAIN_Helpers.DebugDrawer.Sphere(botPos, 0.25f, Color.red, lifetime);

                    SAIN_Helpers.DebugDrawer.Sphere(grenadePos, 0.25f, Color.red, lifetime);

                    SAIN_Helpers.DebugDrawer.Line(botPos, grenadePos, 0.1f, Color.red, lifetime);
                }
            }

            /// <summary>
            /// Draws a danger indicator at the given position with lines from the given from and force positions.
            /// </summary>
            /// <param name="hitPos">The position to draw the danger indicator at.</param>
            /// <param name="from">The position to draw the first line from.</param>
            /// <param name="force">The position to draw the second line from.</param>
            public static void DrawDanger(Vector3 hitPos, Vector3 from, Vector3 force)
            {
                if (DebugDrawTools.Value)
                {
                    SAIN_Helpers.DebugDrawer.Sphere(hitPos, 0.25f, Color.white, 2f);

                    SAIN_Helpers.DebugDrawer.Line(hitPos, from, 0.1f, Color.white, 2f);

                    SAIN_Helpers.DebugDrawer.Line(from, force, 0.1f, Color.white, 2f);

                    SAIN_Helpers.DebugDrawer.Line(force, hitPos, 0.1f, Color.white, 2f);
                }
            }
        }
    }
}