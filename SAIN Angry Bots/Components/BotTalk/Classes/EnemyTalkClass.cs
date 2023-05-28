using Comfort.Common;
using EFT;
using System.ComponentModel;
using UnityEngine;

namespace SAIN.Talk.Components
{
    public class EnemyTalk : SAINTalk
    {
        public EnemyTalk(BotOwner bot) : base(bot)
        {
            ResponseDist = 25f * Random.Range(0.5f, 1.5f);

            var type = SAIN.Info.BotPersonality;
            if (type == Classes.BotPersonality.Chad || type == Classes.BotPersonality.GigaChad)
            {
                TauntDist = 40f;
                TauntFreq = 6f;
            }
            else if (type == Classes.BotPersonality.Timmy || type == Classes.BotPersonality.Coward || type == Classes.BotPersonality.Rat)
            {
                TauntDist = 0f;
                TauntFreq = 999f;
            }
            else
            {
                TauntDist = 25f;
                TauntFreq = 15f;
            }

            TauntTimer = Time.time + TauntFreq;
        }

        private readonly float ResponseDist;

        private const float EnemyCheckFreq = 0.25f;
        private readonly float TauntDist = 20f;
        private readonly float TauntFreq = 10f;

        public void ManualUpdate()
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (LastEnemyCheckTime < Time.time)
                {
                    LastEnemyCheckTime = Time.time + EnemyCheckFreq;

                    CheckForEnemyTalk();

                    if (TauntTimer < Time.time)
                    {
                        TauntTimer = Time.time + TauntFreq * Random.Range(0.5f, 1.5f);

                        TauntEnemy();
                    }

                    StartResponse();
                }
            }
        }

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
                    if (sainEnemy.EnemyLookingAtMe)
                    {
                        tauntEnemy = true;
                    }
                    if (type == Classes.BotPersonality.Chad)
                    {
                        tauntEnemy = true;
                    }
                }
                if (type == Classes.BotPersonality.GigaChad)
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
                Talk.Say(EPhraseTrigger.MumblePhrase, ETagStatus.Combat, true);

                Logger.LogWarning($"Bot Taunted Enemy!: [{type}]");
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
                    if (PlayerComponent.PlayerTalked)
                    {
                        SetEnemyTalk(EPhraseTrigger.MumblePhrase, ETagStatus.Combat, true);
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
                    if (Vector3.Distance(player.Transform.position, BotOwner.Transform.position) < ResponseDist)
                    {
                        bool isEnemy = BotOwner.EnemiesController.IsEnemy(player);
                        bool enemyAdd = !isEnemy && BotOwner.Profile.Side != player.Profile.Side;

                        if (isEnemy || enemyAdd)
                        {
                            bool enemyTalked;
                            if (player.IsAI)
                            {
                                enemyTalked = player.AIData.BotOwner.GetComponent<BotTalkComponent>().ThisBotTalked;
                            }
                            else
                            {
                                enemyTalked = player.GetComponent<PlayerTalkComponent>().PlayerTalked;
                            }

                            if (enemyTalked)
                            {
                                SetEnemyTalk(EPhraseTrigger.MumblePhrase, ETagStatus.Combat, true);

                                if (enemyAdd)
                                {
                                    Logger.LogDebug($"Added player as enemy to bot because he heard them");

                                    BotOwner.BotsGroup.AddEnemy(player);
                                }

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

        private BotTalk Talk => BotTalkComponent.Talk;
    }
}