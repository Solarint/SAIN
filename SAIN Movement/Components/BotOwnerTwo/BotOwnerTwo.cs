using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.Game.Spawning;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using SAIN.Movement.Components;

namespace SAIN
{
    public sealed class BotOwnerNew : MonoBehaviour, IAIDetails
    {
        public const float DIST_CHECK_NAVMESH = 0.2f;
        public const string PATH_TO_AI = "AI";
        public const string PATH_TO_AI_DEBUG = "AIDebug";
        public static readonly Vector3 STAY_HEIGHT = new Vector3(0f, 1.15f, 0f);
        public static int BotCount;
        public CustomPath CurrPath;
        public GClass277 DebugMemory;
        public GClass282 DecisionProxy;
        public Transform LookedTransform;
        public BotMemoryClass Memory;
        public BotDifficultySettingsClass Settings;
        private Action<BotOwnerNew> action_0;
        [CompilerGenerated]
        private Action<EBotState> action_1;

        private bool bool_0;
        private bool bool_1;

        private EBotState ebotState_0;
        private float float_0 = 2f;
        private float float_1;
        private float float_2;
        private float float_3;

        public event Action<EBotState> OnBotStateChange
        {
            [CompilerGenerated]
            add
            {
                Action<EBotState> action = this.action_1;
                Action<EBotState> action2;
                do
                {
                    action2 = action;
                    Action<EBotState> value2 = (Action<EBotState>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<EBotState>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<EBotState> action = this.action_1;
                Action<EBotState> action2;
                do
                {
                    action2 = action;
                    Action<EBotState> value2 = (Action<EBotState>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<EBotState>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
        }

        public float ActivateTime
        {
            get
            {
                return this.float_2;
            }
        }

        public AiDataClass AIData
        {
            get
            {
                return this.GetPlayer.AIData;
            }
        }

        public GInterface5 AimingData { get; private set; }
        public Ambush Ambush { get; private set; }
        public ArtilleryDangerPlace ArtilleryDangerPlace { get; private set; }
        public AssaultBuildingData AssaultBuildingData { get; private set; }
        public AssaultDangerArea AssaultDangerArea { get; private set; }
        public BewareGrenade BewareGrenade { get; private set; }
        public Boss Boss { get; private set; }
        public BotAttackManager BotAttackManager { get; private set; }
        public BotFollowerClass BotFollower { get; private set; }
        public BotLayClass BotLay { get; private set; }
        public GClass340 BotLight { get; private set; }
        public GClass569 BotPersonalStats { get; private set; }
        public GClass466 BotRequestController { get; private set; }
        public GClass348 BotRun { get; private set; }
        public BotControllerClass BotsController { get; private set; }
        public BotGroupClass BotsGroup { get; set; }
        public EBotState BotState
        {
            get
            {
                return this.ebotState_0;
            }
            private set
            {
                this.ebotState_0 = value;
                if (this.action_1 != null)
                {
                    this.action_1(this.ebotState_0);
                }
            }
        }

        public GClass352 BotTalk { get; private set; }
        public GClass394 BotTurnAwayLight { get; private set; }
        public BotBrainClass Brain { get; private set; }
        public GClass367 CalledData { get; private set; }
        public GClass366 CallForHelp { get; private set; }
        public bool CanSprintPlayer
        {
            get
            {
                return this.GetPlayer.Physical.CanSprint;
            }
        }

        public GClass322 CellData { get; private set; }
        public GClass323 Covers { get; private set; }
        public GClass324 DangerArea { get; private set; }
        public GClass325 DangerPointsData { get; private set; }
        public GClass368 DeadBodyData { get; private set; }
        public GClass326 DeadBodyWork { get; private set; }
        public GClass370 DecisionQueue { get; private set; }
        public GClass327 DelayActions { get; private set; }
        public Vector3 Destination
        {
            get
            {
                return this.Mover.CurPathLastPoint;
            }
        }

        public GClass328 DogFight { get; private set; }
        public GClass463 DoorOpener { get; private set; }
        public GClass432 EatDrinkData { get; private set; }
        public EnemiesControllerClass EnemiesController { get; private set; }
        public float ENEMY_LOOK_AT_ME { get; private set; }
        public GClass381 EnemyChooser { get; private set; }
        public GClass329 EnemyLookData { get; private set; }
        public GClass452 ExternalItemsController { get; private set; }
        public GClass391 FindPlaceToShoot { get; private set; }
        public BifacialTransform Fireport
        {
            get
            {
                if (this.GetPlayer.MultiBarrelFireports != null && this.GetPlayer.MultiBarrelFireports.Length != 0)
                {
                    return this.GetPlayer.MultiBarrelFireports[0];
                }
                if (this.GetPlayer.Fireport != null)
                {
                    return this.GetPlayer.Fireport;
                }
                return this.WeaponRoot;
            }
        }

        public GClass330 FlashGrenade { get; private set; }
        public GClass433 FriendChecker { get; private set; }
        public GClass434 FriendlyTilt { get; private set; }
        public GameTimeClass GameDateTime { get; private set; }
        public GClass331 GameEventsData { get; private set; }
        public GClass435 Gesture { get; private set; }
        public Player GetPlayer { get; private set; }
        public GClass334 GiftData { get; private set; }
        public GClass389 GoalCulculator { get; private set; }
        public GClass406 GoToSomePointData { get; private set; }
        public GClass392 GrenadeSuicide { get; private set; }
        public string GroupId
        {
            get
            {
                return this.Profile.Info.GroupId;
            }
        }

        public bool HasPathAndNotComplete
        {
            get
            {
                return this.Mover.HasPathAndNoComplete;
            }
        }

        public GClass337 HeadData { get; private set; }
        public GClass403 HealAnotherTarget { get; private set; }
        public GClass404 HealingBySomebody { get; private set; }
        public IHealthController HealthController
        {
            get
            {
                return this.GetPlayer.HealthController;
            }
        }

        public GClass549 HearingSensor { get; private set; }
        public int Id { get; private set; }
        public string Infiltration
        {
            get
            {
                return this.Profile.Info.EntryPoint;
            }
        }

        public bool IsAI
        {
            get
            {
                return this.AIData != null && this.AIData.IsAI;
            }
        }

        public bool IsDead { get; set; }
        public GClass453 ItemDropper { get; private set; }
        public GClass454 ItemTaker { get; private set; }
        public Vector2 Lean { get; set; }
        public GClass339 LeaveData { get; private set; }
        public GClass416 LookData { get; private set; }
        public Vector3 LookDirection
        {
            get
            {
                return this.GetPlayer.LookDirection;
            }
        }

        public BotLookSensor LookSensor { get; private set; }
        public GClass341 LootOpener { get; private set; }
        public BotLoyaltyClass Loyalty
        {
            get
            {
                return this.GetPlayer.Loyalty;
            }
        }

        public GClass342 LoyaltyData { get; private set; }
        public GClass343 MagazineChecker { get; private set; }
        public Dictionary<BodyPartType, BodyPartClass> MainParts { get; private set; }
        public GClass405 Medecine { get; private set; }
        public GClass408 Mover { get; private set; }
        public GClass407 MoveToEnemyData { get; private set; }
        public BifacialTransform MyHead { get; private set; }
        public GClass412 NavMeshCutterController { get; private set; }
        public GClass393 NightVision { get; private set; }
        public GClass430 PatrollingData { get; private set; }
        public GClass438 PeacefulActions { get; private set; }
        public GClass436 PeaceHardAim { get; private set; }
        public GClass437 PeaceLook { get; private set; }
        public GClass456 PlanDropItem { get; private set; }
        public GClass332 PlayerFollowData { get; private set; }
        public Vector3 Position
        {
            get
            {
                return this.Transform.position;
            }
        }

        public GClass418 PriorityAxeTarget { get; private set; }
        public Profile Profile
        {
            get
            {
                return this.GetPlayer.Profile;
            }
        }

        public string ProfileId { get; private set; }
        public GClass347 Receiver { get; private set; }
        public GClass545 RecoilData { get; private set; }
        public GClass376 SearchData { get; private set; }
        public GClass439 SecondWeaponData { get; private set; }
        public GClass546 ShootData { get; private set; }
        public GClass349 ShootFromPlace { get; private set; }
        public EPlayerSide Side
        {
            get
            {
                return this.GetPlayer.Profile.Info.Side;
            }
        }

        public GClass350 SmokeGrenade { get; private set; }
        public IBotData SpawnProfileData { get; set; }
        public GClass351 StandBy { get; private set; }
        public GClass413 Steering { get; private set; }
        public GClass441 SuppressGrenade { get; private set; }
        public GClass442 SuppressShoot { get; private set; }
        public GClass443 SuppressStationary { get; private set; }
        public GClass459 SuspiciousPlaceData { get; private set; }
        public BotTacticClass Tactic { get; private set; }
        public string TeamId
        {
            get
            {
                return this.Profile.Info.TeamId;
            }
        }

        public GClass353 Tilt { get; private set; }
        [Obsolete("Use Player.Transform instead!", true)]
        public new Transform transform
        {
            get
            {
                return base.transform;
            }
        }

        public BifacialTransform Transform
        {
            get
            {
                return this.GetPlayer.PlayerBones.BodyTransform;
            }
        }

        public GClass414 TrianglePosition { get; private set; }
        public GClass415 UnityEditorRunChecker { get; private set; }
        public GClass362 WeaponManager { get; private set; }
        public BifacialTransform WeaponRoot
        {
            get
            {
                return this.GetPlayer.PlayerBones.WeaponRoot;
            }
        }
        public static BotOwnerNew Create(Player player, GameObject behaviourTreePrefab, GameTimeClass gameDataTime, BotControllerClass botsController, bool isLocalGame)
        {
            player.ProceduralWeaponAnimation.Mask = EProceduralAnimationMask.DrawDown;
            player.Profile.UncoverAll(null);
            BotDifficulty difficulty;
            WildSpawnType role;
            if (player.Profile.Info != null && player.Profile.Info.Settings != null)
            {
                difficulty = player.Profile.Info.Settings.BotDifficulty;
                role = player.Profile.Info.Settings.Role;
            }
            else
            {
                difficulty = BotDifficulty.normal;
                role = WildSpawnType.assault;
            }
            BotDifficultySettingsClass settings = Singleton<GClass563>.Instance.GetSettings(difficulty, role);
            BotOwnerNew botOwner = player.gameObject.AddComponent<BotOwnerNew>();
            botOwner.bool_1 = isLocalGame;
            botOwner.Settings = settings;
            botOwner.BotTalk = new GClass352(botOwner);
            botOwner.Tactic = new BotTacticClass(botOwner);
            botOwner.name = string.Format("Bot{0}", ++BotOwnerNew.BotCount);
            player.SetOwnerToAIData(botOwner);
            botOwner.ENEMY_LOOK_AT_ME = Mathf.Cos(botOwner.Settings.FileSettings.Mind.ENEMY_LOOK_AT_ME_ANG * 0.017453292f);
            botOwner.BotsController = botsController;
            botOwner.GetPlayer = player;
            botOwner.Id = player.Id;
            botOwner.ProfileId = player.Profile.Id;
            botOwner.GetPlayer.ActiveHealthController.SetDamageCoeff(botOwner.Settings.FileSettings.Core.DamageCoeff);
            botOwner.MyHead = player.PlayerBones.Head;
            botOwner.Brain = new BotBrainClass(botOwner);
            botOwner.DecisionProxy = new GClass282(botOwner);
            botOwner.DecisionQueue = new GClass370(botOwner);
            botOwner.BotLight = new GClass340(botOwner);
            botOwner.BotTurnAwayLight = new GClass394(botOwner);
            botOwner.LookData = new GClass416(botOwner);
            botOwner.HeadData = new GClass337(botOwner);
            botOwner.NavMeshCutterController = new GClass412(botOwner);
            botOwner.GrenadeSuicide = new GClass392(botOwner);
            botOwner.EatDrinkData = new GClass432(botOwner);
            botOwner.SecondWeaponData = new GClass439(botOwner);
            botOwner.MagazineChecker = new GClass343(botOwner);
            botOwner.FriendlyTilt = new GClass434(botOwner);
            botOwner.GoalCulculator = new GClass389(botOwner);
            botOwner.PlanDropItem = new GClass456(botOwner);
            botOwner.ItemTaker = new GClass454(botOwner);
            botOwner.ExternalItemsController = new GClass452(botOwner);
            botOwner.PeaceHardAim = new GClass436(botOwner);
            botOwner.PeaceLook = new GClass437(botOwner);
            botOwner.MoveToEnemyData = new GClass407(botOwner);
            botOwner.DangerArea = new GClass324(botOwner);
            botOwner.AssaultDangerArea = new GClass320(botOwner);
            botOwner.ItemDropper = new GClass453(botOwner);
            botOwner.PlayerFollowData = new GClass332(botOwner);
            botOwner.LoyaltyData = new GClass342(botOwner);
            botOwner.AssaultBuildingData = new GClass319(botOwner);
            botOwner.FindPlaceToShoot = new GClass391(botOwner);
            botOwner.GoToSomePointData = new GClass406(botOwner);
            botOwner.FriendChecker = new GClass433(botOwner, botsController.Bots.GetConnector());
            botOwner.PeacefulActions = new GClass438(botOwner);
            botOwner.SuspiciousPlaceData = new GClass459(botOwner);
            botOwner.AimingData = new GClass544(botOwner);
            botOwner.Covers = new GClass323(botOwner);
            botOwner.StandBy = new GClass351(botOwner);
            botOwner.SuppressStationary = new GClass443(botOwner);
            botOwner.EnemyLookData = new GClass329(botOwner, true);
            botOwner.HealingBySomebody = new GClass404(botOwner);
            botOwner.DelayActions = new GClass327(botOwner);
            botOwner.Medecine = new GClass405(botOwner);
            botOwner.LeaveData = new GClass339(botOwner);
            botOwner.BotFollower = BotFollowerClass.Create(botOwner);
            botOwner.UnityEditorRunChecker = new GClass415(botOwner);
            botOwner.EnemiesController = EnemiesControllerClass.Create(botOwner);
            botOwner.Boss = new GClass311(botOwner);
            botOwner.DoorOpener = new GClass463(botOwner);
            botOwner.RecoilData = new GClass545(botOwner);
            botOwner.LootOpener = new GClass341(botOwner);
            botOwner.DeadBodyWork = new GClass326(botOwner);
            botOwner.WeaponManager = new GClass362(botOwner);
            botOwner.BotRun = new GClass348(botOwner);
            botOwner.HealAnotherTarget = new GClass403(botOwner);
            botOwner.Steering = new GClass413(botOwner);
            botOwner.ShootData = new GClass546(botOwner, botOwner.RecoilData);
            botOwner.DeadBodyData = new GClass368(botOwner);
            botOwner.BotLay = new BotLayClass(botOwner);
            botOwner.Tilt = new GClass353(botOwner);
            botOwner.TrianglePosition = new GClass414(botOwner);
            botOwner.GoToSomePointData = new GClass406(botOwner);
            botOwner.Receiver = new GClass347(botOwner);
            botOwner.NightVision = new GClass393(botOwner);
            botOwner.SearchData = GClass376.Create(botOwner);
            botOwner.GoToSomePointData = new GClass406(botOwner);
            botOwner.Gesture = new GClass435(botOwner);
            botOwner.GameDateTime = gameDataTime;
            botOwner.LookSensor = new GClass550(botOwner);
            botOwner.BotAttackManager = new GClass293(botOwner);
            botOwner.HearingSensor = new GClass549(botOwner);
            botOwner.BotRequestController = new GClass466(botOwner);
            botOwner.MainParts = BodyPartClass.Create(botOwner, player.PlayerBones);
            botOwner.BotPersonalStats = new GClass569();
            botOwner.ShootFromPlace = new GClass349(botOwner);
            botOwner.DebugMemory = new GClass277(botOwner);
            botOwner.BewareGrenade = new GClass321(botOwner);
            botOwner.ArtilleryDangerPlace = new GClass457(botOwner);
            botOwner.FlashGrenade = new GClass330(botOwner);
            botOwner.CellData = new GClass322(botOwner);
            botOwner.DogFight = new GClass328(botOwner);
            botOwner.CallForHelp = new GClass366(botOwner);
            botOwner.CalledData = new GClass367(botOwner);
            botOwner.Ambush = new GClass318(botOwner);
            botOwner.PriorityAxeTarget = new GClass418(botOwner);
            botOwner.SmokeGrenade = new GClass350(botOwner);
            botOwner.SuppressShoot = new GClass442(botOwner);
            botOwner.SuppressGrenade = new GClass441(botOwner);
            botOwner.SuppressStationary = new GClass443(botOwner);
            botOwner.DangerPointsData = new GClass325(botOwner);
            botOwner.EnemyChooser = GClass381.Create(botOwner);
            botOwner.GiftData = new GClass334(botOwner);
            if (botOwner.Settings.FileSettings.Move.ETERNITY_STAMINA)
            {
                botOwner.GetPlayer.Physical.Stamina.ForceMode = true;
                botOwner.GetPlayer.Physical.HandsStamina.ForceMode = true;
            }
            return botOwner;
        }

        public void CalcGoal()
        {
            this.float_0 = GClass560.Core.UPDATE_GOAL_TIMER_SEC + Time.time;
            this.BotsGroup.CalcGoalForBot(this);
        }

        public HashSet<Vector3> CarePositions()
        {
            return this.Covers.CarePositions();
        }

        [CanBeNull]
        public ShootPointClass CurrentEnemyTargetPosition(bool sensPosition)
        {
            if (this.Memory.GoalEnemy == null)
            {
                return null;
            }
            Vector3 point;
            if (sensPosition)
            {
                point = this.Memory.GoalEnemy.EnemyLastPosition + BotOwnerNew.STAY_HEIGHT;
            }
            else
            {
                point = this.Memory.GoalEnemy.BodyPart.Position;
            }
            return new ShootPointClass(point, 1f);
        }

        public void Deactivate()
        {
            this.BotState = EBotState.NonActive;
        }

        public void Disable()
        {
            this.BotState = EBotState.NonActive;
        }

        public void Dispose()
        {
            this.action_1 = null;
            float time = Time.time;
            if (this.ebotState_0 == EBotState.PreActive)
            {
                return;
            }
            this.BotState = EBotState.Disposed;
            try
            {
                BotBrainClass brain = this.Brain;
                if (brain != null)
                {
                    brain.Dispose();
                }
                GClass457 artilleryDangerPlace = this.ArtilleryDangerPlace;
                if (artilleryDangerPlace != null)
                {
                    artilleryDangerPlace.Dispose();
                }
                AiDataClass aidata = this.AIData;
                if (aidata != null)
                {
                    GClass528 askRequests = aidata.AskRequests;
                    if (askRequests != null)
                    {
                        askRequests.DisposeAll(null);
                    }
                }
                GClass408 mover = this.Mover;
                if (mover != null)
                {
                    mover.Dispose();
                }
                GClass441 suppressGrenade = this.SuppressGrenade;
                if (suppressGrenade != null)
                {
                    suppressGrenade.Dispose();
                }
                GClass442 suppressShoot = this.SuppressShoot;
                if (suppressShoot != null)
                {
                    suppressShoot.Dispose();
                }
                GClass443 suppressStationary = this.SuppressStationary;
                if (suppressStationary != null)
                {
                    suppressStationary.Dispose();
                }
                BotLayClass botLay = this.BotLay;
                if (botLay != null)
                {
                    botLay.Dispose();
                }
                GClass319 assaultBuildingData = this.AssaultBuildingData;
                if (assaultBuildingData != null)
                {
                    assaultBuildingData.Dispose();
                }
                GClass342 loyaltyData = this.LoyaltyData;
                if (loyaltyData != null)
                {
                    loyaltyData.Dispose();
                }
                GClass311 boss = this.Boss;
                if (boss != null)
                {
                    boss.Dispose();
                }
                BotTacticClass tactic = this.Tactic;
                if (tactic != null)
                {
                    tactic.Dispose();
                }
                GClass434 friendlyTilt = this.FriendlyTilt;
                if (friendlyTilt != null)
                {
                    friendlyTilt.Dispose();
                }
                GClass452 externalItemsController = this.ExternalItemsController;
                if (externalItemsController != null)
                {
                    externalItemsController.Dispose();
                }
                GClass324 dangerArea = this.DangerArea;
                if (dangerArea != null)
                {
                    dangerArea.Dispose();
                }
                GClass339 leaveData = this.LeaveData;
                if (leaveData != null)
                {
                    leaveData.Dispose();
                }
                GClass381 enemyChooser = this.EnemyChooser;
                if (enemyChooser != null)
                {
                    enemyChooser.Dispose();
                }
                GClass323 covers = this.Covers;
                if (covers != null)
                {
                    covers.Dispose();
                }
                GClass277 debugMemory = this.DebugMemory;
                if (debugMemory != null)
                {
                    debugMemory.Dispose();
                }
                GClass376 searchData = this.SearchData;
                if (searchData != null)
                {
                    searchData.Dispose();
                }
                GClass332 playerFollowData = this.PlayerFollowData;
                if (playerFollowData != null)
                {
                    playerFollowData.Dispose();
                }
                GClass362 weaponManager = this.WeaponManager;
                if (weaponManager != null)
                {
                    weaponManager.Dispose();
                }
                BotLookSensor lookSensor = this.LookSensor;
                if (lookSensor != null)
                {
                    lookSensor.Dispose();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                GClass415 unityEditorRunChecker = this.UnityEditorRunChecker;
                if (unityEditorRunChecker != null)
                {
                    unityEditorRunChecker.Dispose();
                }
                GClass405 medecine = this.Medecine;
                if (medecine != null)
                {
                    medecine.Dispose();
                }
                GClass412 navMeshCutterController = this.NavMeshCutterController;
                if (navMeshCutterController != null)
                {
                    navMeshCutterController.Dispose();
                }
                GClass330 flashGrenade = this.FlashGrenade;
                if (flashGrenade != null)
                {
                    flashGrenade.Dispose();
                }
                GClass430 patrollingData = this.PatrollingData;
                if (patrollingData != null)
                {
                    patrollingData.Dispose();
                }
                GClass438 peacefulActions = this.PeacefulActions;
                if (peacefulActions != null)
                {
                    peacefulActions.Dispose();
                }
                BotFollowerClass botFollower = this.BotFollower;
                if (botFollower != null)
                {
                    botFollower.Dispose();
                }
                GClass546 shootData = this.ShootData;
                if (shootData != null)
                {
                    shootData.Dispose();
                }
                GClass466 botRequestController = this.BotRequestController;
                if (botRequestController != null)
                {
                    botRequestController.Dispose();
                }
                GClass327 delayActions = this.DelayActions;
                if (delayActions != null)
                {
                    delayActions.Dispose();
                }
                GClass432 eatDrinkData = this.EatDrinkData;
                if (eatDrinkData != null)
                {
                    eatDrinkData.Dispose();
                }
                GClass436 peaceHardAim = this.PeaceHardAim;
                if (peaceHardAim != null)
                {
                    peaceHardAim.Dispose();
                }
                GClass454 itemTaker = this.ItemTaker;
                if (itemTaker != null)
                {
                    itemTaker.Dispose();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                this.method_8();
            }
            catch (Exception)
            {
            }
            try
            {
                GClass430 patrollingData2 = this.PatrollingData;
                if (patrollingData2 != null)
                {
                    patrollingData2.Disable();
                }
                Collider[] componentsInChildren = base.GetComponentsInChildren<Collider>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    componentsInChildren[i].isTrigger = false;
                }
                this.BotState = EBotState.NonActive;
                GClass549 hearingSensor = this.HearingSensor;
                if (hearingSensor != null)
                {
                    hearingSensor.Dispose();
                }
                GClass347 receiver = this.Receiver;
                if (receiver != null)
                {
                    receiver.Dispose();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                BotMemoryClass memory = this.Memory;
                if (memory != null)
                {
                    memory.Dispose();
                }
                GClass569 botPersonalStats = this.BotPersonalStats;
                if (botPersonalStats != null)
                {
                    botPersonalStats.Dispose();
                }
            }
            catch (Exception)
            {
            }
            this.PeaceLook = null;
            this.EnemyChooser = null;
            this.Brain = null;
            this.ArtilleryDangerPlace = null;
            this.Mover = null;
            this.SuppressGrenade = null;
            this.AssaultBuildingData = null;
            this.SuppressShoot = null;
            this.BotLay = null;
            this.LoyaltyData = null;
            this.FriendlyTilt = null;
            this.ExternalItemsController = null;
            this.DangerArea = null;
            this.LeaveData = null;
            this.Covers = null;
            this.DebugMemory = null;
            this.SearchData = null;
            this.UnityEditorRunChecker = null;
            this.Medecine = null;
            this.PatrollingData = null;
            this.BotFollower = null;
            this.DelayActions = null;
            this.EatDrinkData = null;
            this.PeaceHardAim = null;
            this.ItemTaker = null;
            this.PatrollingData = null;
            this.HearingSensor = null;
            this.Receiver = null;
        }

        public float DistTo(Vector3 v)
        {
            return (this.Transform.position - v).magnitude;
        }

        public void GoToByWay(Vector3[] way, float reachDist = -1f)
        {
            if (reachDist < 0f)
            {
                reachDist = this.Settings.FileSettings.Move.REACH_DIST;
            }
            this.Mover.GoToByWay(way, reachDist, Vector3.zero);
        }

        public void GoToPoint(CustomNavigationPoint targetPoint, string debugdata = "")
        {
            this.Memory.GoToPoint(targetPoint, debugdata);
        }

        public NavMeshPathStatus GoToPoint(Vector3 position, bool slowAtTheEnd = true, float reachDist = -1f, bool getUpWithCheck = false, bool mustHaveWay = true, bool mustGetUp = true)
        {
            if (reachDist < 0f)
            {
                reachDist = this.Settings.FileSettings.Move.REACH_DIST;
            }
            return this.Mover.GoToPoint(position, slowAtTheEnd, reachDist, true, mustHaveWay, mustGetUp);
        }

        public bool IsEnemyLookingAtMe(GClass475 goalEnemy)
        {
            return goalEnemy != null && this.IsEnemyLookingAtMe(goalEnemy.Person);
        }

        public bool IsEnemyLookingAtMe(IAIDetails gamePerson)
        {
            Vector3 position = this.WeaponRoot.position;
            BifacialTransform weaponRoot = gamePerson.WeaponRoot;
            return GClass782.IsAngLessNormalized(GClass782.NormalizeFastSelf(position - weaponRoot.position), gamePerson.LookDirection, 0.9659258f);
        }

        public bool IsFollower()
        {
            return this.Profile != null && this.Profile.Info != null && this.Profile.Info.Settings != null && this.Profile.Info.Settings.IsFollower();
        }

        public bool IsRole(WildSpawnType role)
        {
            return this.Profile != null && this.Profile.Info != null && this.Profile.Info.Settings != null && this.Profile.Info.Settings.Role == role;
        }

        public void MovementPause(float pauseTime)
        {
            this.Mover.MovementPause(pauseTime, true);
        }

        public void MovementResume()
        {
            this.Mover.MovementResume();
        }

        public void OnDrawGizmosSelected()
        {
            GClass278.DrawBotOwnerGizmosSelected(this);
        }

        public void PostActivate()
        {
            if (this.BotState == EBotState.NonActive)
            {
                this.BotState = EBotState.PreActive;
            }
        }

        public void PreActivate(BotZone zone, GameTimeClass time, BotGroupClass group, bool autoActivate = true)
        {
            this.float_1 = Time.time;
            this.GameDateTime = time;
            this.BotsGroup = group;
            this.LookSensor.UpdateZoneValue(zone);
            this.Covers.Init(zone);
            if (this.GetPlayer.CharacterControllerCommon is GClass710)
            {
                this.Mover = new GClass409(this, this.GetPlayer);
            }
            else
            {
                this.Mover = new GClass410(this, this.GetPlayer);
            }
            this.WeaponManager.PreActivate();
            this.Memory = new BotMemoryClass(this, this.BotsGroup);
            this.PatrollingData = new GClass430(this);
            this.GameEventsData = new GClass331(this);
            this.method_4();
            this.DebugMemory.Init();
            if (autoActivate)
            {
                this.BotState = EBotState.PreActive;
            }
        }

        public void SayGroupAboutEnemy(IAIDetails person, Vector3? partPos = null)
        {
        }
        public float SDistTo(Vector3 v)
        {
            return (this.Transform.position - v).sqrMagnitude;
        }

        public void SetDieCallback(Action<BotOwnerNew> botDied)
        {
            this.action_0 = botDied;
        }

        public void SetHandle(string toString)
        {
        }

        public void SetLean(Vector2 lean)
        {
            this.Lean = lean;
        }

        public void SetPose(float targetPose)
        {
            this.Mover.SetPose(targetPose);
        }

        public void SetTargetMoveSpeed(float speed)
        {
            this.Mover.SetTargetMoveSpeed(speed);
        }

        public void SetYAngle(float angle)
        {
            this.Steering.SetYAngle(angle);
        }

        public void Sprint(bool val, bool withDebugCallback = true)
        {
            if (val)
            {
                this.SetPose(1f);
                this.AimingData.LoseTarget();
            }
            this.Mover.Sprint(val, withDebugCallback);
        }

        public void StopMove()
        {
            this.Mover.Stop();
        }

        public void UpdateManual()
        {
            if (this.BotState == EBotState.Active && this.GetPlayer.HealthController.IsAlive)
            {
                this.StandBy.Update();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                this.LookSensor.UpdateLook();
                stopwatch.Stop();
                if (this.StandBy.StandByType != BotStandByType.paused)
                {
                    if (this.float_0 < Time.time)
                    {
                        this.CalcGoal();
                    }
                    this.HeadData.ManualUpdate();
                    this.ShootData.ManualUpdate();
                    this.Tilt.ManualUpdate();
                    this.NightVision.ManualUpdate();
                    this.CellData.Update();
                    this.DogFight.ManualUpdate();
                    this.FriendChecker.ManualUpdate();
                    this.RecoilData.LosingRecoil();
                    this.Mover.ManualUpdate();
                    this.AimingData.PermanentUpdate();
                    this.TrianglePosition.ManualUpdate();
                    this.Medecine.ManualUpdate();
                    this.Boss.ManualUpdate();
                    this.BotTalk.ManualUpdate();
                    this.WeaponManager.ManualUpdate();
                    this.BotRequestController.Update();
                    this.Tactic.UpdateChangeTactics();
                    this.Memory.ManualUpdate(Time.deltaTime);
                    this.Settings.UpdateManual();
                    this.BotRequestController.TryToFind();
                    if (this.GetPlayer.UpdateQueue == EUpdateQueue.Update)
                    {
                        this.Mover.ManualFixedUpdate();
                        this.Steering.ManualFixedUpdate();
                    }
                    this.UnityEditorRunChecker.ManualLateUpdate();
                }
                return;
            }
            if (this.BotState == EBotState.PreActive && this.WeaponManager.IsReady)
            {
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(this.GetPlayer.Position, out navMeshHit, 0.2f, -1))
                {
                    this.method_10();
                    return;
                }
                if (this.float_3 < Time.time)
                {
                    this.float_3 = Time.time + 1f;
                    this.Transform.position = this.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
                    this.method_10();
                }
            }
        }

        private void FixedUpdate()
        {
            if (this.BotState != EBotState.Active)
            {
                return;
            }
            if (this.GetPlayer.UpdateQueue == EUpdateQueue.FixedUpdate)
            {
                this.Steering.ManualFixedUpdate();
                this.Mover.ManualFixedUpdate();
            }
        }

        private void method_0()
        {
            Collider collider = this.GetPlayer.CharacterControllerCommon.GetCollider();
            foreach (BotOwnerNew botOwner in this.BotsGroup.BotGame.BotsController.Bots.BotOwners)
            {
                GClass672.IgnoreCollision(botOwner.GetPlayer.CharacterControllerCommon.GetCollider(), collider, true);
            }
        }
        private void method_1(Func<int, bool> condition, IAIDetails person)
        {
            this.BotsGroup.RemoveInfo(person);
            if (this.Memory.GoalEnemy.Person.Id == person.Id)
            {
                this.Memory.GoalEnemy = null;
            }
        }
        private void method_10()
        {
            try
            {
                this.LookSensor.Activate();
                this.Settings.Activate();
                this.ExternalItemsController.Activate();
                this.ItemTaker.Activate();
                this.EnemyChooser.Activate();
                this.PlanDropItem.Activate();
                this.ItemDropper.Activate();
                this.NavMeshCutterController.Activate();
                this.AimingData.Activate();
                this.BotFollower.Activate();
                this.FriendlyTilt.Activate();
                this.Tactic.Activate();
                this.EnemiesController.Activate(this.BotsGroup.BotGame.BotsController.OnlineDependenceSettings.CanPersueAxeman);
                this.HearingSensor.Init();
                this.LeaveData.Activate();
                this.Receiver.Init();
                this.Mover.Activate();
                this.BotTalk.Activate();
                this.LoyaltyData.Activate();
                this.AssaultDangerArea.Activate();
                this.DangerArea.Activate();
                this.TrianglePosition.Activate(this.BotsGroup.BotZone.CachePathLength);
                this.BotPersonalStats.Init(this, this.BotsGroup.BotZone.name);
                this.StandBy.InitPoints(this.BotsGroup.BotZone.Modifier.DistToActivate, this.BotsGroup.BotZone.Modifier.DistToSleep);
                this.method_2();
                this.FlashGrenade.Activate();
                this.PeaceHardAim.Activate();
                this.ShootData.Activate();
                this.PeaceLook.Activate();
                this.CellData.Activate();
                this.UnityEditorRunChecker.Activate();
                this.NightVision.Activate();
                this.SearchData.Activate();
                this.Medecine.Activate();
                this.BotState = EBotState.Active;
                this.Memory.Activate();
                this.SuppressShoot.Activate();
                this.EatDrinkData.Activate();
                this.SecondWeaponData.Activate();
                this.SuppressGrenade.Activate();
                this.ArtilleryDangerPlace.Activate();
                this.method_11();
                this.Brain.Activate();
                this.PatrollingData.Activate();
                this.WeaponManager.Activate();
                this.BotFollower.TryFindBoss();
                this.float_2 = Time.time;
            }
            catch (Exception)
            {
                this.BotState = EBotState.ActiveFail;
            }
        }

        private void method_11()
        {
            if (this.Settings.FileSettings.Boss.EFFECT_PAINKILLER)
            {
                this.GetPlayer.ActiveHealthController.DoPainKiller();
            }
            if (this.Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN > 0f)
            {
                this.GetPlayer.ActiveHealthController.DoScavRegeneration(this.Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN);
            }
        }

        private void method_2()
        {
            this.GetPlayer.OnPlayerDead += this.method_3;
            this.GetPlayer.BeingHitAction += this.method_9;
        }

        private void method_3(Player player, Player lastAggressor, DamageInfo lastDamageInfo, EBodyPart lastBodyPart)
        {
            if (lastAggressor != null)
            {
                this.BotsGroup.ReportAboutEnemy(lastAggressor, EEnemyPartVisibleType.visible);
                this.method_9(lastDamageInfo, EBodyPart.Chest, 0f);
            }
        }

        private void method_4()
        {
            this.GetPlayer.HealthController.ApplyDamageEvent += this.method_7;
            this.GetPlayer.HealthController.DiedEvent += this.method_6;
            if (Singleton<GClass629>.Instantiated)
            {
                Singleton<GClass629>.Instance.OnBodyBotDead += this.method_5;
            }
        }

        private void method_5(Vector3 obj)
        {
            if (this.Memory.IsPeace && (obj - this.Transform.position).sqrMagnitude < this.Settings.FileSettings.Hearing.DEAD_BODY_SOUND_RAD)
            {
                this.BotsGroup.AddPointToSearch(obj, 80f, this, true);
            }
        }

        private void method_6(EDamageType damageType)
        {
            if (Singleton<GClass629>.Instantiated)
            {
                Vector3 position = this.Transform.position;
                Singleton<GClass629>.Instance.DeadBodySound(position);
            }
            this.BotsController.BotDied(this);
            this.BotPersonalStats.Death();
            this.Dispose();
            this.IsDead = true;
            this.BotsGroup.BotZone.ZoneDangerAreas.BotDied(this.Position);
            if (this.action_0 != null)
            {
                this.action_0(this);
                return;
            }
            UnityEngine.Debug.LogError("bot die but have problems: _botState:" + this.ebotState_0.ToString());
        }
        private void method_7(EBodyPart bodyPart, float damage, DamageInfo damageInfo)
        {
            damageInfo.DamageType.IsSelfInflicted();
        }
        private void method_8()
        {
            this.bool_0 = true;
            this.GetPlayer.OnPlayerDead -= this.method_3;
            this.GetPlayer.BeingHitAction -= this.method_9;
            this.GetPlayer.HealthController.ApplyDamageEvent -= this.method_7;
            this.GetPlayer.HealthController.DiedEvent -= this.method_6;
            if (Singleton<GClass629>.Instantiated)
            {
                Singleton<GClass629>.Instance.OnBodyBotDead -= this.method_5;
            }
        }

        private void method_9(DamageInfo damageInfo, EBodyPart bodyType, float damageReducedByArmor)
        {
            this.StandBy.GetHit();
            if (damageInfo.Player == null)
            {
                return;
            }
            if (!damageInfo.Player.IsAI && damageInfo.Player.Side == EPlayerSide.Savage && !this.Profile.Info.Settings.Role.IsHostileToEverybody() && !damageInfo.Player.Loyalty.CanBeFreeKilled)
            {
                damageInfo.Player.Loyalty.MarkAsCanBeFreeKilled();
            }
            this.BotPersonalStats.GetHit(damageInfo, bodyType);
            this.Memory.GetHit(damageInfo);
            if (damageInfo.Player != null && damageInfo.Player.Side == this.Side)
            {
                this.BotTalk.TrySay(EPhraseTrigger.FriendlyFire);
            }
        }
        private void OnDrawGizmos()
        {
        }
    }
}