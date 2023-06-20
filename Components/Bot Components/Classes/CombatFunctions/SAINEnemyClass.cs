using BepInEx.Logging;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.VisionConfig;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace SAIN.Classes
{
    public class SAINEnemy : SAINBot
    {
        public GClass475 GoalEnemy => BotOwner.Memory.GoalEnemy;
        public Player EnemyPlayer { get; private set; }
        public EnemyComponent EnemyComponent { get; private set; }

        private readonly float BotDifficultyModifier;

        public SAINEnemy(BotOwner bot, IAIDetails person, float BotDifficultyMod) : base(bot)
        {
            Person = person;
            EnemyPlayer = person.GetPlayer;
            BotDifficultyModifier = BotDifficultyMod;
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            if (SAIN.Squad.BotInGroup)
            {
                FindEnemyComponent();
            }
        }

        private void FindEnemyComponent()
        {
            var componentArray = EnemyPlayer.GetComponents<EnemyComponent>();
            int? count = componentArray?.Count();
            if (componentArray == null || count == 0)
            {
                AddNewComponent();
            }
            else
            {
                if (componentArray != null && count > 0)
                {
                    foreach (var component in componentArray)
                    {
                        if (component != null)
                        {
                            if (component.CheckIfComponentIsForGroup(SAIN))
                            {
                                EnemyComponent = component;
                                component.AddOwner(SAIN);
                                break;
                            }
                        }
                    }
                }
            }
            if (EnemyComponent == null)
            {
                AddNewComponent();
            }
        }

        private void AddNewComponent()
        {
            Logger.LogDebug($"New Enemy Component added for enemy: [{EnemyPlayer.name}] for SquadID: [{SAIN.SquadId}]");
            EnemyComponent = EnemyPlayer.gameObject.AddComponent<EnemyComponent>();
            EnemyComponent.AddOwner(SAIN);
        }

        private readonly ManualLogSource Logger;

        public void Update()
        {
            UpdateDistance();
            UpdatePath();
        }

        private void UpdateDistance()
        {
            if (DistanceTimer < Time.time)
            {
                DistanceTimer = Time.time + 0.25f;
                float distance = GetMagnitudeToBot(Position);
                RealDistance = distance;
                LastSeenDistance = IsVisible ? distance : GetMagnitudeToBot(PositionLastSeen);
                DistanceFromLastSeen = IsVisible ? 0f : (PositionLastSeen - Position).magnitude;
            }
        }

        public float DistanceFromLastSeen { get; private set; }
        public Vector3 Position => Person.Position;

        private float GetMagnitudeToBot(Vector3 point)
        {
            return (BotOwner.Position - point).magnitude;
        }

        private bool _enemyIsReloading;
        private float _soundResetTimer;

        public bool EnemyIsReloading
        {
            get
            {
                if (_soundResetTimer < Time.time)
                {
                    _enemyIsReloading = false;
                }
                return _enemyIsReloading;
            }
            set
            {
                if (value == true)
                {
                    _enemyIsReloading = true;
                    _soundResetTimer = Time.time + 3f;
                }
            }
        }

        private bool _enemyHasGrenade;
        private float _grenadeResetTimer;

        public bool EnemyHasGrenadeOut
        {
            get
            {
                if (_grenadeResetTimer < Time.time)
                {
                    _enemyHasGrenade = false;
                }
                return _enemyHasGrenade;
            }
            set
            {
                if (value == true)
                {
                    _enemyHasGrenade = true;
                    _grenadeResetTimer = Time.time + 3f;
                }
            }
        }

        private bool _enemyIsHeal;
        private float _healResetTimer;

        public bool EnemyIsHealing
        {
            get
            {
                if (_healResetTimer < Time.time)
                {
                    _enemyIsHeal = false;
                }
                return _enemyIsHeal;
            }
            set
            {
                if (value == true)
                {
                    _enemyIsHeal = true;
                    _healResetTimer = Time.time + 4f;
                }
            }
        }


        private float SoundResetTimer = 0f;

        public float RealDistance { get; private set; }
        public float LastSeenDistance { get; private set; }
        public Vector3 PositionLastSeen { get; private set; }
        public float TimeSinceSeen { get; private set; }
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public bool CanHearCloseVisible { get; private set; }
        public bool EnemyClose { get; private set; }
        
        private float DistanceTimer = 0f;

        public void OnGainSight(float percentage)
        {
            if (percentage > 50f)
            {
                UpdateVisible(true);
                return;
            }
            if (IsVisible)
            {
                UpdateVisible(true);
            }
            else
            {
                bool random = EFTMath.RandomBool(25);
                UpdateVisible(random);
            }
        }

        public void OnLoseSight()
        {
            UpdateVisible(false);
        }

        public void UpdateCanShoot(bool value, float percentage)
        {
            PercentageEnemyCanShoot = percentage;
            CanShoot = value;
        }

        public float PercentageEnemyCanShoot { get; private set; }

        private void UpdateVisible(bool inLineOfSight)
        {
            CanShoot = inLineOfSight;
            InLineOfSight = inLineOfSight;

            bool visible = GoalEnemy != null
                && BotOwner.LookSensor.IsPointInVisibleSector(GoalEnemy.CurrPosition)
                && GoalEnemy.IsVisible == true
                && inLineOfSight;

            bool wasVisible = IsVisible;
            IsVisible = visible;

            float realDistance = RealDistance;
            bool close = realDistance < 15f;

            var move = Person.GetPlayer.MovementContext;
            bool enemySprinting = move.IsSprintEnabled;
            float enemyMoveSpeed = move.ClampedSpeed / move.MaxSpeed;

            bool canHear = (enemySprinting && realDistance < 35f) || close && enemyMoveSpeed > 0.25f;

            if (IsVisible)
            {
                TimeSinceSeen = 0f;
                if (!Seen)
                {
                    TimeFirstSeen = Time.time;
                    Seen = true;
                }
            }
            if (!IsVisible)
            {
                if (wasVisible)
                {
                    TimeLastSeen = Time.time;
                    PositionLastSeen = Person.Position;
                }
                if (Seen)
                {
                    TimeSinceSeen = Time.time - TimeLastSeen;
                }
            }

            EnemyClose = close;
            CanHearCloseVisible = canHear;
            EnemyComponent?.UpdateVisible(SAIN.ProfileId, IsVisible, EnemyClose, CanHearCloseVisible);
        }

        public void UpdatePath()
        {
            if (CheckPathTimer < Time.time)
            {
                CheckPathTimer = Time.time + 0.25f;

                CalcPath(Person.Position);
            }
        }

        private void CalcPath(Vector3 pos)
        {
            Path = new NavMeshPath();
            NavMesh.CalculatePath(BotOwner.Transform.position, pos, -1, Path);
            PathDistance = Path.CalculatePathLength();
        }

        public SAINEnemyPath CheckPathDistance()
        {
            const float VeryCloseDist = 5f;
            const float CloseDist = 20f;
            const float FarDist = 80f;
            const float VeryFarDist = 120f;

            SAINEnemyPath pathDistance;
            float distance = PathDistance;

            if (distance <= VeryCloseDist)
            {
                pathDistance = SAINEnemyPath.VeryClose;
            }
            else if (distance <= CloseDist)
            {
                pathDistance = SAINEnemyPath.Close;
            }
            else if (distance <= FarDist)
            {
                pathDistance = SAINEnemyPath.Mid;
            }
            else if (distance <= VeryFarDist)
            {
                pathDistance = SAINEnemyPath.Far;
            }
            else
            {
                pathDistance = SAINEnemyPath.VeryFar;
            }

            return pathDistance;
        }

        public NavMeshPath Path = new NavMeshPath();
        public float PathDistance { get; private set; }

        private float CheckPathTimer = 0f;

        public bool EnemyLookingAtMe
        {
            get
            {
                Vector3 EnemyLookDirection = VectorHelpers.NormalizeFastSelf(BotOwner.LookSensor._headPoint - Person.Transform.position);
                return VectorHelpers.IsAngLessNormalized(EnemyLookDirection, Person.LookDirection, 0.9659258f);
            }
        }

        public Vector3 EnemyHeadPosition => Person.MainParts[BodyPartType.head].Position;

        public Vector3 EnemyChestPosition => Person.MainParts[BodyPartType.body].Position;

        public IAIDetails Person { get; private set; }

        public bool InLineOfSight { get; private set; }

        public bool IsVisible { get; private set; }

        public bool CanShoot { get; private set; }
    }

    public class GroupMemberComponent : MonoBehaviour
    {
        public SAINComponent SAIN { get; private set; }
        public Dictionary<BotOwner, SAINComponent> Members => SAIN.Squad.SquadMembers;
        public Dictionary<BotOwner, SAINComponent> VisibleMembers { get; private set; } = new Dictionary<BotOwner, SAINComponent>();

        public void Init(SAINComponent sain)
        {
            SAIN = sain;
        }

        private void Awake()
        {

        }

        private void Update()
        {
            if (!SAIN.Squad.BotInGroup || SAIN.GameIsEnding || SAIN.IsDead)
            {
                Dispose();
                return;
            }
            if (SAIN.BotActive)
            {
                if (CheckVisTimer < Time.time)
                {
                    CheckVisTimer = Time.time + 1f; 
                    UpdateVisibleMembers();
                }
            }
        }

        private float CheckVisTimer = 0f;

        private void UpdateVisibleMembers()
        {
            var memberDict = new Dictionary<BotOwner, SAINComponent>(Members);
            var visibleMembers = new Dictionary<BotOwner, SAINComponent>();
            foreach (var member in memberDict)
            {
                if (member.Value == null || member.Value.IsDead) continue;
                if (VisibleByMember(member.Value))
                {
                    if (!VisibleMembers.ContainsKey(member.Key))
                    {
                        visibleMembers.Add(member.Key, member.Value);
                    }
                }
                else if (VisibleMembers.ContainsKey(member.Key))
                {
                    visibleMembers.Remove(member.Key);
                }
            }
            VisibleMembers = visibleMembers;
        }

        public bool VisibleByMember(SAINComponent member)
        {
            Vector3 start = SAIN.HeadPosition;
            Vector3 end = member.HeadPosition;
            Vector3 direction = end - start;
            return !Physics.Raycast(start, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public void Dispose()
        {
            StopAllCoroutines();
            VisibleMembers?.Clear();
            Destroy(this);
        }
    }

    public class EnemyComponent : MonoBehaviour
    {
        public Dictionary<string, GroupOwner> Owners { get; private set; } = new Dictionary<string, GroupOwner>();
        public Player EnemyPlayer { get; private set; }
        public bool VisibleByGroup { get; private set; }
        public bool HeardByGroup { get; private set; }
        public bool CloseToGroupMember { get; private set; }
        public float EnemyPower { get; private set; }
        public ETagStatus EnemyHealthStatus { get; private set; }
        public string OwnerSquadId { get; private set; } = "None";

        public GroupOwner MyInfo(string botProfileID)
        {
            if (!Owners.ContainsKey(botProfileID))
            {
                System.Console.WriteLine($"{botProfileID} does not exist in owners list!");
                return null;
            }
            return Owners[botProfileID];
        }

        public bool CheckIfComponentIsForGroup(SAINComponent sainComponent)
        {
            foreach (var item in Owners)
            {
                if (item.Value == null) continue;
                if (item.Value.SAINComponent.ProfileId == sainComponent.ProfileId)
                {
                    return true;
                }
                if (item.Value.SAINComponent.SquadId == sainComponent.SquadId)
                {
                    return true;
                }
            }
            return false;
        }

        private void Awake()
        {
            EnemyPlayer = GetComponent<Player>();
            EnemyPower = EnemyPlayer.AIData?.PowerOfEquipment ?? 50f;
        }

        private void Update()
        {
            if (EnemyPlayer == null || EnemyPlayer.HealthController?.IsAlive == false || Owners.Count == 0)
            {
                Dispose();
                return;
            }
            if (UpdateTimer < Time.time)
            {
                UpdateTimer = Time.time + 0.5f; 
                ClearDead();
                CheckVisibleStatus();
            }
        }

        private void CheckVisibleStatus()
        {
            bool visible = false;
            bool canHear = false;
            bool close = false;

            foreach (var item in Owners)
            {
                if (close && canHear && visible) break;
                if (item.Value == null) continue;
                if (item.Value.BotOwner.Memory.GoalEnemy?.Person?.GetPlayer != EnemyPlayer) continue;

                if (!visible && item.Value.CanSeeEnemy)
                {
                    visible = true;
                }
                if (!canHear && item.Value.CanHearEnemy)
                {
                    canHear = true;
                }
                if (!close && item.Value.CloseToEnemy)
                {
                    close = true;
                }
            }

            VisibleByGroup = visible;
            if (visible)
            {
                EnemyHealthStatus = EnemyPlayer.HealthStatus;
            }
            HeardByGroup = canHear;
            CloseToGroupMember = close;
        }

        private void ClearDead()
        {
            var owners = new Dictionary<string, GroupOwner>(Owners);
            foreach (var item in Owners)
            {
                if (item.Value == null || item.Value.BotIsDead)
                {
                    owners.Remove(item.Key);
                }
            }
            Owners = owners;
        }

        private float UpdateTimer = 0f;

        public bool AddOwner(SAINComponent sainComponent)
        {
            string botID = sainComponent.BotOwner.ProfileId;
            if (!Owners.ContainsKey(botID))
            {
                if (OwnerSquadId == "None")
                {
                    OwnerSquadId = sainComponent.SquadId;
                }
                Owners.Add(botID, new GroupOwner(sainComponent));
                return true;
            }
            return false;
        }

        public bool UpdateVisible(string botProfileID, bool visible, bool close, bool canHear)
        {
            var owner = MyInfo(botProfileID);
            if (owner == null)
            {
                return false;
            }

            owner.CanSeeEnemy = visible;
            owner.CloseToEnemy = close;
            owner.CanHearEnemy = canHear;
            return true;
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Owners?.Clear();
            EnemyPlayer = null;
            Destroy(this);
        }
    }

    public class GroupOwner
    {
        public SAINComponent SAINComponent { get; private set; }
        public BotOwner BotOwner => SAINComponent.BotOwner;
        public bool CloseToEnemy { get; set; }
        public bool CanSeeEnemy { get; set; }
        public bool CanHearEnemy { get; set; }
        public bool BotIsDead => BotOwner.HealthController?.IsAlive == false;
        public GroupOwner(SAINComponent SAIN)
        {
            SAINComponent = SAIN;
        }
    }

    public class ReactionTimeChecker
    {
        public ReactionTimeChecker(float reactionTime)
        {
            ReactionTime = reactionTime;
            startTime = Time.time;
        }

        private readonly float ReactionTime;
        private readonly float startTime;

        public bool Update()
        {
            return Time.time - startTime >= ReactionTime;
        }
    }
}