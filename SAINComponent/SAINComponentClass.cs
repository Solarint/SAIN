using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent.BaseClasses;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.Debug;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Info;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes.Talk;
using System;
using UnityEngine;

namespace SAIN.SAINComponent
{
    public class SAINComponentClass : MonoBehaviour, IBotComponent
    {
        public static bool TryAddSAINToBot(BotOwner botOwner, out SAINComponentClass component)
        {
            Player player = EFTInfo.GetPlayer(botOwner?.ProfileId);
            GameObject gameObject = botOwner?.gameObject;

            if (gameObject != null && player != null)
            {
                // If Somehow this bot already has SAIN attached, destroy it.
                if (gameObject.TryGetComponent(out component))
                {
                    component.Dispose();
                }

                // Create a new Component
                component = gameObject.AddComponent<SAINComponentClass>();

                // Check is component is successfully initialized
                if (component?.Init(new SAINPersonClass(player)) == true)
                {
                    return true;
                }
            }
            component = null;
            return false;
        }

        public SAINPersonClass Person { get; private set; }

        public bool Init(SAINPersonClass person)
        {
            Person = person;

            try
            {
                NoBushESP = this.GetOrAddComponent<SAINNoBushESP>();
                NoBushESP.Init(person.BotOwner, this);
                FlashLight = person.Player?.gameObject?.AddComponent<SAINFlashLightComponent>();

                // Must be first, other classes use it
                Squad = new SAINSquadClass(this);
                Equipment = new SAINBotEquipmentClass(this);
                Info = new SAINBotInfoClass(this);
                Memory = new SAINMemoryClass(this);
                BotStuck = new SAINBotUnstuckClass(this);
                Hearing = new SAINHearingSensorClass(this);
                Talk = new SAINBotTalkClass(this);
                Decision = new SAINDecisionClass(this);
                Cover = new SAINCoverClass(this);
                SelfActions = new SAINSelfActionClass(this);
                Steering = new SAINSteeringClass(this);
                Grenade = new SAINBotGrenadeClass(this);
                Mover = new SAINMoverClass(this);
                EnemyController = new SAINEnemyController(this);
                Sounds = new SAINSoundsController(this);
                FriendlyFireClass = new SAINFriendlyFireClass(this);
                Vision = new SAINVisionClass(this);
            }
            catch (Exception ex)
            {
                Logger.LogError("Init SAIN ERROR, Disposing.");
                Logger.LogError(ex);
                Dispose();
                return false;
            }

            Memory.Init();
            EnemyController.Init();
            FriendlyFireClass.Init();
            Sounds.Init();
            Vision.Init();
            Equipment.Init();
            Mover.Init();
            BotStuck.Init();
            Hearing.Init();
            Talk.Init();
            Decision.Init();
            Cover.Init();
            Info.Init();
            Squad.Init();
            SelfActions.Init();
            Grenade.Init();
            Steering.Init();

            return true;
        }

        private void Update()
        {
            if (IsDead || Singleton<GameWorld>.Instance == null)
            {
                Dispose();
                return;
            }

            if (GameIsEnding)
            {
                return;
            }

            if (BotActive)
            {
                if (LayersActive)
                {
                    BotOwner.PatrollingData?.Pause();
                }
                else if (Enemy == null)
                {
                    BotOwner.PatrollingData?.Unpause();
                }

                Person.Update();

                Memory.Update();
                EnemyController.Update();
                FriendlyFireClass.Update();
                Sounds.Update();
                Vision.Update();
                Equipment.Update();
                Mover.Update();
                BotStuck.Update();
                Hearing.Update();
                Talk.Update();
                Decision.Update();
                Cover.Update();
                Info.Update();
                Squad.Update();
                SelfActions.Update();
                Grenade.Update();
                Steering.Update();
                BotOwner.DoorOpener.Update();

                if (Enemy == null && BotOwner.BotLight?.IsEnable == false)
                {
                    BotOwner.BotLight?.TurnOn();
                }
            }
        }

        public float DistanceToAimTarget
        {
            get
            {
                if (Enemy != null)
                {
                    return Enemy.RealDistance;
                }
                else if (BotOwner.Memory.GoalEnemy != null)
                {
                    return BotOwner.Memory.GoalEnemy.Distance;
                }
                return 10f;
            }
        }

        public void Shoot(bool checkFF = true)
        {
            if (checkFF && !FriendlyFireClass.ClearShot)
            {
                BotOwner.ShootData.EndShoot();
                return;
            }

            BotOwner.ShootData.Shoot();
        }

        private bool SAINActive => BigBrainHandler.IsBotUsingSAINLayer(BotOwner);

        public bool LayersActive
        {
            get
            {
                if (RecheckTimer < Time.time)
                {
                    if (SAINActive)
                    {
                        RecheckTimer = Time.time + 0.5f;
                        Active = true;
                    }
                    else
                    {
                        RecheckTimer = Time.time + 0.05f;
                        Active = false;
                    }
                }
                return Active;
            }
        }

        private bool Active;
        private float RecheckTimer = 0f;

        public void Dispose()
        {
            try
            {
                StopAllCoroutines();

                Memory.Dispose();
                EnemyController.Dispose();
                FriendlyFireClass.Dispose();
                Sounds.Dispose();
                Vision.Dispose();
                Equipment.Dispose();
                Mover.Dispose();
                BotStuck.Dispose();
                Hearing.Dispose();
                Talk.Dispose();
                Decision.Dispose();
                Cover.Dispose();
                Info.Dispose();
                Squad.Dispose();
                SelfActions.Dispose();
                Grenade.Dispose();
                Steering.Dispose();
                Enemy?.Dispose();

                Destroy(this);
            }
            catch { }
        }

        public Vector3? CurrentTargetPosition
        {
            get
            {
                if (HasEnemy)
                {
                    return Enemy.EnemyPosition;
                }
                var Target = BotOwner.Memory.GoalTarget;
                if (Target != null && Target?.Position != null)
                {
                    if ((Target.Position.Value - BotOwner.Position).sqrMagnitude < 2f)
                    {
                        Target.Clear();
                    }
                    else
                    {
                        return Target.Position;
                    }
                }
                var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 200f, true);
                if (sound != null && !sound.IsCome)
                {
                    if ((sound.Position - Position).sqrMagnitude < 2f)
                    {
                        sound.IsCome = true;
                    }
                    else
                    {
                        return Target.Position;
                    }
                }
                if (Time.time - BotOwner.Memory.LastTimeHit < 3f)
                {
                    return BotOwner.Memory.LastHitPos;
                }
                return null;
            }
        }

        public SAINEnemyClass Enemy => HasEnemy ? EnemyController.ActiveEnemy : null;
        public SAINPersonTransformClass Transform => Person.Transform;
        public SAINMemoryClass Memory { get; private set; }
        public SAINEnemyController EnemyController { get; private set; }
        public SAINNoBushESP NoBushESP { get; private set; }
        public SAINFriendlyFireClass FriendlyFireClass { get; private set; }
        public SAINSoundsController Sounds { get; private set; }
        public SAINVisionClass Vision { get; private set; }
        public SAINBotEquipmentClass Equipment { get; private set; }
        public SAINMoverClass Mover { get; private set; }
        public SAINBotUnstuckClass BotStuck { get; private set; }
        public SAINFlashLightComponent FlashLight { get; private set; }
        public SAINHearingSensorClass Hearing { get; private set; }
        public SAINBotTalkClass Talk { get; private set; }
        public SAINDecisionClass Decision { get; private set; }
        public SAINCoverClass Cover { get; private set; }
        public SAINBotInfoClass Info { get; private set; }
        public SAINSquadClass Squad { get; private set; }
        public SAINSelfActionClass SelfActions { get; private set; }
        public SAINBotGrenadeClass Grenade { get; private set; }
        public SAINSteeringClass Steering { get; private set; }

        public bool IsDead => BotOwner == null || BotOwner.IsDead == true || Player == null || Player.HealthController.IsAlive == false;
        public bool BotActive => IsDead == false && BotOwner.enabled && Player.enabled && BotOwner.BotState == EBotState.Active;
        public bool GameIsEnding => Singleton<IBotGame>.Instance == null || Singleton<IBotGame>.Instance.Status == GameStatus.Stopping;

        public Vector3 Position => Person.Position;
        public BotOwner BotOwner => Person.BotOwner;
        public string ProfileId => Person.ProfileId;
        public Player Player => Person.Player;
        public bool HasEnemy => EnemyController.HasEnemy;
    }
}