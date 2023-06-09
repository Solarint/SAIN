﻿using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Classes;
using SAIN.Classes.Sense;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public List<Player> VisiblePlayers = new List<Player>();
        public List<string> VisiblePlayerIds = new List<string>();

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            AddComponents(BotOwner);
        }

        public Collider BotZoneCollider => BotZone?.Collider;
        public AIPlaceInfo BotZone => BotOwner.AIData.PlaceInfo;

        public List<Vector3> ExitsToLocation { get; private set; } = new List<Vector3>();

        public void UpdateExitsToLoc(Vector3[] exits)
        {
            //ExitsToLocation.Clear();
            //ExitsToLocation.AddRange(exits);
        }

        public string ProfileId => BotOwner.ProfileId;
        public string SquadId => Squad.SquadID;
        public Player Player => BotOwner.GetPlayer;

        private void AddComponents(BotOwner bot)
        {
            // Must be first, other classes use it
            Squad = bot.GetOrAddComponent<SquadClass>();
            Equipment = bot.GetOrAddComponent<BotEquipmentClass>();

            Info = new SAINBotInfo(bot);
            Info.Init();

            BotStuck = bot.GetOrAddComponent<SAINBotUnstuck>();
            Hearing = bot.GetOrAddComponent<HearingSensorClass>();
            Talk = bot.GetOrAddComponent<BotTalkClass>();
            Decision = bot.GetOrAddComponent<DecisionClass>();
            Cover = bot.GetOrAddComponent<CoverClass>();
            FlashLight = bot.GetPlayer.gameObject.AddComponent<FlashLightComponent>();
            SelfActions = bot.GetOrAddComponent<SelfActionClass>();
            Steering = bot.GetOrAddComponent<SteeringClass>();
            Grenade = bot.GetOrAddComponent<BotGrenadeClass>();
            Mover = bot.GetOrAddComponent<MoverClass>();
            NoBushESP = bot.GetOrAddComponent<NoBushESP>();
            EnemyController = bot.GetOrAddComponent<EnemyController>();
            Sounds = bot.GetOrAddComponent<SoundsController>();
            FriendlyFireClass = new FriendlyFireClass(bot);
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public EnemyController EnemyController { get; private set; }
        public NoBushESP NoBushESP { get; private set; }
        public bool NoBushESPActive => NoBushESP.NoBushESPActive;
        public SAINBotController BotController => SAINPlugin.BotController;

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

                Info?.Update();
                FriendlyFireClass?.Update();
                Enemy?.Update();
                BotOwner.DoorOpener?.Update();

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
                BotOwner.PatrollingData.Pause();
            }
            else
            {
                if (Enemy == null)
                {
                    BotOwner.PatrollingData.Unpause();
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

        public bool HasEnemy => Enemy != null;
        public SAINEnemy Enemy => EnemyController.Enemy;

        public void Dispose()
        {
            try
            {
                StopAllCoroutines();

                Info?.Dispose();
                Cover?.Dispose();

                Destroy(Squad);
                Destroy(Equipment);
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
                if (Enemy != null)
                {
                    return Enemy.Position;
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
                //if (Time.time - BotOwner.Memory.LastTimeHit < 20f && !BotOwner.Memory.IsPeace)
                //{
                //    return BotOwner.Memory.LastHitPos;
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