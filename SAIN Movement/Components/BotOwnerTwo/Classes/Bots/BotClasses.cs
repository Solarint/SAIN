using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN
{
    public abstract class OwnerBot
    {
        public OwnerBot(BotOwner owner)
        {
            this.botOwner_0 = owner;
        }

        protected BotOwner botOwner_0;
    }

    public class ArtilleryDangerPlace : OwnerBot
    {
        public ArtilleryDangerPlace(BotOwner owner) : base(owner)
        {
        }

        public void Activate()
        {
            GClass629 instance = Singleton<GClass629>.Instance;
            if (instance != null)
            {
                instance.OnArtilleryStart += this.method_0;
            }
        }

        public bool ShallEnd()
        {
            return this.EndTime < Time.time;
        }

        public bool ShallRunAway()
        {
            if (this.bool_0)
            {
                if (!this.ShallEnd())
                {
                    return (this.botOwner_0.Position - this.Point).sqrMagnitude < this.sRadius;
                }
                this.bool_0 = false;
            }
            return false;
        }

        private void method_0(Vector3 point, float radius, float duration)
        {
            if (Time.time - this.float_1 < 1f && (this.Point - point).sqrMagnitude < 1f)
            {
                return;
            }
            if ((point - this.botOwner_0.Position).sqrMagnitude > 100f)
            {
                return;
            }
            this.list_0.Clear();
            this.bool_0 = true;
            this.Point = point;
            this.Radius = radius;
            this.sRadius = radius * radius;
            this.float_1 = Time.time;
            this.EndTime = Time.time + duration;
            CustomNavigationPoint[] allPoints = this.botOwner_0.Covers.AllPoints;
            int num = 0;
            foreach (CustomNavigationPoint customNavigationPoint in allPoints)
            {
                if ((customNavigationPoint.Position - point).sqrMagnitude < this.sRadius)
                {
                    num++;
                    customNavigationPoint.Spotted(duration);
                }
                else
                {
                    this.list_0.Add(customNavigationPoint);
                }
            }
            if (num == allPoints.Length)
            {
                Debug.LogError("BAD! Arillery blocks all cover points. I don't know what to do =(");
            }
        }

        public void Dispose()
        {
            GClass629 instance = Singleton<GClass629>.Instance;
            if (instance != null)
            {
                instance.OnArtilleryStart -= this.method_0;
            }
        }

        private const float float_0 = 100f;

        public float EndTime;

        public Vector3 Point;

        public float Radius;

        public float sRadius;

        private bool bool_0;

        private readonly List<CustomNavigationPoint> list_0 = new List<CustomNavigationPoint>();

        private float float_1;
    }

    public class BotLookSensor
    {
        public readonly float VISIBLE_ANGLE = -0.3420201f;
        public readonly float VISIBLE_ANGLE_LIGHT = 0.5f;
        public readonly float VISIBLE_ANGLE_NIGHTVISION = 0.5f;
        public Vector3 _canShootStartPos;
        public Vector3 _headPoint;
        public float SeenCoef;
        private readonly AnimationCurve animationCurve_0;
        private readonly BifacialTransform bifacialTransform_0;
        private readonly BifacialTransform bifacialTransform_1;
        private readonly BotOwner botOwner_0;
        private readonly BotDifficultySettingsClass gclass561_0;
        private readonly List<IAIDetails> list_0 = new List<IAIDetails>(16);
        private readonly List<GClass475> list_1 = new List<GClass475>(16);
        private bool bool_0 = true;
        private bool bool_1;
        private bool bool_2;
        private bool bool_3;

        [CompilerGenerated]
        private bool bool_4;

        private float float_0;
        private float float_1 = 200f;
        private float float_2 = 30f;
        private float float_3;
        private float float_4 = 0.5f;
        private float float_5;
        private float float_6;

        [CompilerGenerated]
        private float float_7;

        [CompilerGenerated]
        private float float_8;

        [CompilerGenerated]
        private float float_9;

        private GStruct21 gstruct21_0;
        private int int_0;

        [CompilerGenerated]
        private int int_1;

        [CompilerGenerated]
        private LayerMask layerMask_0;

        private Vector3 vector3_0;

        public BotLookSensor(BotOwner botOwner)
        {
            this.botOwner_0 = botOwner;
            this.bifacialTransform_1 = botOwner.Transform;
            this.bifacialTransform_0 = this.botOwner_0.Fireport;
            this.gclass561_0 = botOwner.Settings;
            this.VISIBLE_ANGLE = Mathf.Cos(this.gclass561_0.FileSettings.Core.VisibleAngle * 0.017453292f);
            this.VISIBLE_ANGLE_LIGHT = Mathf.Cos(this.gclass561_0.FileSettings.Look.VISIBLE_ANG_LIGHT * 0.017453292f);
            this.VISIBLE_ANGLE_NIGHTVISION = Mathf.Cos(this.gclass561_0.FileSettings.Look.VISIBLE_ANG_NIGHTVISION * 0.017453292f);
            if (this.gclass561_0.FileSettings.Look.SELF_NIGHTVISION)
            {
                this.animationCurve_0 = this.botOwner_0.Settings.Curv.NightVisionSettings;
                return;
            }
            this.animationCurve_0 = this.botOwner_0.Settings.Curv.StandartVisionSettings;
        }

        public float ClearVisibleDist { get; private set; }
        public bool CurLookThroughGrass { get; private set; }
        public int HourServer { get; set; }
        public LayerMask Mask { get; private set; }

        public float MaxShootDist { get; private set; } = 200f;

        public float PreferedShootDist
        {
            get
            {
                if (this.float_2 < 1f)
                {
                    return 50f;
                }
                return this.float_2;
            }
        }

        public float VisibleDist { get; private set; }

        private AiTaskManagerClass GClass548_0
        {
            get
            {
                return this.botOwner_0.BotsGroup.BotGame.BotsController.AiTaskManager;
            }
        }

        public void Activate()
        {
            this.method_2();
            this.float_4 = this.botOwner_0.Settings.FileSettings.Look.POSIBLE_VISION_SPACE * 0.75f;
            this.bool_1 = this.botOwner_0.Profile.Info.Settings.IsBossOrFollower();
        }

        public bool CheckLookSimple(Player from, Player to)
        {
            BodyPartClass bodyPartClass = from.MainParts[BodyPartType.head];
            Vector3 direction = to.MainParts[BodyPartType.head].Position - bodyPartClass.Position;
            float magnitude = direction.magnitude;
            RaycastHit raycastHit;
            return !Physics.Raycast(new Ray(bodyPartClass.Position, direction), out raycastHit, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public void Dispose()
        {
            this.botOwner_0.GetPlayer.BeingHitAction -= this.method_0;
            this.list_0.Clear();
            this.list_1.Clear();
        }

        public void DrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(this.vector3_0, 0.3f);
            Gizmos.DrawWireSphere(this.vector3_0, 0.26f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(this._canShootStartPos, 0.2f);
        }

        public bool EnoughDistToShoot(out bool canRunNoAmmo)
        {
            if (this.botOwner_0.Memory.GoalEnemy == null)
            {
                canRunNoAmmo = false;
                return false;
            }
            float sqrMagnitude = (this.botOwner_0.Memory.GoalEnemy.CurrPosition - this.bifacialTransform_1.position).sqrMagnitude;
            canRunNoAmmo = (sqrMagnitude > this.botOwner_0.Settings.FileSettings.Shoot.RUN_DIST_NO_AMMO_SQRT);
            return sqrMagnitude < this.float_1;
        }

        public void Init()
        {
            this.bool_2 = this.botOwner_0.Settings.FileSettings.Look.LOOK_THROUGH_GRASS;
            if (!this.bool_2 && this.botOwner_0.Settings.FileSettings.Look.LOOK_THROUGH_PERIOD_BY_HIT > 0f)
            {
                this.botOwner_0.GetPlayer.BeingHitAction += this.method_0;
            }
            this.method_1(this.bool_2);
            Weapon currentWeapon = this.botOwner_0.WeaponManager.CurrentWeapon;
            int bEffDist = currentWeapon.Template.bEffDist;
            this.MaxShootDist = (float)bEffDist * this.gclass561_0.FileSettings.Shoot.MAX_DIST_COEF;
            if (currentWeapon is GClass2355)
            {
                this.float_2 = this.gclass561_0.FileSettings.Core.PistolFireDistancePref;
            }
            else if (currentWeapon is GClass2357)
            {
                this.float_2 = this.gclass561_0.FileSettings.Core.ShotgunFireDistancePref;
            }
            else
            {
                this.float_2 = this.gclass561_0.FileSettings.Core.RifleFireDistancePref;
            }
            if (this.MaxShootDist < 30f)
            {
                Debug.LogError("weapon.Template.bEffDist < 30. Name:" + currentWeapon.Template.Name);
                this.MaxShootDist = 30f;
            }
            if (this.float_2 <= 20f)
            {
                this.float_2 = 20f;
                string str = (currentWeapon == null) ? "weapon null" : ("TemplateId" + currentWeapon.TemplateId);
                Debug.LogError("Bot shoot Preferred DIST is LESS  <= 20 meters!!!!!!!  Watafak!!!!! + _preferedFireDist <= 1f weapon:" + str);
            }
            this.MaxShootDist *= UnityEngine.Random.Range(0.9f, 1.1f);
            this.float_2 = GClass783.GreateRandom(this.float_2);
            this.float_1 = this.MaxShootDist * this.MaxShootDist;
            this.float_1 = this.MaxShootDist * this.MaxShootDist;
        }

        public bool IsPointInVisibleSector(Vector3 position)
        {
            Vector3 v = position - this.botOwner_0.Position;
            float cos;
            if (this.botOwner_0.NightVision.UsingNow)
            {
                cos = this.VISIBLE_ANGLE_NIGHTVISION;
            }
            else
            {
                cos = (this.botOwner_0.BotLight.IsEnable ? this.VISIBLE_ANGLE_LIGHT : this.VISIBLE_ANGLE);
            }
            return GClass782.IsAngLessNormalized(this.botOwner_0.LookDirection, GClass782.NormalizeFastSelf(v), cos);
        }

        public void UpdateGroupsValue(BotGroupClass botsGroup)
        {
            this.float_2 *= botsGroup.BotZone.DistanceCoef;
        }

        public void UpdateLook()
        {
            if (this.bool_3 && this.float_6 < Time.time)
            {
                this.bool_3 = false;
                this.method_1(false);
            }
            if (this.bool_0)
            {
                this.bool_0 = false;
                GStruct21 nTask = new GStruct21
                {
                    Task = new Func<Task>(this.method_6)
                };
                this.GClass548_0.AddTask(nTask);
                this.gstruct21_0 = nTask;
                this.float_5 = Time.time;
                return;
            }
            if (Time.time - this.float_5 > this.float_4)
            {
                this.GClass548_0.TaskWaitTooLong();
                this.gstruct21_0.IsExecuted = true;
                this.gstruct21_0.Task().HandleExceptions();
            }
        }

        public void UpdateZoneValue(BotZone botZone)
        {
            this.float_2 *= botZone.DistanceCoef;
        }

        private void method_0(DamageInfo arg1, EBodyPart arg2, float arg3)
        {
            if (this.bool_2)
            {
                return;
            }
            this.method_1(true);
            this.bool_3 = true;
            this.float_6 = Time.time + this.botOwner_0.Settings.FileSettings.Look.LOOK_THROUGH_PERIOD_BY_HIT;
        }

        private void method_1(bool val)
        {
            this.CurLookThroughGrass = val;
            if (this.CurLookThroughGrass)
            {
                this.Mask = LayerMaskClass.HighPolyWithTerrainMask;
                return;
            }
            this.Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
        }

        private void method_2()
        {
            if (this.float_3 < Time.time)
            {
                float currentVisibleDistance = this.botOwner_0.Settings.Current.CurrentVisibleDistance;
                float num = 1f;
                if (this.botOwner_0.GameDateTime != null && this.animationCurve_0 != null)
                {
                    DateTime dateTime = this.botOwner_0.GameDateTime.Calculate();
                    num = this.animationCurve_0.Evaluate((float)dateTime.Hour);
                    this.HourServer = (int)((short)dateTime.Hour);
                }
                this.ClearVisibleDist = currentVisibleDistance * num;
                this.VisibleDist = this.botOwner_0.NightVision.UpdateVision(this.ClearVisibleDist);
                this.VisibleDist = this.botOwner_0.BotLight.UpdateLightEnable(this.VisibleDist);
                this.float_3 = Time.time + (float)(this.botOwner_0.FlashGrenade.IsFlashed ? 3 : 60);
            }
        }

        private async Task method_3()
        {
            if (this.float_0 > 0f)
            {
                this.float_0 -= Time.deltaTime;
            }
            else
            {
                await Task.Yield();
                this.method_2();
                this.int_0++;
                this.float_0 = 0.1f;
                if (!(this.botOwner_0 == null) && !this.botOwner_0.IsDead && this.botOwner_0.LeaveData != null && !this.botOwner_0.LeaveData.LeaveComplete)
                {
                    this.vector3_0 = this.bifacialTransform_0.position;
                    Vector3 a = GClass782.NormalizeFast(this.botOwner_0.LookDirection);
                    this._canShootStartPos = this.vector3_0 - a * this.botOwner_0.Settings.FileSettings.Aiming.WEAPON_ROOT_OFFSET;
                    this._headPoint = this.botOwner_0.MyHead.position;
                    GClass552 gclass = new GClass552();
                    if (!this.method_4(gclass))
                    {
                        if (gclass.distCheck && !this.bool_1 && !this.botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Protect))
                        {
                            if (gclass.minDistance > this.botOwner_0.WeaponManager.AmbushDistance)
                            {
                                this.botOwner_0.Tactic.SetTactic(BotGroupClass.BotCurrentTactic.Ambush, false, -1f);
                            }
                            else
                            {
                                this.botOwner_0.Tactic.SetTactic(BotGroupClass.BotCurrentTactic.Attack, false, -1f);
                            }
                        }
                        for (int i = 0; i < gclass.reportsData.Count; i++)
                        {
                            GClass522 gclass2 = gclass.reportsData[i];
                            this.botOwner_0.BotsGroup.ReportAboutEnemy(gclass2.enemy, gclass2.VisibleOnlyBuSence);
                        }
                        if (gclass.reportsData.Count > 0)
                        {
                            this.botOwner_0.Memory.SetLastTimeSeeEnemy();
                        }
                        if (gclass.shallRecalcGoal)
                        {
                            this.botOwner_0.CalcGoal();
                        }
                        if (this.int_0 > 20)
                        {
                            this.int_0 = 0;
                        }
                    }
                }
            }
        }

        private bool method_4(GClass552 lookAll)
        {
            Dictionary<IAIDetails, GClass475> enemyInfos = this.botOwner_0.EnemiesController.EnemyInfos;
            this.list_0.Clear();
            this.list_1.Clear();
            foreach (KeyValuePair<IAIDetails, GClass475> keyValuePair in enemyInfos)
            {
                this.list_0.Add(keyValuePair.Key);
                this.list_1.Add(keyValuePair.Value);
            }
            for (int i = 0; i < this.list_0.Count; i++)
            {
                IAIDetails iaidetails = this.list_0[i];
                GClass475 gclass = this.list_1[i];
                try
                {
                    this.method_5(iaidetails, gclass, lookAll);
                }
                catch (Exception ex)
                {
                    this.botOwner_0.BotsGroup.BotGame.BotsController.DebugLogsAboultRemoveEnemies();
                    this.botOwner_0.EnemiesController.EnemyInfos.Remove(iaidetails);
                    bool flag = iaidetails.HealthController != null && iaidetails.HealthController.IsAlive;
                    Debug.LogError(string.Format(" enemyInfo.ProfileId:{0} byBotId:{1}   botProfileId:{2}  isAlive:{3} error:{4}", new object[]
                    {
                    gclass.ProfileId,
                    this.botOwner_0.Id,
                    this.botOwner_0.Profile.Id,
                    flag,
                    ex
                    }));
                    return true;
                }
            }
            return false;
        }

        private void method_5(IAIDetails person, GClass475 info, GClass552 lookAll)
        {
            info.CheckLookEnemy(person, lookAll);
        }

        [CompilerGenerated]
        private async Task method_6()
        {
            await this.method_3();
            this.bool_0 = true;
        }
    }

    public class Ambush : OwnerBot
    {
        public Ambush(BotOwner owner) : base(owner)
        {
        }

        public bool TryGetAmbushPoint(out CustomNavigationPoint ambushPoint)
        {
            if (this.customNavigationPoint_0 != null && this.customNavigationPoint_0.IsSpotted)
            {
                this.customNavigationPoint_0 = null;
                ambushPoint = null;
                return false;
            }
            if (this.customNavigationPoint_0 != null)
            {
                ambushPoint = this.customNavigationPoint_0;
                return true;
            }
            ambushPoint = null;
            return false;
        }

        public void SetCoverAtMiddle(CustomNavigationPoint coverInMiddle)
        {
            this.customNavigationPoint_0 = coverInMiddle;
        }

        public GClass261 GetCoverSearchData()
        {
            Vector3 position = this.botOwner_0.Transform.position;
            Vector3 b = this.botOwner_0.Position;
            if (this.botOwner_0.Memory.GoalEnemy != null)
            {
                b = this.botOwner_0.Memory.GoalEnemy.CurrPosition;
            }
            else if (this.botOwner_0.Memory.GoalTarget.GoalTarget != null)
            {
                b = this.botOwner_0.Memory.GoalTarget.GoalTarget.Position;
            }
            Vector3 centerPos = (this.botOwner_0.Position + b) / 2f;
            return new GClass261(centerPos, this.botOwner_0, CoverShootType.shoot, 4f, 0f, CoverSearchType.distToToCenter, null, null, new Vector3?(position), ECheckSHootHide.shootAndHide, false, false, new GClass263(this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL, 999), PointsArrayType.byShootType, true, null, null, "Default")
            {
                MinSDistToCarePos = 0f,
                CenterPos = centerPos,
                searchLabel = "Ambush",
                shootType = CoverShootType.hide,
                CheckShootHide = ECheckSHootHide.shootAndHide,
                RecalcIfBadPower = false,
                MoreDistIfCantFind = false,
                coverSearchDefenceData =
            {
                minDefenceLevel = this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL,
                maxDangerLevel = 1
            }
            };
        }

        public bool HaveCover()
        {
            return this.customNavigationPoint_0 != null;
        }

        private const int int_0 = 1;

        private CustomNavigationPoint customNavigationPoint_0;
    }

    public class AssaultBuildingData : OwnerBot
    {
        public AssaultBuildingData(BotOwner owner) : base(owner)
        {
        }

        public bool IsActive
        {
            get
            {
                return this.bool_0;
            }
        }

        public void StartFor(float period)
        {
            if (this.bool_0 && this.ginterface8_0 != null && this.ginterface8_0.IsActive)
            {
                this.ginterface8_0.Stop(false);
            }
            this.bool_0 = true;
            this.ginterface8_0 = this.botOwner_0.DelayActions.Add(period, new Action(this.method_1));
        }

        public void PeriodCheckStart()
        {
            if (this.bool_0)
            {
                return;
            }
            if (this.float_0 > Time.time)
            {
                return;
            }
            this.float_0 = Time.time + 3f;
            GClass475 goalEnemy = this.botOwner_0.Memory.GoalEnemy;
            if (goalEnemy == null)
            {
                return;
            }
            if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 3f)
            {
                return;
            }
            int num = 0;
            float num2 = float.MinValue;
            for (int i = 0; i < this.botOwner_0.BotsGroup.MembersCount; i++)
            {
                BotOwner botOwner = this.botOwner_0.BotsGroup.Member(i);
                if (botOwner.Memory.IsInCover)
                {
                    num++;
                }
                for (int j = i; j < this.botOwner_0.BotsGroup.MembersCount; j++)
                {
                    float sqrMagnitude = (this.botOwner_0.BotsGroup.Member(j).Position - botOwner.Position).sqrMagnitude;
                    if (sqrMagnitude > num2)
                    {
                        num2 = sqrMagnitude;
                    }
                }
            }
            if (num < this.botOwner_0.BotsGroup.MembersCount - 1)
            {
                return;
            }
            if (num2 < 625f)
            {
                this.method_0();
            }
        }

        private void method_0()
        {
            for (int i = 0; i < this.botOwner_0.BotsGroup.MembersCount; i++)
            {
                this.botOwner_0.BotsGroup.Member(i).AssaultBuildingData.StartFor(60f);
            }
        }

        public void Stop()
        {
            if (this.ginterface8_0 != null && this.ginterface8_0.IsActive)
            {
                this.ginterface8_0.Stop(false);
            }
            this.bool_0 = false;
        }

        private void method_1()
        {
            this.bool_0 = false;
        }

        public void Dispose()
        {
        }

        private bool bool_0;

        private GClass583.GInterface8 ginterface8_0;

        private float float_0;

        private const float float_1 = 60f;

        private const float float_2 = 625f;
    }

    public class AssaultDangerArea : GClass294
    {
        public GClass512 AssaultGroup { get; private set; }

        public bool HaveTarget
        {
            get
            {
                return this.AssaultGroup != null;
            }
        }

        public EBotAssaultAreaStatus ShallAssault
        {
            get
            {
                return this.AssaultGroup.Status;
            }
        }

        public AssaultDangerArea(BotOwner owner) : base(owner)
        {
        }

        public void AreaCompleted()
        {
            this.AssaultGroup = null;
        }

        public void FindCover()
        {
            Vector3 a = GClass782.NormalizeFastSelf(this.AssaultGroup.AreaToAssaut.Point - this.botOwner_0.Position);
            Vector3 pos = this.AssaultGroup.AreaToAssaut.Point - a * 20f;
            CustomNavigationPoint customNavigationPoint = this.botOwner_0.Covers.FindClosestPoint(pos, false);
            this.customNavigationPoint_0 = customNavigationPoint;
        }

        public BotLogicDecision ManualUpdate()
        {
            EBotAssaultAreaStatus shallAssault = this.ShallAssault;
            if (shallAssault != EBotAssaultAreaStatus.shallGoClose)
            {
                if (shallAssault != EBotAssaultAreaStatus.assault)
                {
                    return BotLogicDecision.holdPosition;
                }
                this.botOwner_0.AssaultDangerArea.AssaultGroup.BotStartAssault(this.botOwner_0);
                GClass475 goalEnemy = this.botOwner_0.Memory.GoalEnemy;
                if (goalEnemy == null)
                {
                    CustomNavigationPoint customNavigationPoint = this.botOwner_0.Covers.FindClosestPoint(this.AssaultGroup.AreaToAssaut.Point, false);
                    this.botOwner_0.GoToSomePointData.SetPoint(customNavigationPoint.BasePosition);
                    return BotLogicDecision.goToPoint;
                }
                if (goalEnemy.CanShoot)
                {
                    return BotLogicDecision.shootFromPlace;
                }
                return BotLogicDecision.runToEnemy;
            }
            else
            {
                if (!this.botOwner_0.Memory.IsInCover)
                {
                    if (this.customNavigationPoint_0 == null)
                    {
                        this.FindCover();
                    }
                    return BotLogicDecision.goToCoverPoint;
                }
                this.botOwner_0.StopMove();
                if (this.botOwner_0.Memory.CurCustomCoverPoint == this.customNavigationPoint_0)
                {
                    this.AssaultGroup.BotIsReady(this.botOwner_0);
                    return BotLogicDecision.holdPosition;
                }
                if (this.customNavigationPoint_0 == null)
                {
                    this.FindCover();
                }
                return BotLogicDecision.goToCoverPoint;
            }
        }

        public bool CanDoAssault()
        {
            GClass475 goalEnemy = this.botOwner_0.Memory.GoalEnemy;
            return goalEnemy == null || Time.time - goalEnemy.PersonalLastSeenTime >= 20f;
        }

        public void ComeToCoverComplete()
        {
        }

        public void Activate()
        {
            this.AssaultGroup = null;
        }

        public void SetActivaArea(GClass512 botZoneDangerArea)
        {
            this.AssaultGroup = botZoneDangerArea;
        }

        public bool ShallEndHoldPosition()
        {
            if (this.float_1 < Time.time)
            {
                this.float_1 = Time.time + 3f;
                return true;
            }
            return false;
        }

        public CustomNavigationPoint GetCloseCover()
        {
            if (this.customNavigationPoint_0 == null && this.float_2 < Time.time)
            {
                this.float_2 = Time.time + 3f;
                this.FindCover();
            }
            return this.customNavigationPoint_0;
        }

        private const float float_0 = 20f;

        private CustomNavigationPoint customNavigationPoint_0;

        private float float_1;

        private float float_2;

        [CompilerGenerated]
        private GClass512 gclass512_0;
    }

    public class BewareGrenade : GClass294
    {
        public BewareGrenade(BotOwner owner) : base(owner)
        {
        }

        public void TurnAwayFlash(Grenade grenade)
        {
            BewareGrenade.Class134 @class = new BewareGrenade.Class134();
            @class.grenade = grenade;
            this.botOwner_0.BotTurnAwayLight.Activate(new Func<Vector3?>(@class.method_0), this.botOwner_0.Settings.FileSettings.Grenade.WAIT_TIME_TURN_AWAY);
        }

        public void AddGrenadeDanger(Vector3 danger, Grenade grenade)
        {
            BewareGrenade.Class135 @class = new BewareGrenade.Class135();
            @class.grenade = grenade;
            @class.gclass321_0 = this;
            @class.danger = danger;
            @class.isMyFlash = false;
            if (@class.grenade.Player.Id == this.botOwner_0.GetPlayer.Id)
            {
                if (this.botOwner_0.Settings.FileSettings.Grenade.NO_RUN_FROM_AI_GRENADES)
                {
                    return;
                }
                @class.isMyFlash = (@class.grenade as StunGrenade != null);
                if (@class.isMyFlash)
                {
                    this.TurnAwayFlash(@class.grenade);
                }
            }
            float magnitude = (this.botOwner_0.Position - @class.danger).magnitude;
            if (magnitude < this.botOwner_0.Settings.FileSettings.Grenade.ADD_GRENADE_AS_DANGER)
            {
                this.botOwner_0.BotsGroup.AddPointToSearch(@class.danger, 140f, this.botOwner_0, !(@class.grenade is SmokeGrenade));
                if (magnitude < this.botOwner_0.Settings.FileSettings.Grenade.RUN_AWAY_SQR)
                {
                    this.method_1(@class.grenade, @class.danger, @class.isMyFlash);
                    return;
                }
                Debug.LogError(string.Format("danger still is too far TIMER ADDS:{0} pos:{1} dist:{2}", @class.danger, this.botOwner_0.Position, magnitude));
                StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(1.0), false).OnTimer += @class.method_0;
            }
        }

        public bool ShallRunAway()
        {
            if (this.GrenadeDangerPoint == null)
            {
                return false;
            }
            if (this.method_3())
            {
                return false;
            }
            if (this.GrenadeDangerPoint.ShallDestroy())
            {
                this.GrenadeDangerPoint = null;
                return false;
            }
            return this.GrenadeDangerPoint.ShallRunAway() && !this.method_0(this.GrenadeDangerPoint.Grenade) && this.GrenadeDangerPoint.IsActive();
        }

        public void DrawGizmosSelected()
        {
            if (this.GrenadeDangerPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(this.GrenadeDangerPoint.DangerPoint, GClass560.Core.DELTA_GRENADE_RUN_DIST);
            }
        }

        public void IgnoreGrenadesForSec(float sec)
        {
            this.float_0 = Time.time + sec;
        }

        public void UpdateByNode()
        {
            this.method_4();
            this.botOwner_0.BotRun.RunAwayGrenade(-1f);
        }

        private bool method_0(Grenade grenade)
        {
            return grenade is SmokeGrenade && this.botOwner_0.Settings.FileSettings.Grenade.IGNORE_SMOKE_GRENADE;
        }

        private void method_1(Grenade grenade, Vector3 danger, bool isMyFlash)
        {
            BewareGrenade.Class136 @class = new BewareGrenade.Class136();
            @class.gclass321_0 = this;
            @class.grenade = grenade;
            @class.danger = danger;
            if (GClass783.IsTrue100(this.botOwner_0.Settings.FileSettings.Grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100))
            {
                Action<Grenade> onBewareGrenade = this.OnBewareGrenade;
                if (onBewareGrenade != null)
                {
                    onBewareGrenade(@class.grenade);
                }
                bool flag = @class.grenade as SmokeGrenade != null;
                bool flag2 = @class.grenade as StunGrenade != null;
                if (!flag && !flag2)
                {
                    this.method_2(@class.danger);
                    GClass583.GInterface8 ginterface = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(1.399999976158142), false);
                    GClass583.GInterface8 ginterface2 = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(3.0), false);
                    ginterface.OnTimer += @class.method_0;
                    ginterface2.OnTimer += @class.method_0;
                }
                if (!isMyFlash)
                {
                    this.botOwner_0.BotTalk.TrySay(EPhraseTrigger.OnEnemyGrenade, true);
                    this.GrenadeDangerPoint = new GClass519(@class.danger, @class.grenade, this.botOwner_0, this.botOwner_0.Settings.FileSettings.Grenade.DELTA_GRENADE_START_TIME);
                }
            }
        }

        private void method_2(Vector3 pos)
        {
            foreach (CustomNavigationPoint customNavigationPoint in this.botOwner_0.BotsGroup.CoverPointMaster.GetClosePoints(pos, this.botOwner_0, this.botOwner_0.Settings.FileSettings.Cover.SPOTTED_GRENADE_RADIUS))
            {
                customNavigationPoint.Spotted(this.botOwner_0.Settings.FileSettings.Cover.SPOTTED_GRENADE_TIME);
            }
        }

        private bool method_3()
        {
            return Time.time < this.float_0;
        }

        private void method_4()
        {
            bool flag = false;
            if (this.botOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
            {
                CustomNavigationPoint covPoint = this.botOwner_0.Memory.BotCurrentCoverInfo.CovPoint;
                if (!(flag = ((this.GrenadeDangerPoint.DangerPoint - covPoint.Position).sqrMagnitude < GClass560.Core.DELTA_GRENADE_BEWARE_DIST_SQRT)))
                {
                    if (!this.botOwner_0.Mover.HasPathAndNoComplete || this.float_3 < Time.time)
                    {
                        this.float_3 = Time.time + 0.5f;
                        this.botOwner_0.GoToPoint(covPoint, "");
                    }
                    return;
                }
            }
            if (this.float_1 < Time.time || flag)
            {
                GClass261 data = new GClass261(this.botOwner_0.Position, this.botOwner_0, CoverShootType.hide, 2500f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.nothing, false, true, new GClass263(0f), PointsArrayType.allWithBush, false, null, null, "Default");
                this.float_1 = Time.time + this.float_2;
                CustomNavigationPoint coverPointMain = this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, false);
                this.botOwner_0.Memory.BotCurrentCoverInfo.SetCover(coverPointMain, false);
                this.botOwner_0.GoToPoint(coverPointMain, "");
            }
        }

        public GClass519 GrenadeDangerPoint;

        public Action<Grenade> OnBewareGrenade;

        private float float_0;

        private float float_1;

        private readonly float float_2 = 2f;

        private float float_3 = 2f;

        [CompilerGenerated]
        private sealed class Class134
        {
            internal Vector3? method_0()
            {
                if (this.grenade != null)
                {
                    return new Vector3?(this.grenade.transform.position);
                }
                return null;
            }

            public Grenade grenade;
        }

        [CompilerGenerated]
        private sealed class Class135
        {
            internal void method_0()
            {
                if (this.grenade != null)
                {
                    float magnitude = (this.gclass321_0.botOwner_0.Position - this.grenade.transform.position).magnitude;
                    if (magnitude < this.gclass321_0.botOwner_0.Settings.FileSettings.Grenade.RUN_AWAY)
                    {
                        this.gclass321_0.method_1(this.grenade, this.danger, this.isMyFlash);
                        return;
                    }
                    Debug.LogError(string.Format("grenade still is too far:  grenade:{0}  _owner.Position:{1}  distToGrenade:{2}", this.grenade.transform.position, this.gclass321_0.botOwner_0.Position, magnitude));
                }
            }

            public Grenade grenade;

            public BewareGrenade gclass321_0;

            public Vector3 danger;

            public bool isMyFlash;
        }

        [CompilerGenerated]
        private sealed class Class136
        {
            internal void method_0()
            {
                if (this.gclass321_0.botOwner_0.BotState == EBotState.Active && this.gclass321_0.botOwner_0.HealthController.IsAlive && this.grenade != null)
                {
                    this.gclass321_0.method_2(this.grenade.transform.position);
                    this.gclass321_0.GrenadeDangerPoint = new GClass519(this.danger, this.grenade, this.gclass321_0.botOwner_0, 0f);
                }
            }

            public BewareGrenade gclass321_0;

            public Grenade grenade;

            public Vector3 danger;
        }
    }

    public class Boss : GClass294, GInterface2
    {
        public static bool IsFollowerSuitableForBoss(WildSpawnType follower, WildSpawnType boss)
        {
            if (follower == boss)
            {
                return true;
            }
            switch (follower)
            {
                case WildSpawnType.followerBully:
                    if (boss == WildSpawnType.bossBully)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerKojaniy:
                    if (boss == WildSpawnType.bossKojaniy)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.pmcBot:
                    if (boss == WildSpawnType.pmcBot)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerGluharAssault:
                case WildSpawnType.followerGluharSecurity:
                case WildSpawnType.followerGluharScout:
                case WildSpawnType.followerGluharSnipe:
                    if (boss == WildSpawnType.bossGluhar)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerSanitar:
                    if (boss == WildSpawnType.bossSanitar)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.assaultGroup:
                    if (boss == WildSpawnType.assaultGroup)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.sectantWarrior:
                    if (boss == WildSpawnType.sectantPriest)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerTagilla:
                    if (boss == WildSpawnType.bossKilla)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.exUsec:
                    if (boss == WildSpawnType.exUsec)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerBigPipe:
                    if (boss == WildSpawnType.bossKnight)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerBirdEye:
                    if (boss == WildSpawnType.bossKnight)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.followerZryachiy:
                    if (boss == WildSpawnType.bossZryachiy)
                    {
                        return true;
                    }
                    break;

                case WildSpawnType.arenaFighterEvent:
                    if (boss == WildSpawnType.arenaFighterEvent)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }

        public event Action<BotOwner, List<BotOwner>> OnBossDead
        {
            [CompilerGenerated]
            add
            {
                Action<BotOwner, List<BotOwner>> action = this.action_0;
                Action<BotOwner, List<BotOwner>> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner, List<BotOwner>> value2 = (Action<BotOwner, List<BotOwner>>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner, List<BotOwner>>>(ref this.action_0, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<BotOwner, List<BotOwner>> action = this.action_0;
                Action<BotOwner, List<BotOwner>> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner, List<BotOwner>> value2 = (Action<BotOwner, List<BotOwner>>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner, List<BotOwner>>>(ref this.action_0, value2, action2);
                }
                while (action != action2);
            }
        }

        public event Action<BotOwner> OnBecomeBoss
        {
            [CompilerGenerated]
            add
            {
                Action<BotOwner> action = this.action_1;
                Action<BotOwner> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<BotOwner> action = this.action_1;
                Action<BotOwner> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner> value2 = (Action<BotOwner>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
        }

        public event Action<BotOwner, FollowerStatusChange> OnFollowerStatusChange
        {
            [CompilerGenerated]
            add
            {
                Action<BotOwner, FollowerStatusChange> action = this.action_2;
                Action<BotOwner, FollowerStatusChange> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner, FollowerStatusChange> value2 = (Action<BotOwner, FollowerStatusChange>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner, FollowerStatusChange>>(ref this.action_2, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<BotOwner, FollowerStatusChange> action = this.action_2;
                Action<BotOwner, FollowerStatusChange> action2;
                do
                {
                    action2 = action;
                    Action<BotOwner, FollowerStatusChange> value2 = (Action<BotOwner, FollowerStatusChange>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<BotOwner, FollowerStatusChange>>(ref this.action_2, value2, action2);
                }
                while (action != action2);
            }
        }

        public bool IamBoss { get; private set; }

        public bool AllowRequestSelf
        {
            get
            {
                return this.gclass315_0.warnMode == EWarnMode.SelfFromPlace;
            }
        }

        public GClass309 BossWarnData { get; private set; }

        public GClass295 BossLogic { get; private set; }

        public bool NeedProtection { get; private set; } = true;

        public GClass430 PatrollingData
        {
            get
            {
                return this.botOwner_0.PatrollingData;
            }
        }

        public bool IsAI
        {
            get
            {
                return true;
            }
        }

        public BotOwner Owner
        {
            get
            {
                return this.botOwner_0;
            }
        }

        public Vector3 PositionOrCover
        {
            get
            {
                if (this.botOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
                {
                    return this.botOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position;
                }
                return this.botOwner_0.Position;
            }
        }

        public Vector3 PositionIfInCover
        {
            get
            {
                if (this.botOwner_0.Memory.IsInCover)
                {
                    return this.botOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Position;
                }
                return this.botOwner_0.Position;
            }
        }

        public Vector3 Position
        {
            get
            {
                return this.botOwner_0.Position;
            }
        }

        public float MoveSpeed
        {
            get
            {
                return this.botOwner_0.Mover.DestMoveSpeed;
            }
        }

        public List<string> DebugInfoFollower
        {
            get
            {
                return this.gclass315_0.DebugInfo();
            }
        }

        public List<BotOwner> Followers
        {
            get
            {
                return this.gclass315_0.Followers;
            }
        }

        public int Id
        {
            get
            {
                return this.botOwner_0.Id;
            }
        }

        public int TargetFollowersCount
        {
            get
            {
                return this.gclass315_0.TargetFollowersCount;
            }
        }

        public Boss(BotOwner owner) : base(owner)
        {
            WildSpawnType role = this.botOwner_0.Profile.Info.Settings.Role;
            if (role == WildSpawnType.bossTest || role == WildSpawnType.bossGluhar)
            {
                this.gclass315_0 = new GClass316(this.botOwner_0);
                return;
            }
            if (role != WildSpawnType.bossTagilla)
            {
                this.gclass315_0 = new GClass315(this.botOwner_0);
                return;
            }
            this.gclass315_0 = new GClass317(this.botOwner_0);
        }

        public PatrolPoint GetPatrolPosByIndex(int botFollowerIndex)
        {
            if (this.PatrollingData.CurPatrolPoint != null)
            {
                return this.PatrollingData.CurPatrolPoint.TargetPoint.GetPosByIndex(botFollowerIndex);
            }
            return null;
        }

        public void ManualUpdate()
        {
            if (this.IamBoss)
            {
                this.gclass315_0.CheckFollowers();
                this.BossLogic.BossLogicUpdate();
            }
        }

        public IAIDetails Player()
        {
            return this.botOwner_0;
        }

        public GClass475 CurEnemy()
        {
            return this.botOwner_0.Memory.GoalEnemy;
        }

        public BotOwner GetFirstFollower(bool withGrenade)
        {
            for (int i = 0; i < this.gclass315_0.Followers.Count; i++)
            {
                BotOwner botOwner = this.gclass315_0.Followers[i];
                if (botOwner.HealthController.IsAlive && (!withGrenade || botOwner.WeaponManager.Grenades.HaveGrenade))
                {
                    return botOwner;
                }
            }
            return null;
        }

        public void SetBoss(int followersCount)
        {
            this.method_0();
            this.gclass315_0.SetTargetFollowersCount(followersCount);
            this.IamBoss = true;
            this.gclass315_0.UpdateFollowers();
            this.method_1();
            Action<BotOwner> action = this.action_1;
            if (action == null)
            {
                return;
            }
            action(this.botOwner_0);
        }

        public void SetWarnMode(EWarnMode mode)
        {
            this.gclass315_0.warnMode = mode;
        }

        public bool HaveFollowers()
        {
            return this.gclass315_0.Followers.Any<BotOwner>();
        }

        public void RemoveFollower(BotOwner botFollower)
        {
            this.gclass315_0.Remove(botFollower);
            Action<BotOwner, FollowerStatusChange> action = this.action_2;
            if (action == null)
            {
                return;
            }
            action(botFollower, FollowerStatusChange.Remove);
        }

        public bool IsMe(IAIDetails player)
        {
            return false;
        }

        public void DeletePlayer(Player getPlayer)
        {
            if (this.IamBoss)
            {
                this.BossWarnData.PlayerDead(getPlayer);
            }
        }

        public bool OfferSelf(BotOwner offer)
        {
            if (this.gclass315_0.Followers.Count >= this.gclass315_0.TargetFollowersCount)
            {
                return false;
            }
            if (!Boss.IsFollowerSuitableForBoss(offer.Profile.Info.Settings.Role, this.botOwner_0.Profile.Info.Settings.Role))
            {
                return false;
            }
            if (!offer.BotFollower.HaveBoss && !offer.Boss.IamBoss)
            {
                this.gclass315_0.AddFollower(offer);
                Action<BotOwner, FollowerStatusChange> action = this.action_2;
                if (action != null)
                {
                    action(offer, FollowerStatusChange.Add);
                }
                offer.BotFollower.SetToFollow(this, this.gclass315_0.Followers.Count - 1, false);
            }
            return true;
        }

        private void method_0()
        {
            switch (this.botOwner_0.Profile.Info.Settings.Role)
            {
                case WildSpawnType.bossTest:
                case WildSpawnType.gifter:
                    this.BossLogic = new GClass308(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.bossBully:
                    this.BossLogic = new GClass296(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.followerTest:
                case WildSpawnType.followerBully:
                case WildSpawnType.pmcBot:
                case WildSpawnType.assaultGroup:
                case WildSpawnType.exUsec:
                case WildSpawnType.arenaFighterEvent:
                    this.BossLogic = new GClass297(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.bossKilla:
                case WildSpawnType.cursedAssault:
                    this.BossLogic = new GClass299(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.bossKojaniy:
                case WildSpawnType.followerKojaniy:
                    this.BossLogic = new GClass305(this.botOwner_0, this);
                    this.NeedProtection = false;
                    goto IL_1C3;
                case WildSpawnType.bossGluhar:
                    this.BossLogic = new GClass298(this.botOwner_0, this);
                    this.NeedProtection = true;
                    goto IL_1C3;
                case WildSpawnType.bossSanitar:
                    this.BossLogic = new GClass303(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.sectantPriest:
                    this.BossLogic = new GClass306(this.botOwner_0, this);
                    this.NeedProtection = false;
                    goto IL_1C3;
                case WildSpawnType.bossTagilla:
                    this.BossLogic = new GClass300(this.botOwner_0, this);
                    goto IL_1C3;
                case WildSpawnType.bossKnight:
                    this.BossLogic = new GClass302(this.botOwner_0, this);
                    this.NeedProtection = false;
                    goto IL_1C3;
                case WildSpawnType.bossZryachiy:
                case WildSpawnType.followerZryachiy:
                    this.BossLogic = new GClass307(this.botOwner_0, this);
                    this.NeedProtection = false;
                    goto IL_1C3;
            }
            this.BossLogic = new GClass297(this.botOwner_0, this);
        IL_1C3:
            this.BossLogic.Activate();
            WildSpawnType role = this.botOwner_0.Profile.Info.Settings.Role;
            if (role == WildSpawnType.gifter)
            {
                this.BossWarnData = new GClass310(this.botOwner_0);
            }
            else
            {
                this.BossWarnData = new GClass309(this.botOwner_0);
            }
            this.botOwner_0.BotsGroup.BossAppear(this.botOwner_0);
        }

        private void method_1()
        {
            this.BossLogic.SetPatrolMode();
        }

        public void Dispose()
        {
            this.action_1 = null;
            this.action_2 = null;
            if (this.IamBoss)
            {
                this.BossLogic.Dispose();
                List<BotOwner> arg = this.gclass315_0.Followers.ToList<BotOwner>();
                BotOwner[] array = this.gclass315_0.Followers.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].BotFollower.Dispose();
                }
                Action<BotOwner, List<BotOwner>> action = this.action_0;
                if (action != null)
                {
                    action(this.botOwner_0, arg);
                }
                this.action_0 = null;
                this.gclass315_0.Dispose();
                this.gclass315_0.Clear();
            }
        }

        protected GClass315 gclass315_0;

        private GInterface3 ginterface3_0;

        [CompilerGenerated]
        private Action<BotOwner, List<BotOwner>> action_0;

        [CompilerGenerated]
        private Action<BotOwner> action_1;

        [CompilerGenerated]
        private Action<BotOwner, FollowerStatusChange> action_2;

        [CompilerGenerated]
        private bool bool_0;

        [CompilerGenerated]
        private GClass309 gclass309_0;

        [CompilerGenerated]
        private GClass295 gclass295_0;

        [CompilerGenerated]
        private bool bool_1;
    }

    public class BotAttackManager
    {
        public BotAttackManager(BotOwner owner)
        {
            this.botOwner_0 = owner;
        }

        public void TryPointGetting(bool withShoot, CoverSearchType searchType, ShootPointClass shoot2point, float deistStartSearchSqr, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true)
        {
            CoverShootType coverShootType = withShoot ? CoverShootType.shoot : CoverShootType.hide;
            this.TryPointGetting(this.botOwner_0.Transform.position, coverShootType, deistStartSearchSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent, false, true, null);
        }

        public void TryPointGetting(CoverShootType coverShootType, CoverSearchType searchType, ShootPointClass shoot2point, float deistStartSearchSqr, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true, bool canChange = true)
        {
            this.TryPointGetting(this.botOwner_0.Transform.position, coverShootType, deistStartSearchSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent, false, canChange, null);
        }

        public void TryPointGetting(Vector3 centerPos, bool withShoot, float distStart2SerachSqr, CoverSearchType searchType, ShootPointClass shoot2point, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true)
        {
            CoverShootType coverShootType = withShoot ? CoverShootType.shoot : CoverShootType.hide;
            this.TryPointGetting(centerPos, coverShootType, distStart2SerachSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent, false, true, null);
        }

        public void TryPointGetting(GClass261 searchData, Action<CustomNavigationPoint> OnFind, Action OnFail)
        {
            if (this.float_1 < Time.time || !this.botOwner_0.Memory.BotCurrentCoverInfo.HaveCover)
            {
                this.float_1 = Time.time + 1f;
                CustomNavigationPoint coverPointMain = this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(searchData, true);
                if (coverPointMain != null)
                {
                    if (OnFind != null)
                    {
                        OnFind(coverPointMain);
                        return;
                    }
                }
                else if (OnFail != null)
                {
                    OnFail();
                }
            }
        }

        public void TryPointGetting(Vector3 centerPos, CoverShootType coverShootType, float distStart2SerachSqr, CoverSearchType searchType, ShootPointClass shoot2point, Action<CustomNavigationPoint> OnFind, Action OnFail, bool checkCurrent = true, bool recalcNoTimer = false, bool useFightLogic = true, int? placeInfo = null)
        {
            if (this.float_1 < Time.time || !this.botOwner_0.Memory.BotCurrentCoverInfo.HaveCover || recalcNoTimer)
            {
                this.float_1 = Time.time + 1f;
                Vector3? closeFriendCover = this.botOwner_0.Covers.ClosestFriendCoverPoint();
                GClass261 data = new GClass261(centerPos, this.botOwner_0, coverShootType, distStart2SerachSqr, 0f, searchType, shoot2point, closeFriendCover, null, ECheckSHootHide.shootAndHide, checkCurrent, true, new GClass263(this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, useFightLogic, placeInfo, null, "Default");
                CustomNavigationPoint coverPointMain = this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, true);
                if (coverPointMain != null)
                {
                    if (OnFind != null)
                    {
                        OnFind(coverPointMain);
                        return;
                    }
                }
                else if (OnFail != null)
                {
                    OnFail();
                }
            }
        }

        public void PointAndPathDetecting()
        {
            this.botOwner_0.GoToPoint(this.botOwner_0.Memory.CurCustomCoverPoint, "");
        }

        public void UpdateNextTick()
        {
            this.float_1 = Time.time - 1f;
        }

        [CanBeNull]
        public CustomNavigationPoint PointToGo()
        {
            bool flag = this.botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Attack);
            bool flag2 = this.botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Protect);
            ShootPointClass shootPointClass = null;
            bool flag3 = flag || flag2;
            CoverShootType coverShootType = flag3 ? CoverShootType.shoot : CoverShootType.hide;
            if (flag3)
            {
                if (this.botOwner_0.Memory.GoalEnemy == null)
                {
                    coverShootType = CoverShootType.hide;
                }
                else if (Time.time - this.botOwner_0.BotsGroup.EnemyLastSeenTimeReal > this.botOwner_0.Settings.FileSettings.Mind.FIND_COVER_TO_GET_POSITION_WITH_SHOOT)
                {
                    coverShootType = CoverShootType.hide;
                }
            }
            if (coverShootType == CoverShootType.shoot)
            {
                shootPointClass = this.botOwner_0.CurrentEnemyTargetPosition(flag2);
                if (shootPointClass == null)
                {
                    coverShootType = CoverShootType.hide;
                }
            }
            CoverSearchType searchType = this.botOwner_0.Tactic.SubTactic.SearchTypeForCover(coverShootType);
            if (this.botOwner_0.Medecine.FirstAid.Have2Do)
            {
                searchType = CoverSearchType.distToBot;
            }
            Vector3? closeFriendCover = this.botOwner_0.Covers.ClosestFriendCoverPoint();
            float maxDistSqr = GClass560.Core.START_DIST_TO_COV;
            if (this.botOwner_0.Memory.GoalEnemy != null)
            {
                Vector3 position = this.botOwner_0.Transform.position;
                Vector3 enemyLastPosition = this.botOwner_0.Memory.GoalEnemy.EnemyLastPosition;
                GClass261 data = new GClass261((coverShootType != CoverShootType.hide) ? ((position + enemyLastPosition) / 2f) : position, this.botOwner_0, coverShootType, maxDistSqr, 0f, searchType, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, false, true, new GClass263(this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, true, null, null, "Default");
                return this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, true);
            }
            Vector3 position2 = this.botOwner_0.Position;
            if (this.botOwner_0.BotFollower.NeedToProtectBoss && this.botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Protect))
            {
                position2 = this.botOwner_0.BotFollower.BossToFollow.Position;
                float num = GClass560.Core.GOOD_DIST_TO_POINT * this.botOwner_0.Settings.FileSettings.Cover.GOOD_DIST_TO_POINT_COEF;
                maxDistSqr = num * num;
                if (coverShootType != CoverShootType.hide)
                {
                    searchType = CoverSearchType.shoot_toCover_toBot_Distances;
                }
            }
            GClass261 data2 = new GClass261(position2, this.botOwner_0, coverShootType, maxDistSqr, 0f, searchType, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, false, true, new GClass263(this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, true, null, null, "Default");
            return this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data2, true);
        }

        [CanBeNull]
        public CustomNavigationPoint CheckDestinations()
        {
            bool flag = this.botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Attack);
            CoverShootType shootType = CoverShootType.hide;
            ShootPointClass shootPointClass = this.botOwner_0.CurrentEnemyTargetPosition(true);
            if (flag)
            {
                shootType = ((shootPointClass != null) ? CoverShootType.shoot : CoverShootType.hide);
            }
            Vector3? closeFriendCover = this.botOwner_0.Covers.ClosestFriendCoverPoint();
            GClass261 data = new GClass261(this.botOwner_0.Position, this.botOwner_0, shootType, GClass560.Core.START_DIST_TO_COV, 0f, CoverSearchType.shoot_toCover_toBot_Distances, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, false, true, new GClass263(this.botOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, true, null, null, "Default");
            return this.botOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, true);
        }

        private const float float_0 = 1f;

        private readonly BotOwner botOwner_0;

        private float float_1;
    }

    public class BotFollowerClass : GClass294
    {
        public static BotFollowerClass Create(BotOwner bot)
        {
            WildSpawnType role = bot.Profile.Info.Settings.Role;
            if (role != WildSpawnType.followerTest && role - WildSpawnType.followerGluharAssault > 3)
            {
                return new BotFollowerClass(bot);
            }
            return new GClass313(bot);
        }

        public event Action<GInterface2> OnBossFinded
        {
            [CompilerGenerated]
            add
            {
                Action<GInterface2> action = this.action_0;
                Action<GInterface2> action2;
                do
                {
                    action2 = action;
                    Action<GInterface2> value2 = (Action<GInterface2>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<GInterface2>>(ref this.action_0, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<GInterface2> action = this.action_0;
                Action<GInterface2> action2;
                do
                {
                    action2 = action;
                    Action<GInterface2> value2 = (Action<GInterface2>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<GInterface2>>(ref this.action_0, value2, action2);
                }
                while (action != action2);
            }
        }

        public event Action<Player> OnBossDead
        {
            [CompilerGenerated]
            add
            {
                Action<Player> action = this.action_1;
                Action<Player> action2;
                do
                {
                    action2 = action;
                    Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<Player>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<Player> action = this.action_1;
                Action<Player> action2;
                do
                {
                    action2 = action;
                    Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<Player>>(ref this.action_1, value2, action2);
                }
                while (action != action2);
            }
        }

        public GInterface2 BossToFollow { get; protected set; }

        public GClass420 PatrolDataFollower
        {
            get
            {
                if (this.gclass420_0 == null)
                {
                    this.gclass420_0 = new GClass420(this.botOwner_0, this.Index);
                }
                return this.gclass420_0;
            }
            private set
            {
                this.gclass420_0 = value;
            }
        }

        public GClass314 BotFollowerFight
        {
            get
            {
                if (this.gclass314_0 == null)
                {
                    this.gclass314_0 = new GClass314(this.botOwner_0, this);
                }
                return this.gclass314_0;
            }
            private set
            {
                this.gclass314_0 = value;
            }
        }

        public bool HaveBoss
        {
            get
            {
                return this.BossToFollow != null;
            }
        }

        public int Index { get; protected set; }

        public virtual bool NeedToProtectBoss
        {
            get
            {
                return this.HaveBoss && this.BossToFollow.NeedProtection;
            }
        }

        public BotFollowerClass(BotOwner owner) : base(owner)
        {
        }

        private void method_0(float period)
        {
            this.PatrolDataFollower.StopFor(Mathf.Clamp(period, 1f, 60f));
        }

        public virtual void Activate()
        {
            this.BotFollowerFight = new GClass314(this.botOwner_0, this);
            this.PatrolDataFollower = new GClass420(this.botOwner_0, this.Index);
        }

        public void TryFindBoss()
        {
            if (this.botOwner_0.IsFollower())
            {
                this.method_3(null);
                this.method_2(null);
            }
        }

        public virtual void SetToFollow(GInterface2 boss, int index, bool changeLogicMode = false)
        {
            this.Index = index;
            if (this.BossToFollow == null || changeLogicMode)
            {
                this.BossToFollow = boss;
                this.PatrolDataFollower.Activate();
                this.PatrolDataFollower.SetIndex(this.Index);
                PatrolMode mode = PatrolMode.follower;
                PatrolMode mode2 = PatrolMode.simple;
                if (this.botOwner_0.Profile.Info.Settings.Role == WildSpawnType.followerBigPipe || this.botOwner_0.Profile.Info.Settings.Role == WildSpawnType.followerBirdEye)
                {
                    mode2 = PatrolMode.groupMoving;
                }
                GClass481 pointChooser = GClass430.GetPointChooser(this.botOwner_0, mode2, this.botOwner_0.SpawnProfileData);
                this.botOwner_0.PatrollingData.SetMode(mode, pointChooser);
                this.botOwner_0.Tactic.SetTactic(BotGroupClass.BotCurrentTactic.Protect, false, -1f);
                this.method_1();
                if (boss.IsAI)
                {
                    BotOwner botOwner = boss.Player().AIData.BotOwner;
                    if (changeLogicMode)
                    {
                        botOwner.PeacefulActions.OnStartPeacefulMove -= this.method_5;
                        botOwner.DeadBodyWork.OnStartLookToBody -= this.method_0;
                    }
                    botOwner.PeacefulActions.OnStartPeacefulMove += this.method_5;
                    botOwner.DeadBodyWork.OnStartLookToBody += this.method_0;
                }
            }
        }

        public void DrawGizmos()
        {
            if (this.HaveBoss)
            {
                Vector3 center = this.botOwner_0.Position + Vector3.up * 1.5f;
                Gizmos.color = new Color(1f, 0.64705884f, 0f);
                Gizmos.DrawWireSphere(center, 0.3f);
                if (this.PatrolDataFollower.HaveProblems)
                {
                    Gizmos.color = new Color(1f, 0.25490198f, 0.2f);
                }
                Gizmos.DrawWireSphere(center, 0.4f);
            }
        }

        public void DrawGizmosSelected()
        {
            if (this.PatrolDataFollower != null)
            {
                this.PatrolDataFollower.OnDrawGizmosSelected();
            }
        }

        public virtual void Update(BotOwner bot)
        {
        }

        protected void method_1()
        {
            Action<GInterface2> action = this.action_0;
            if (action != null)
            {
                action(this.BossToFollow);
            }
            this.BossToFollow.Player().GetPlayer.OnPlayerDead += this.method_4;
        }

        private void method_2(float? maxDist = null)
        {
            float num = float.MaxValue;
            if (maxDist != null)
            {
                num = maxDist.Value * maxDist.Value;
            }
            List<Player> allBossPlayers = this.botOwner_0.BotsGroup.BotGame.BotsController.GetAllBossPlayers();
            Player player = null;
            float num2 = float.MaxValue;
            for (int i = 0; i < allBossPlayers.Count; i++)
            {
                Player player2 = allBossPlayers[i];
                if (player2.HealthController.IsAlive)
                {
                    float sqrMagnitude = (player2.Position - this.botOwner_0.Position).sqrMagnitude;
                    if (sqrMagnitude < num2 && sqrMagnitude < num)
                    {
                        num2 = sqrMagnitude;
                        player = player2;
                    }
                }
            }
            if (player != null)
            {
                player.AIData.AIBossPlayer.OfferBot(this.botOwner_0);
            }
        }

        private void method_3(float? maxDist = null)
        {
            float num = float.MaxValue;
            if (maxDist != null)
            {
                num = maxDist.Value * maxDist.Value;
            }
            IEnumerable<BotOwner> botOwners = this.botOwner_0.BotsGroup.BotGame.BotsController.Bots.BotOwners;
            GClass311 gclass = null;
            float num2 = float.MaxValue;
            foreach (BotOwner botOwner in botOwners)
            {
                if (botOwner.BotState == EBotState.Active && botOwner.HealthController.IsAlive && botOwner.Boss.IamBoss && botOwner.Id != this.botOwner_0.Id && botOwner.Boss.Followers.Count < botOwner.Boss.TargetFollowersCount && !(botOwner.BotsGroup.BotZone != this.botOwner_0.BotsGroup.BotZone))
                {
                    float sqrMagnitude = (botOwner.Position - this.botOwner_0.Position).sqrMagnitude;
                    if (sqrMagnitude < num2 && sqrMagnitude < num)
                    {
                        num2 = sqrMagnitude;
                        if (GClass311.IsFollowerSuitableForBoss(this.botOwner_0.Profile.Info.Settings.Role, botOwner.Profile.Info.Settings.Role))
                        {
                            gclass = botOwner.Boss;
                        }
                    }
                }
            }
            if (gclass != null)
            {
                gclass.OfferSelf(this.botOwner_0);
            }
        }

        private void method_4(Player player, Player lastAggressor, DamageInfo damageInfo, EBodyPart part)
        {
            Action<Player> action = this.action_1;
            if (action == null)
            {
                return;
            }
            action(player);
        }

        private void method_5(GClass286 pairData)
        {
            float value = pairData.EndTime - Time.time;
            if (pairData.ShallStop)
            {
                this.PatrolDataFollower.StopFor(Mathf.Clamp(value, 1f, 60f));
            }
        }

        public bool Dispose()
        {
            this.PatrolDataFollower.Dispose();
            if (this.BossToFollow != null)
            {
                GClass481 pointChooser = GClass430.GetPointChooser(this.botOwner_0, PatrolMode.simple, this.botOwner_0.SpawnProfileData);
                if (this.botOwner_0.HealthController.IsAlive)
                {
                    GClass430 patrollingData = this.botOwner_0.PatrollingData;
                    if (patrollingData != null)
                    {
                        patrollingData.SetMode(PatrolMode.simple, pointChooser);
                    }
                }
                this.BossToFollow.RemoveFollower(this.botOwner_0);
                this.BossToFollow = null;
                return true;
            }
            return false;
        }

        private GClass420 gclass420_0;

        private GClass314 gclass314_0;

        [CompilerGenerated]
        private Action<GInterface2> action_0;

        [CompilerGenerated]
        private Action<Player> action_1;

        [CompilerGenerated]
        private GInterface2 ginterface2_0;

        [CompilerGenerated]
        private int int_0;
    }

}