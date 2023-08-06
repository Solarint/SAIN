using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using SAIN.Components;
using System.Threading.Tasks;
using SAIN.SAINComponent.Classes.Debug;
using SAIN.SAINComponent.Classes.Info;

namespace SAIN.SAINComponent
{
    public class SAINComponentHandler
    {
        public static SAINComponentClass AddComponent(BotOwner botOwner)
        {
            Player player = EFTInfo.GetPlayer(botOwner?.ProfileId);

            if (botOwner?.gameObject != null && player != null
                && AddSAIN(player, botOwner, out var component))
            {
                return component;
            }
            return null;
        }

        public static SAINComponentClass AddComponent(Player player)
        {
            BotOwner botOwner = player?.AIData?.BotOwner;

            if (player?.gameObject != null && botOwner != null 
                && AddSAIN(player, botOwner, out var component))
            {
                return component;
            }
            return null;
        }

        static bool AddSAIN(Player player, BotOwner botOwner, out SAINComponentClass component)
        {
            component = botOwner.GetOrAddComponent<SAINComponentClass>();
            return component?.Init(player, botOwner) == true;
        }
    }

    public class SAINComponentClass : MonoBehaviour, IBotComponent
    {
        public bool Init(Player player, BotOwner botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($"SAIN Component [{botOwner?.name}]");

            Player = player;
            BotOwner = botOwner;

            try
            {
                NoBushESP = this.GetOrAddComponent<NoBushESP>();
                NoBushESP.Init(BotOwner, this);
                FlashLight = player?.gameObject?.AddComponent<FlashLightComponent>();

                // Must be first, other classes use it
                Squad = new SquadClass(this);
                Equipment = new BotEquipmentClass(this);
                Info = new BotInfoClass(this);
                Memory = new MemoryClass(this);
                Transform = new TransformClass(this);
                BotStuck = new BotUnstuckClass(this);
                Hearing = new HearingSensorClass(this);
                Talk = new BotTalkClass(this);
                Decision = new DecisionClass(this);
                Cover = new CoverClass(this);
                SelfActions = new SelfActionClass(this);
                Steering = new SteeringClass(this);
                Grenade = new BotGrenadeClass(this);
                Mover = new MoverClass(this);
                EnemyController = new EnemyController(this);
                Sounds = new SoundsController(this);
                FriendlyFireClass = new FriendlyFireClass(this);
                Vision = new VisionClass(this);
            }
            catch (Exception ex)
            {
                Logger.LogError("Init SAIN ERROR, Disposing.");
                Logger.LogError(ex);
                Dispose();
                return false;
            }

            Transform.Init();
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

        public string ProfileId => BotOwner?.ProfileId;
        public bool NoBushESPActive => NoBushESP.NoBushESPActive;
        public SAINBotController BotController => SAINPlugin.BotController;

        public Player Player { get; private set; }

        private void Update()
        {
            if (this == null || IsDead || Singleton<GameWorld>.Instance == null || Singleton<IBotGame>.Instance == null)
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

                Transform.Update();
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

                Enemy?.Update();

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

        public void Shoot(bool noBush = true, bool checkFF = true)
        {
            if ((noBush && NoBushESPActive) || (checkFF && !FriendlyFireClass.ClearShot))
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

        public bool HasEnemy => EnemyController.HasEnemy;
        public EnemyClass Enemy => HasEnemy ? EnemyController.Enemy : null;

        public void Dispose()
        {
            try
            {
                StopAllCoroutines();

                Transform.Dispose();
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
                    return Enemy.CurrPosition;
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
                    if ((sound.Position - BotOwner.Position).sqrMagnitude < 2f)
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

        public TransformClass Transform { get; private set; }
        public MemoryClass Memory { get; private set; }
        public EnemyController EnemyController { get; private set; }
        public NoBushESP NoBushESP { get; private set; }
        public FriendlyFireClass FriendlyFireClass { get; private set; }
        public SoundsController Sounds { get; private set; }
        public VisionClass Vision { get; private set; }
        public BotEquipmentClass Equipment { get; private set; }
        public MoverClass Mover { get; private set; }
        public BotUnstuckClass BotStuck { get; private set; }
        public FlashLightComponent FlashLight { get; private set; }
        public HearingSensorClass Hearing { get; private set; }
        public BotTalkClass Talk { get; private set; }
        public DecisionClass Decision { get; private set; }
        public CoverClass Cover { get; private set; }
        public Classes.Info.BotInfoClass Info { get; private set; }
        public SquadClass Squad { get; private set; }
        public SelfActionClass SelfActions { get; private set; }
        public BotGrenadeClass Grenade { get; private set; }
        public SteeringClass Steering { get; private set; }

        public bool IsDead => BotOwner == null || BotOwner.IsDead == true || Player == null || Player.HealthController.IsAlive == false;
        public bool BotActive => IsDead == false && BotOwner.enabled && Player.enabled && BotOwner.BotState == EBotState.Active;
        public bool GameIsEnding => Singleton<IBotGame>.Instance == null || Singleton<IBotGame>.Instance.Status == GameStatus.Stopping;


        public BotOwner BotOwner { get; private set; }

        public ManualLogSource Logger { get; private set; }
    }
}