using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.Debug;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using SAIN.SAINComponent.Classes.Memory;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes.Search;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent
{
    public class BotComponent : BotComponentBase
    {
        public bool IsCheater { get; private set; }

        public bool BotActive => BotActivation.BotActive;
        public bool BotInStandBy => BotActivation.BotInStandBy;
        public AILimitSetting CurrentAILimit => AILimit.CurrentAILimit;

        public bool HasEnemy => EnemyController.ActiveEnemy?.EnemyPerson.Active == true;
        public bool HasLastEnemy => EnemyController.LastEnemy?.EnemyPerson.Active == true;
        public Enemy Enemy => HasEnemy ? EnemyController.ActiveEnemy : null;
        public Enemy LastEnemy => HasLastEnemy ? EnemyController.LastEnemy : null;

        public Vector3? CurrentTargetPosition => CurrentTarget.CurrentTargetPosition;
        public Vector3? CurrentTargetDirection => CurrentTarget.CurrentTargetDirection;
        public float CurrentTargetDistance => CurrentTarget.CurrentTargetDistance;

        public BotGlobalEventsClass GlobalEvents { get; private set; }
        public BotBusyHandsDetector BusyHandsDetector { get; private set; }
        public ShootDeciderClass Shoot { get; private set; }
        public BotWeightManagement WeightManagement { get; private set; }
        public SAINBotMedicalClass Medical { get; private set; }
        public SAINActivationClass BotActivation { get; private set; }
        public DoorOpener DoorOpener { get; private set; }
        public ManualShootClass ManualShoot { get; private set; }
        public CurrentTargetClass CurrentTarget { get; private set; }
        public BotBackpackDropClass BackpackDropper { get; private set; }
        public BotLightController BotLight { get; private set; }
        public SAINBotSpaceAwareness SpaceAwareness { get; private set; }
        public AimDownSightsController AimDownSightsController { get; private set; }
        public SAINAILimit AILimit { get; private set; }
        public SAINBotSuppressClass Suppression { get; private set; }
        public SAINVaultClass Vault { get; private set; }
        public SAINSearchClass Search { get; private set; }
        public SAINMemoryClass Memory { get; private set; }
        public SAINEnemyController EnemyController { get; private set; }
        public SAINNoBushESP NoBushESP { get; private set; }
        public SAINFriendlyFireClass FriendlyFire { get; private set; }
        public SAINVisionClass Vision { get; private set; }
        public SAINMoverClass Mover { get; private set; }
        public SAINBotUnstuckClass BotStuck { get; private set; }
        public SAINHearingSensorClass Hearing { get; private set; }
        public SAINBotTalkClass Talk { get; private set; }
        public SAINDecisionClass Decision { get; private set; }
        public SAINCoverClass Cover { get; private set; }
        public SAINBotInfoClass Info { get; private set; }
        public SAINSquadClass Squad { get; private set; }
        public SAINSelfActionClass SelfActions { get; private set; }
        public BotGrenadeManager Grenade { get; private set; }
        public SAINSteeringClass Steering { get; private set; }
        public AimClass Aim { get; private set; }
        public CoroutineManager<BotComponent> CoroutineManager { get; private set; }

        public bool IsDead => !Person.ActivationClass.IsAlive;
        public bool GameEnding => BotActivation.GameEnding;
        public bool SAINLayersActive => BotActivation.SAINLayersActive;

        public float DistanceToAimTarget {
            get
            {
                if (BotOwner.AimingManager.CurrentAiming != null) {
                    return BotOwner.AimingManager.CurrentAiming.LastDist2Target;
                }
                return CurrentTarget.CurrentTargetDistance;
            }
        }

        public float LastCheckVisibleTime;

        public ESAINLayer ActiveLayer {
            get
            {
                return BotActivation.ActiveLayer;
            }
            set
            {
                BotActivation.SetActiveLayer(value);
            }
        }

        private void Update()
        {
            BotActivation.Update();
            if (!BotActive) {
                return;
            }

            AILimit.Update();
            CurrentTarget.Update();
            EnemyController.Update();
            BotStuck.Update();
            Decision.Update();
            GlobalEvents.Update();

            if (BotInStandBy) {
                return;
            }

            Info.Update();
            BusyHandsDetector.Update();
            WeightManagement.Update();
            DoorOpener.Update();
            Aim.Update();
            Search.Update();
            Memory.Update();
            FriendlyFire.Update();
            Vision.Update();
            Mover.Update();
            Hearing.Update();
            Talk.Update();
            Cover.Update();
            Squad.Update();
            SelfActions.Update();
            Grenade.Update();
            Steering.Update();
            Vault.Update();
            Suppression.Update();
            AimDownSightsController.Update();
            SpaceAwareness.Update();
            Medical.Update();
            BotLight.Update();
            ManualShoot.Update();
            Shoot.Update();

            handleDumbShit();
        }

        public bool InitializeBot(PersonClass person)
        {
            if (base.Init(person) == false) {
                return false;
            }
            if (createClasses(person) == false) {
                return false;
            }
            if (addToSquad() == false) {
                return false;
            }
            if (initClasses() == false) {
                return false;
            }
            if (finishInit() == false) {
                return false;
            }
            return true;
        }

        private bool createClasses(PersonClass person)
        {
            try {
                // Must be first, other classes use it
                Info = initBotClass<SAINBotInfoClass>();

                CoroutineManager = new CoroutineManager<BotComponent>(this);
                NoBushESP = this.gameObject.AddComponent<SAINNoBushESP>();

                Squad =
                    initBotClass<SAINSquadClass>();
                BusyHandsDetector =
                    initBotClass<BotBusyHandsDetector>();
                GlobalEvents =
                    initBotClass<BotGlobalEventsClass>();
                Shoot =
                    initBotClass<ShootDeciderClass>();
                WeightManagement =
                    initBotClass<BotWeightManagement>();
                Memory =
                    initBotClass<SAINMemoryClass>();
                BotStuck =
                    initBotClass<SAINBotUnstuckClass>();
                Hearing =
                    initBotClass<SAINHearingSensorClass>();
                Talk =
                    initBotClass<SAINBotTalkClass>();
                Decision =
                    initBotClass<SAINDecisionClass>();
                Cover =
                    initBotClass<SAINCoverClass>();
                SelfActions =
                    initBotClass<SAINSelfActionClass>();
                Steering =
                    initBotClass<SAINSteeringClass>();
                Grenade =
                    initBotClass<BotGrenadeManager>();
                Mover =
                    initBotClass<SAINMoverClass>();
                EnemyController =
                    initBotClass<SAINEnemyController>();
                FriendlyFire =
                    initBotClass<SAINFriendlyFireClass>();
                Vision =
                    initBotClass<SAINVisionClass>();
                Search =
                    initBotClass<SAINSearchClass>();
                Vault =
                    initBotClass<SAINVaultClass>();
                Suppression =
                    initBotClass<SAINBotSuppressClass>();
                AILimit =
                    initBotClass<SAINAILimit>();
                AimDownSightsController =
                    initBotClass<AimDownSightsController>();
                SpaceAwareness =
                    initBotClass<SAINBotSpaceAwareness>();
                DoorOpener =
                    initBotClass<DoorOpener>();
                Medical =
                    initBotClass<SAINBotMedicalClass>();
                BotLight =
                    initBotClass<BotLightController>();
                BackpackDropper =
                    initBotClass<BotBackpackDropClass>();
                CurrentTarget =
                    initBotClass<CurrentTargetClass>();
                ManualShoot =
                    initBotClass<ManualShootClass>();
                BotActivation =
                    initBotClass<SAINActivationClass>();
                Aim =
                    initBotClass<AimClass>();
            }
            catch (Exception ex) {
                Logger.LogError($"Error When Creating Classes, Disposing... : {ex}");
                return false;
            }
            return true;
        }

        private T initBotClass<T>() where T : BotBase
        {
            T botClass = (T)Activator.CreateInstance(typeof(T), new object[] { this });
            _botClasses.Add(typeof(T), botClass as IBotClass);
            return botClass;
        }

        private bool addToSquad()
        {
            try {
                Squad.SquadInfo.AddMember(this);
            }
            catch (Exception ex) {
                Logger.LogError($"Error adding member to squad!: {ex}");
                return false;
            }
            return true;
        }

        private bool initClasses()
        {
            try {
                NoBushESP.Init(Person.AIInfo.BotOwner, this);
            }
            catch (Exception ex) {
                Logger.LogError($"Error When Initializing Components, Disposing... : {ex}");
                return false;
            }
            foreach (var botClass in _botClasses) {
                try {
                    botClass.Value.Init();
                }
                catch (Exception ex) {
                    Logger.LogError($"Error When Initializing Class [{botClass.Key.ToString()}], Disposing... : {ex}");
                    return false;
                }
            }
            return true;
        }

        private bool finishInit()
        {
            try {
                if (!verifyBrain(Person)) {
                    Logger.LogError("Init SAIN ERROR, Disposing...");
                    return false;
                }

                try {
					BotOwner.LookSensor.MaxShootDist = float.MaxValue;
					if (BotOwner.AIData is GClass567 aiData)
					{
						aiData.IsNoOffsetShooting = false;
					}
				}
                catch (Exception ex) {
                    Logger.LogError($"Error setting MaxShootDist during init, but continuing with initialization...: {ex}");
                }

                try {
                    var settings = GlobalSettingsClass.Instance.General.Jokes;
                    if (settings.RandomCheaters &&
                        (EFTMath.RandomBool(settings.RandomCheaterChance) || Player.Profile.Nickname.ToLower().Contains("solarint"))) {
                        IsCheater = true;
                    }
                }
                catch (Exception ex) {
                    Logger.LogWarning($"Error when initializing dumb shit for this bot, continuing anyways since its some dumb shit. Error: {ex}");
                }
            }
            catch (Exception ex) {
                Logger.LogError($"Error When Finishing Bot Initialization, Disposing... : {ex}");
                return false;
            }
            return true;
        }

        private bool verifyBrain(PersonClass person)
        {
            if (Info.Profile.IsPMC &&
                person.AIInfo.BotOwner.Brain.BaseBrain.ShortName() != Brain.PMC.ToString()) {
                Logger.LogAndNotifyError($"{BotOwner.name} is a PMC but does not have [PMC] Base Brain! Current Brain Assignment: [{person.AIInfo.BotOwner.Brain.BaseBrain.ShortName()}] : SAIN Server mod is either missing or another mod is overwriting it. Destroying SAIN for this bot...");
                return false;
            }
            return true;
        }

        private void OnDisable()
        {
            BotActivation.SetActive(false);
            StopAllCoroutines();
        }

        private void LateUpdate()
        {
            BotActivation.LateUpdate();
        }

        private void handleDumbShit()
        {
            if (IsCheater) {
                if (defaultMoveSpeed == 0) {
                    defaultMoveSpeed = Player.MovementContext.MaxSpeed;
                    defaultSprintSpeed = Player.MovementContext.SprintSpeed;
                }
                Player.Grounder.enabled = Enemy == null;
                if (Enemy != null) {
                    Player.MovementContext.SetCharacterMovementSpeed(350, true);
                    Player.MovementContext.SprintSpeed = 50f;
                    Player.ChangeSpeed(100f);
                    Player.UpdateSpeedLimit(100f, Player.ESpeedLimit.SurfaceNormal);
                    Player.MovementContext.ChangeSpeedLimit(100f, Player.ESpeedLimit.SurfaceNormal);
                    BotOwner.SetTargetMoveSpeed(100f);
                }
                else {
                    Player.MovementContext.SetCharacterMovementSpeed(defaultMoveSpeed, false);
                    Player.MovementContext.SprintSpeed = defaultSprintSpeed;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            BotActivation?.SetActive(false);
            StopAllCoroutines();

            foreach (var botClass in _botClasses) {
                try {
                    botClass.Value.Dispose();
                }
                catch (Exception ex) {
                    Logger.LogError($"Dispose Class [{botClass.Key.ToString()}] Error: {ex}");
                }
            }

            if (NoBushESP != null)
                GameObject.Destroy(NoBushESP);
            if (BotOwner != null)
                BotOwner.OnBotStateChange -= resetBot;

            Destroy(this);
        }

        private void resetBot(EBotState state)
        {
            Decision.ResetDecisions(false);
        }

        private Dictionary<Type, IBotClass> _botClasses { get; } = new Dictionary<Type, IBotClass>();
        private float defaultMoveSpeed;
        private float defaultSprintSpeed;
    }
}