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
        public GDelegate4 OnEnemySounHearded;
        private BotOwner botOwner_0;
        private void Awake()
        {
            botOwner_0 = GetComponent<BotOwner>();
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
                // Makes sure bots aren't reacting to their own sounds
                if (botOwner_0.GetPlayer == player.GetPlayer)
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

                bool wasHeard = DoIHearSound(player, position, power, type, out float distance);

                if (!wasHeard) return;

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

                    ReactToSound(player, position, power, wasHeard, type);
                    return;
                }

                if (IsSoundClose(distance))
                {
                    ReactToSound(player, position, power, wasHeard, type);
                    return;
                }
            }
            else
            {
                float distance = (botOwner_0.Transform.position - position).magnitude;
                ReactToSound(null, position, power, distance < power, type);
            }
        }
        private void ReactToSound(IAIDetails enemy, Vector3 pos, float power, bool wasHeard, AISoundType type)
        {
            if (botOwner_0.IsDead)
            {
                return;
            }

            bool hasenemy = enemy != null;

            if (hasenemy && enemy.AIData.IsAI && botOwner_0.BotsGroup.Contains(enemy.AIData.BotOwner))
            {
                return;
            }

            float bulletfeeldistance = 500f;
            Vector3 bulletPos = botOwner_0.Transform.position - pos;
            float bulletDistance = bulletPos.magnitude;
            bool gunsound = type == AISoundType.gun || type == AISoundType.silencedGun;
            bool bulletfelt = bulletDistance < bulletfeeldistance;
            float bulletclosedistance = 5f;

            if (wasHeard)
            {
                if (type == AISoundType.step && !botOwner_0.Memory.GoalEnemy.IsVisible)
                {
                    botOwner_0.BotTalk.Say(EPhraseTrigger.NoisePhrase, false, ETagStatus.Aware);
                }

                float num;

                if (type != AISoundType.step && type - AISoundType.silencedGun <= 1)
                {
                    num = bulletDistance / 100f;
                }
                else
                {
                    num = bulletDistance / 50f;
                }

                float num2 = GClass783.Random(-num, num);

                float num3 = GClass783.Random(-num, num);

                Vector3 vector = new Vector3(pos.x + num2, pos.y, pos.z + num3);

                var placeForCheck = botOwner_0.BotsGroup.AddPointToSearch(vector, power, botOwner_0, true);

                if (bulletDistance < botOwner_0.Settings.FileSettings.Hearing.RESET_TIMER_DIST)
                {
                    botOwner_0.LookData.ResetUpdateTime();
                }

                OnEnemySounHearded?.Invoke(vector, bulletDistance, type);

                if (hasenemy && bulletfelt && gunsound)
                {
                    if (bulletDistance >= 20f && !IsShotFiredatMe(bulletPos.magnitude, enemy.LookDirection, bulletPos))
                    {
                        try
                        {
                            botOwner_0.Memory.SetPanicPoint(placeForCheck, false);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        Vector3 to = pos + enemy.LookDirection;
                        bool soundclose = IsSoundClose(pos, to, 20f);

                        try
                        {
                            botOwner_0.Memory.SetPanicPoint(placeForCheck, soundclose);
                        }
                        catch
                        {
                        }

                        if (soundclose)
                        {
                            if (botOwner_0.Memory.IsInCover)
                            {
                                botOwner_0.Memory.BotCurrentCoverInfo.AddShootDirection();
                            }

                            try
                            {
                                botOwner_0.Memory.SetUnderFire();
                            }
                            catch
                            {
                            }

                            if (bulletDistance > 80f)
                            {
                                try
                                {
                                    botOwner_0.BotTalk.Say(EPhraseTrigger.SniperPhrase, false, null);
                                }
                                catch
                                {
                                }
                                return;
                            }
                        }
                    }
                }

                if (!botOwner_0.Memory.GoalTarget.HavePlaceTarget() && botOwner_0.Memory.GoalEnemy == null)
                {
                    try
                    {
                        botOwner_0.BotsGroup.CalcGoalForBot(botOwner_0);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            else if (gunsound && bulletfelt && hasenemy)
            {
                Vector3 to2 = pos + enemy.LookDirection;

                if (IsSoundClose(pos, to2, bulletclosedistance / 2f))
                {
                    if (botOwner_0.Memory.IsInCover)
                    {
                        try
                        {
                            botOwner_0.Memory.Spotted(false, null, null);
                        }
                        catch
                        {
                            return;
                        }
                    }
                    try
                    {
                        GClass270 placeForCheck2 = botOwner_0.BotsGroup.AddPointToSearch(pos, power, botOwner_0, true);
                        botOwner_0.Memory.SetPanicPoint(placeForCheck2, false);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }
        private bool IsSoundClose(Vector3 from, Vector3 to, float maxSDist)
        {
            return (GClass252.GetProjectionPoint(botOwner_0.Position + Vector3.up, from, to) - botOwner_0.Position).sqrMagnitude < maxSDist;
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

            return GClass783.Random(0f, 1f) < num3;
        }
        public bool DoIHearSound(IAIDetails enemy, Vector3 position, float power, AISoundType type, out float distance)
        {
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
                // if so, is sound blocked by obstacles?
                if (!OcclusionCheck(enemy, position, power, distance, type, out float occludedpower))
                {
                    // With obstacles taken into account, is the sound still audible?
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
                    float finalmodifier = RaycastCheck(botheadpos, position, environmentmodifier, power);

                    // Reduce occlusion for unsuppressed guns
                    if (type == AISoundType.gun) finalmodifier = Mathf.Sqrt(finalmodifier);

                    // Apply Modifier
                    occlusion = power * finalmodifier;

                    // Debug
                    if (DebugOcclusion.Value) Console.WriteLine($"SAIN Sound Raycast Check. For [{botOwner_0.name}]: from [{player.GetPlayer.name}] new reduced power: [{power}] because modifier = [{finalmodifier}]");

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
            if (botOwner_0.IsDead || enemy.GetPlayer == null)
            {
                return 1f;
            }

            int botlocation = botOwner_0.AIData.EnvironmentId;
            int enemylocation = enemy.GetPlayer.AIData.EnvironmentId;

            if (DebugOcclusion.Value && botlocation != enemylocation) Console.WriteLine($"SAIN EnviromentID: " +
                $"For [{botOwner_0.name}] = [{botlocation}] | " +
                $"Enemy [{enemy.GetPlayer.name}] = [{enemylocation}]");

            if (botlocation == enemylocation)
            {
                return 1f;
            }
            else
            {
                return 0.66f;
            }
        }
        public float RaycastCheck(Vector3 botpos, Vector3 enemypos, float environmentmodifier, float distance)
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
                        Console.WriteLine("Hit material: " + hitMaterial.name);
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
                    // Create a new GameObject for the line
                    GameObject lineObject = new GameObject();
                    Color[] colors = new Color[] { Color.red, Color.green, Color.blue };
                    // Loop through each hit and create a sphere at the hit point and draw a line between the starting point and each hit point
                    for (int i = 0; i < hits.Length; i++)
                    {
                        // Create a sphere at the hit point
                        Vector3 hitPoint = hits[i].point;
                        float sphereSize = 0.2f;
                        Color sphereColor = colors[i % colors.Length];
                        GameObject sphereObject = Draw.Sphere(hitPoint, sphereSize, sphereColor);

                        // Draw a line between the starting point and each hit point
                        float lineWidth = 0.05f;
                        Color lineColor = colors[i % colors.Length];
                        GameObject lineObjectSegment = Draw.Line(enemypos, hitPoint, lineWidth, lineColor);
                        lineObjectSegment.transform.parent = lineObject.transform;

                        // Set the parent of the sphere object to the line object
                        sphereObject.transform.parent = lineObject.transform;
                    }
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