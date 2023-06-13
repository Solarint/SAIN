using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Drawing;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;

namespace SAIN.Classes
{
    public class EnemyTalk : SAINBot
    {
        private ManualLogSource Logger;

        public EnemyTalk(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            PlayerTalk = Singleton<GameWorld>.Instance.MainPlayer.GetComponent<PlayerTalkComponent>();
        }

        private void Init()
        {
            var type = SAIN.Info.BotPersonality;
            if (type == BotPersonality.Chad || type == BotPersonality.GigaChad)
            {
                TauntDist = 50f;
                TauntFreq = 8f;
            }
            else
            {
                TauntDist = 20f;
                TauntFreq = 20f;
            }

            TauntDist *= Random.Range(0.66f, 1.33f);
            TauntFreq *= Random.Range(0.66f, 1.33f);
            TauntTimer = Time.time + TauntFreq;
            ResponseDist = TauntDist * Random.Range(0.66f, 1.33f);
        }

        private readonly PlayerTalkComponent PlayerTalk;
        private float ResponseDist;

        private const float EnemyCheckFreq = 0.25f;
        private float TauntDist = 0f;
        private float TauntFreq = 0f;

        public void Update()
        {
            if (TauntFreq == 0f)
            {
                Init();
            }

            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (BegForLife())
                {
                    return;
                }

                var pers = SAIN.Info.BotPersonality;
                if (pers == BotPersonality.Coward || pers == BotPersonality.Timmy || pers == BotPersonality.Rat)
                {
                    return;
                }

                if (FakeDeath())
                {
                    return;
                }

                if (LastEnemyCheckTime < Time.time)
                {
                    LastEnemyCheckTime = Time.time + EnemyCheckFreq;

                    CheckForEnemyTalk();

                    if (TauntTimer < Time.time && BotTaunts.Value)
                    {
                        TauntTimer = Time.time + TauntFreq * Random.Range(0.5f, 1.5f);

                        TauntEnemy();
                    }

                    StartResponse();
                }
            }
        }

        private bool FakeDeath()
        {
            if (SAIN.HasEnemy && !SAIN.Squad.BotInGroup)
            {
                var personality = SAIN.Info.BotPersonality;
                if (personality == BotPersonality.GigaChad || personality == BotPersonality.Chad)
                {
                    if (FakeTimer < Time.time)
                    {
                        FakeTimer = Time.time + 10f;
                        var health = SAIN.HealthStatus;
                        if (health != ETagStatus.Healthy && health != ETagStatus.Injured)
                        {
                            float dist = (SAIN.Enemy.Person.Position - BotOwner.Position).magnitude;
                            if (dist < 30f)
                            {
                                bool random = Helpers.EFTMath.RandomBool(1f);
                                if (random)
                                {
                                    Talk.Say(EPhraseTrigger.OnDeath);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private float FakeTimer = 0f;

        private bool BegForLife()
        {
            if (BegTimer < Time.time && SAIN.HasEnemy && !SAIN.Squad.BotInGroup)
            {
                bool random = Helpers.EFTMath.RandomBool(25);
                float timeAdd = random ? 8f : 2f;
                BegTimer = Time.time + timeAdd;

                var personality = SAIN.Info.BotPersonality;
                if (personality == BotPersonality.Timmy || personality == BotPersonality.Coward)
                {
                    var health = SAIN.HealthStatus;
                    if (health != ETagStatus.Healthy)
                    {
                        float dist = (SAIN.Enemy.Person.Position - BotOwner.Position).magnitude;
                        if (dist < 30f)
                        {
                            if (random)
                            {
                                EPhraseTrigger trigger;

                                float randomValue = Random.Range(0f, 1f);

                                if (randomValue <= 0.25f)
                                {
                                    trigger = EPhraseTrigger.Stop;
                                }
                                else if (randomValue <= 0.5f)
                                {
                                    trigger = EPhraseTrigger.OnBeingHurtDissapoinment;
                                }
                                else if (randomValue <= 0.75f)
                                {
                                    trigger = EPhraseTrigger.NeedHelp;
                                }
                                else
                                {
                                    trigger = EPhraseTrigger.HoldFire;
                                }

                                Talk.Say(trigger);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private float BegTimer = 0f;

        private bool TauntEnemy()
        {
            bool tauntEnemy = false;

            var sainEnemy = SAIN.Enemy;
            var type = SAIN.Info.BotPersonality;

            float distanceToEnemy = Vector3.Distance(BotOwner.Memory.GoalEnemy.CurrPosition, BotPos);

            if (distanceToEnemy < TauntDist)
            {
                if (BotOwner.Memory.GoalEnemy.CanShoot && BotOwner.Memory.GoalEnemy.IsVisible)
                {
                    if (sainEnemy != null && sainEnemy.EnemyLookingAtMe)
                    {
                        tauntEnemy = true;
                    }
                    if (type == BotPersonality.Chad)
                    {
                        tauntEnemy = true;
                    }
                }
                if (type == BotPersonality.GigaChad)
                {
                    tauntEnemy = true;
                }
            }

            if (!tauntEnemy && BotOwner.AimingData != null)
            {
                var aim = BotOwner.AimingData;
                if (aim != null && aim.IsReady)
                {
                    if (aim.LastDist2Target < TauntDist)
                    {
                        tauntEnemy = true;
                    }
                }
            }

            if (tauntEnemy)
            {
                Talk.Say(EPhraseTrigger.OnFight, ETagStatus.Combat, true);
            }

            return tauntEnemy;
        }

        private void CheckForEnemyTalk()
        {
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (AddReponseToQueue() == false)
            {
                var player = goalEnemy.Person.GetPlayer;
                if (player.IsYourPlayer && Vector3.Distance(player.Transform.position, BotPos) < ResponseDist)
                {
                    if (PlayerTalk.PlayerTalked)
                    {
                        SetEnemyTalk(EPhraseTrigger.OnFight, ETagStatus.Combat, true);
                    }
                }
            }
        }

        private void StartResponse()
        {
            if (LastEnemyTalk != null)
            {
                float delay = LastEnemyTalk.TalkDelay;
                if (LastEnemyTalk.TalkTime + delay < Time.time)
                {
                    Talk.Say(LastEnemyTalk.Trigger, LastEnemyTalk.Status, true);
                    LastEnemyTalk = null;
                }
            }
        }

        private bool AddReponseToQueue()
        {
            // Get each of the bot goalEnemy Talk Components
            var players = Singleton<GameWorld>.Instance.RegisteredPlayers;
            foreach (var player in players)
            {
                // Check the component and if the bot recently tauntEnemy
                if (player != null && player.HealthController.IsAlive)
                {
                    if (player.IsAI && player.AIData.BotOwner.BotState != EBotState.Active)
                    {
                        continue;
                    }

                    // Check the distance between the bots to see if they can hear them
                    if (Vector3.Distance(player.Position, BotOwner.Position) < ResponseDist)
                    {
                        if (BotOwner.BotsGroup.Enemies.ContainsKey(player))
                        {
                            bool enemyTalked;
                            if (player.IsAI)
                            {
                                var recentTalk = player.AIData.BotOwner.GetComponent<SAINComponent>()?.Talk.RecentTalk;
                                enemyTalked = recentTalk != null && recentTalk.Mask != ETagStatus.Unaware;
                            }
                            else
                            {
                                var component = player.GetComponent<PlayerTalkComponent>();
                                enemyTalked = component?.PlayerTalked == true && component?.TagStatus != ETagStatus.Unaware;
                            }

                            if (enemyTalked)
                            {
                                SetEnemyTalk(EPhraseTrigger.OnFight, ETagStatus.Combat, true);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void SetEnemyTalk(EPhraseTrigger trigger, ETagStatus status, bool withGroupDelay)
        {
            if (LastEnemyTalk == null)
            {
                LastEnemyTalk = new EnemyTalkObject(trigger, status, withGroupDelay);
            }
        }

        private Vector3 BotPos => BotOwner.Transform.position;

        private float LastEnemyCheckTime = 0f;
        private EnemyTalkObject LastEnemyTalk;
        private float TauntTimer = 0f;

        private BotTalkClass Talk => SAIN.Talk;
    }
}