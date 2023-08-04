using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Classes.Sense;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINComponentHandler
    {
        public static SAINComponent AddComponent(BotOwner botOwner)
        {
            Player player = EFTInfo.GetPlayer(botOwner?.ProfileId);

            if (botOwner?.gameObject != null && player != null
                && AddSAIN(player, botOwner, out var component))
            {
                return component;
            }
            return null;
        }

        public static SAINComponent AddComponent(Player player)
        {
            BotOwner botOwner = player?.AIData?.BotOwner;

            if (player?.gameObject != null && botOwner != null 
                && AddSAIN(player, botOwner, out var component))
            {
                return component;
            }
            return null;
        }

        static bool AddSAIN(Player player, BotOwner botOwner, out SAINComponent component)
        {
            component = botOwner.GetOrAddComponent<SAINComponent>();
            return component?.Init(player, botOwner) == true;
        }
    }
    public class SAINComponent : MonoBehaviour
    {
        public bool Init(Player player, BotOwner botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource($"SAIN Component [{botOwner?.name}]");
            Player = player;
            BotOwner = botOwner;
            bool initSuccess = InitClassesAndComponents();
            if (initSuccess == false)
            {
                Logger.LogError("Init SAIN ERROR, disposing.");
                Dispose();
            }
            return initSuccess;
        }

        private T AddComponent<T>() where T : Component
        {
            return this.GetOrAddComponent<T>();
        }

        private bool InitClassesAndComponents()
        {
            // Must be first, other classes use it
            Squad = AddComponent<SquadClass>();
            if (Squad == null) return false;

            Equipment = new BotEquipmentClass(this);
            Info = new SAINBotInfo(this);

            BotStuck = AddComponent<SAINBotUnstuck>();
            if (BotStuck == null) return false;

            Hearing = AddComponent<HearingSensorClass>();
            if (Hearing == null) return false;

            Talk = AddComponent<BotTalkClass>();
            if (Talk == null) return false;

            Decision = AddComponent<DecisionClass>();
            if (Decision == null) return false;

            Cover = AddComponent<CoverClass>();
            if (Cover == null) return false;

            FlashLight = Player?.gameObject?.AddComponent<FlashLightComponent>();
            if (FlashLight == null) return false;

            SelfActions = AddComponent<SelfActionClass>();
            if (SelfActions == null) return false;

            Steering = AddComponent<SteeringClass>();
            if (Steering == null) return false;

            Grenade = AddComponent<BotGrenadeClass>();
            if (Grenade == null) return false;

            Mover = AddComponent<MoverClass>();
            if (Mover == null) return false;

            NoBushESP = AddComponent<NoBushESP>();
            if (NoBushESP == null) return false;
            NoBushESP.Init(BotOwner, this);

            EnemyController = AddComponent<EnemyController>();
            if (EnemyController == null) return false;

            Sounds = AddComponent<SoundsController>();
            if (Sounds == null) return false;

            FriendlyFireClass = new FriendlyFireClass(this);
            Vision = new VisionClass(this);
            return true;
        }

        public Collider BotZoneCollider => BotZone?.Collider;

        public AIPlaceInfo BotZone => BotOwner.AIData.PlaceInfo;

        public string ProfileId => BotOwner.ProfileId;

        public string SquadId => Squad.SquadID;

        public EnemyController EnemyController { get; private set; }

        public NoBushESP NoBushESP { get; private set; }

        public bool NoBushESPActive => NoBushESP.NoBushESPActive;

        public SAINBotController BotController => SAINPlugin.BotController;

        public List<Player> VisiblePlayers = new List<Player>();

        public List<string> VisiblePlayerIds = new List<string>();

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
                UpdatePatrolData();
                UpdateHealth();

                Vision.ManualUpdate();
                Info.ManualUpdate();
                FriendlyFireClass.ManualUpdate();
                BotOwner.DoorOpener.Update();

                Enemy?.ManualUpdate();

                if (Enemy == null && BotOwner.BotLight?.IsEnable == false)
                {
                    BotOwner.BotLight.TurnOn();
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

        private void UpdatePatrolData()
        {
            if (LayersActive)
            {
                BotOwner.PatrollingData?.Pause();
            }
            else
            {
                if (Enemy == null)
                {
                    BotOwner.PatrollingData?.Unpause();
                }
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

        private bool SAINActive => BigBrainSAIN.IsBotUsingSAINLayer(BotOwner);

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

        public Vector3? ExfilPosition { get; set; }
        public bool CannotExfil { get; set; }

        private void UpdateHealth()
        {
            if (UpdateHealthTimer < Time.time)
            {
                UpdateHealthTimer = Time.time + 0.33f;
                HealthStatus = BotOwner.GetPlayer.HealthStatus;
            }
        }

        public FriendlyFireStatus FriendlyFireStatus { get; private set; }
        private float UpdateHealthTimer = 0f;

        public bool HasEnemy => EnemyController.HasEnemy;
        public SAINEnemy Enemy => HasEnemy ? EnemyController.Enemy : null;

        public void Dispose()
        {
            try
            {
                StopAllCoroutines();

                Info?.Dispose();
                Cover?.Dispose();

                Destroy(Squad);
                Destroy(BotStuck);
                Destroy(Hearing);
                Destroy(Talk);
                Destroy(Decision);
                Destroy(FlashLight);
                Destroy(SelfActions);
                Destroy(Steering);
                Destroy(Grenade);
                Destroy(Mover);
                Destroy(NoBushESP);
                Destroy(EnemyController);
                Destroy(Sounds);

                Destroy(this);
            }
            catch { }
        }

        public SAINSoloDecision CurrentDecision => Decision.MainDecision;
        public Vector3 Position => BotOwner.Position;
        public Vector3 WeaponRoot => BotOwner.WeaponRoot.position;
        public Vector3 HeadPosition => BotOwner.LookSensor._headPoint;
        public Vector3 BodyPosition => BotOwner.MainParts[BodyPartType.body].Position;

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
                //if (Time.time - botOwner.Memory.LastTimeHit < 20f && !botOwner.Memory.IsPeace)
                //{
                //    return botOwner.Memory.LastHitPos;
                //}
                return null;
            }
        }

        public Vector3 UnderFireFromPosition { get; set; }

        public FriendlyFireClass FriendlyFireClass { get; private set; }
        public SoundsController Sounds { get; private set; }
        public VisionClass Vision { get; private set; }
        public BotEquipmentClass Equipment { get; private set; }
        public MoverClass Mover { get; private set; }
        public SAINBotUnstuck BotStuck { get; private set; }
        public FlashLightComponent FlashLight { get; private set; }
        public HearingSensorClass Hearing { get; private set; }
        public BotTalkClass Talk { get; private set; }
        public DecisionClass Decision { get; private set; }
        public CoverClass Cover { get; private set; }
        public SAINBotInfo Info { get; private set; }
        public SquadClass Squad { get; private set; }
        public SelfActionClass SelfActions { get; private set; }
        public BotGrenadeClass Grenade { get; private set; }
        public SteeringClass Steering { get; private set; }

        public bool IsDead => BotOwner == null || BotOwner.IsDead == true || BotOwner.GetPlayer == null || BotOwner.GetPlayer.HealthController.IsAlive == false;
        public bool BotActive => IsDead == false && BotOwner.enabled && BotOwner.GetPlayer.enabled && BotOwner.BotState == EBotState.Active;
        public bool GameIsEnding => Singleton<IBotGame>.Instance == null || Singleton<IBotGame>.Instance.Status == GameStatus.Stopping;

        public bool Healthy => HealthStatus == ETagStatus.Healthy;
        public bool Injured => HealthStatus == ETagStatus.Injured;
        public bool BadlyInjured => HealthStatus == ETagStatus.BadlyInjured;
        public bool Dying => HealthStatus == ETagStatus.Dying;

        public ETagStatus HealthStatus { get; private set; }

        public BotOwner BotOwner { get; private set; }

        private ManualLogSource Logger;
    }
}