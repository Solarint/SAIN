using EFT;
using EFT.InventoryLogic;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace SAIN.Combat.Components
{
    public class GClass544 : GInterface5
    {
        public event Action<Vector3> OnSettingsTarget
        {
            [CompilerGenerated]
            add
            {
                Action<Vector3> action = action_0;
                Action<Vector3> action2;
                do
                {
                    action2 = action;
                    Action<Vector3> value2 = (Action<Vector3>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange(ref action_0, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<Vector3> action = action_0;
                Action<Vector3> action2;
                do
                {
                    action2 = action;
                    Action<Vector3> value2 = (Action<Vector3>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange(ref action_0, value2, action2);
                }
                while (action != action2);
            }
        }
        public float LastSpreadCount { get; private set; }
        public float LastAimTime { get; private set; }
        public bool HardAim { get; private set; }
        public BotGlobalAimingSettings AimingSettings { get; set; }
        private bool Boolean_0 => float_12 > Time.time;
        public AimStatus Status
        {
            get => aimStatus_0;
            private set
            {
                if (aimStatus_0 != value && botOwner_0.BotState == EBotState.Active)
                {
                    aimStatus_0 = value;
                    bool flag;
                    if ((flag = (((bool_0 && botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Attack)) || botOwner_0.Memory.IsInCover || method_1()) && aimStatus_0 != AimStatus.NoTarget && method_0())) != botOwner_0.WeaponManager.ShootController.IsAiming)
                    {
                        botOwner_0.WeaponManager.ShootController.SetAim(flag);
                    }
                    HardAim = flag;
                    if (aimStatus_0 == AimStatus.AimComplete)
                    {
                        botOwner_0.BotPersonalStats.Aim(EndTargetPoint, float_7);
                    }
                }
            }
        }
        public Vector3 EndTargetPoint
        {
            get => vector3_1;
            set
            {
                vector3_1 = value;
                method_2();
            }
        }
        public bool IsReady => aimStatus_0 == AimStatus.AimComplete;
        public bool AlwaysTurnOnLight { get; private set; }
        public Vector3 RealTargetPoint { get; private set; }
        public float LastDist2Target { get; private set; }
        public GClass544(BotOwner owner)
        {
            AlwaysTurnOnLight = false;
            gclass543_0 = new GClass543(owner);
            botOwner_0 = owner;
            bifacialTransform_0 = botOwner_0.WeaponRoot;
            gclass561_0 = botOwner_0.Settings;
        }
        public void Activate()
        {
            bool_0 = GClass783.IsTrue100(50f);
            AimingSettings = botOwner_0.Settings.FileSettings.Aiming;
            AlwaysTurnOnLight = GClass783.IsTrue100(AimingSettings.ANYTIME_LIGHT_WHEN_AIM_100);
            int_3 = GClass783.RandomInclude(AimingSettings.BAD_SHOOTS_MIN, AimingSettings.BAD_SHOOTS_MAX);
            float_13 = AimingSettings.START_TIME_COEF;
        }
        public void GetHit(DamageInfo damageInfo)
        {
            if (GClass783.Random(0f, 100f) < AimingSettings.DAMAGE_TO_DISCARD_AIM_0_100)
            {
                gclass543_0.DoAffection();
                return;
            }
            float_7 += GClass783.Random(AimingSettings.MIN_TIME_DISCARD_AIM_SEC, AimingSettings.MAX_TIME_DISCARD_AIM_SEC);
        }
        public void LoseTarget()
        {
            Status = AimStatus.NoTarget;
        }
        public void SetTarget(Vector3 trg)
        {
            if ((RealTargetPoint - trg).sqrMagnitude > 0f)
            {
                Action<Vector3> action = action_0;
                if (action != null)
                {
                    action(trg);
                }
            }
            switch (aimStatus_0)
            {
                case AimStatus.Aiming:
                case AimStatus.AimComplete:
                    method_4(trg);
                    return;
                case AimStatus.NoTarget:
                    method_5(trg);
                    return;
                default:
                    return;
            }
        }
        public void SetNextAimingDelay(float nextAimingDelay)
        {
            if (float_10 < nextAimingDelay)
            {
                float_10 = nextAimingDelay;
            }
        }
        public void ShootDone(Weapon weapon)
        {
            float magnitude = (EndTargetPoint - bifacialTransform_0.position).magnitude;
            Debug.DrawRay(bifacialTransform_0.position, GClass782.NormalizeFastSelf(botOwner_0.LookDirection) * magnitude, Color.green, 2f);
            Debug.DrawLine(bifacialTransform_0.position, EndTargetPoint, Color.red, 2f);
            //this.EndTargetPoint - this.RealTargetPoint;
        }
        public void NodeUpdate()
        {
            float_5 += Time.deltaTime;
            method_2();
            if (float_5 > float_7)
            {
                float value = (AimingSettings.MAX_AIM_PRECICING - float_5 * botOwner_0.Settings.Current.CurrentPrecicingSpeed) / AimingSettings.MAX_AIM_PRECICING;
                float_13 = Mathf.Clamp(value, AimingSettings.MAX_AIMING_UPGRADE_BY_TIME, 1f);
                Status = AimStatus.AimComplete;
                Vector3 dir = gclass543_0.Affect(vector3_0);
                method_11(dir);
                return;
            }
            Status = AimStatus.Aiming;
            float num = float_5 / float_7;
            float t = num * num;
            Vector3 dir2 = Vector3.Lerp(vector3_6, vector3_0, t);
            Vector3 dir3 = gclass543_0.Affect(dir2);
            method_11(dir3);
        }
        public void Panic()
        {
            float_12 = Time.time + AimingSettings.PANIC_TIME;
        }
        public void Move(float speed = 0f)
        {
            float a = speed * Time.deltaTime;
            float_8 = Mathf.Lerp(a, float_8, 0.1f);
            bool_1 = (float_8 > AimingSettings.BOT_MOVE_IF_DELTA);
        }
        public void NextShotMiss()
        {
            bool_3 = true;
        }
        public void TriggerPressedDone()
        {
            int_0++;
            if (int_0 > int_2)
            {
                int_2 = GClass783.RandomInclude(AimingSettings.RECALC_MUST_TIME_MIN, AimingSettings.RECALC_MUST_TIME_MAX);
                bool_2 = true;
            }
        }
        public float GetCurRadius()
        {
            return float_13 * LastSpreadCount;
        }
        public override string ToString()
        {
            return (float_5 / float_7).ToString("0.00") + " %  " + aimStatus_0;
        }
        private bool method_0()
        {
            if (Time.time > float_11)
            {
                bool_4 = GClass783.IsTrue100((float)botOwner_0.Settings.FileSettings.Aiming.HARD_AIM_CHANCE_100);
                float_11 = Time.time + 10f;
            }
            return bool_4;
        }
        private bool method_1()
        {
            if (botOwner_0.Brain != null && botOwner_0.Brain.LastDecision != null)
            {
                BotLogicDecision? lastDecision = botOwner_0.Brain.LastDecision;
                if (!(lastDecision.GetValueOrDefault() == BotLogicDecision.shootFromPlace & lastDecision != null))
                {
                    lastDecision = botOwner_0.Brain.LastDecision;
                    if (!(lastDecision.GetValueOrDefault() == BotLogicDecision.dogFight & lastDecision != null))
                    {
                        lastDecision = botOwner_0.Brain.LastDecision;
                        return lastDecision.GetValueOrDefault() == BotLogicDecision.lay & lastDecision != null;
                    }
                }
                return true;
            }
            return false;
        }
        private void method_2()
        {
            Vector3 v = vector3_1 - bifacialTransform_0.position;
            if (v.sqrMagnitude > 2.25f)
            {
                vector3_3 = bifacialTransform_0.position;
                vector3_0 = v;
                return;
            }
            float num = method_3(v);
            float num2 = 1.5f;
            if (botOwner_0.GetPlayer.PoseLevel < 0.9f)
            {
                num2 = 0.9f;
            }
            Vector3 vector = botOwner_0.LookDirection;
            vector.y = 0f;
            vector = GClass782.NormalizeFastSelf(vector);
            Vector3 vector2 = new Vector3(botOwner_0.Position.x, botOwner_0.Position.y + num2, botOwner_0.Position.z);
            if (num > 0.25f)
            {
                Vector3 vector3 = GClass782.Rotate90(vector, GClass782.SideTurn.right);
                vector3 *= 0.4f;
                vector2 += vector3;
            }
            Vector3 vector4 = vector3_1 - vector2;
            vector3_3 = vector2;
            vector3_0 = vector4;
        }
        private float method_3(Vector3 v)
        {
            return v.x * v.x + v.z * v.z;
        }
        private void method_4(Vector3 target)
        {
            if (bool_2)
            {
                method_5(target);
                return;
            }
            Vector3 vector = target - RealTargetPoint;
            bool flag = Mathf.Abs(vector.y) > AimingSettings.RECLC_Y_DIST;
            vector.y = 0f;
            if (flag | vector.sqrMagnitude > AimingSettings.RECALC_SQR_DIST)
            {
                method_5(target);
            }
            else
            {
                method_12(target);
            }
            method_13();
        }
        private void method_5(Vector3 v)
        {
            bool_2 = false;
            botOwner_0.Mover.Sprint(false, true);
            int_0 = 0;
            Status = AimStatus.Aiming;
            vector3_6 = botOwner_0.LookDirection;
            RealTargetPoint = v;
            method_6(true);
            float_5 = 0f;
        }
        private void method_6(bool withTimeRecalc)
        {
            Vector3 to = RealTargetPoint - bifacialTransform_0.position;
            float num = Vector3.Angle(vector3_6, to);
            LastDist2Target = to.magnitude;
            float_9 = Time.time;
            vector3_4 = method_9(LastDist2Target, num, 1f);
            if (withTimeRecalc)
            {
                float_7 = method_7(LastDist2Target, num);
            }
        }
        private float method_7(float dist, float ang)
        {
            float num = 1f;
            if (bool_1)
            {
                num *= AimingSettings.TIME_COEF_IF_MOVE;
            }
            float num2 = Boolean_0 ? AimingSettings.PANIC_COEF : 1f;
            float num3 = gclass561_0.Curv.AimAngCoef.Evaluate(ang);
            float num4 = gclass561_0.Curv.AimTime2Dist.Evaluate(dist);
            float num5 = 1f;
            if (botOwner_0.Memory.IsInCover)
            {
                num5 = AimingSettings.COEF_FROM_COVER;
            }
            float num6 = num5 * AimingSettings.BOTTOM_COEF;
            float num7 = num3 * num4 * gclass561_0.Current.CurrentAccuratySpeed * num2;
            float num8 = (num6 + num7 + float_10) * num;
            float max_AIM_TIME = AimingSettings.MAX_AIM_TIME;
            if (num8 > max_AIM_TIME)
            {
                num8 = max_AIM_TIME;
            }
            float_10 = 0f;
            LastAimTime = num8;
            return num8;
        }
        private float method_8(float dist)
        {
            float f = botOwner_0.WeaponManager.WeaponAIPreset.BaseShift + dist;
            float p = botOwner_0.WeaponManager.IsCloseWeapon ? AimingSettings.SCATTERING_DIST_MODIF_CLOSE : AimingSettings.SCATTERING_DIST_MODIF;
            float num = Mathf.Pow(f, p);
            float num2 = botOwner_0.WeaponManager.IsCloseWeapon ? gclass561_0.Current.CurrentScatteringClose : gclass561_0.Current.CurrentScattering;
            return num * num2;
        }
        private Vector3 method_9(float dist, float angCoef, float additionCoef = 1f)
        {
            if (AimingSettings.DIST_TO_SHOOT_NO_OFFSET > dist)
            {
                return Vector3.zero;
            }
            GClass475 goalEnemy = botOwner_0.Memory.GoalEnemy;
            bool flag = false;
            if (goalEnemy != null)
            {
                int shootByTarget = botOwner_0.BotPersonalStats.GetShootByTarget(goalEnemy);
                int num = method_10(dist);
                if (shootByTarget < num)
                {
                    flag = true;
                }
            }
            LastSpreadCount = method_8(dist) * additionCoef;
            if (Boolean_0)
            {
                LastSpreadCount *= AimingSettings.PANIC_ACCURATY_COEF;
            }
            if (HardAim)
            {
                LastSpreadCount *= AimingSettings.HARD_AIM;
            }
            if (botOwner_0.BotLay.IsLay)
            {
                LastSpreadCount *= botOwner_0.Settings.FileSettings.Lay.LAY_AIM;
            }
            if (bool_1)
            {
                LastSpreadCount *= AimingSettings.COEF_IF_MOVE;
            }
            float num2 = 0f;
            float y = 0f;
            float num3 = 0f;
            float xz_COEF = botOwner_0.WeaponManager.WeaponAIPreset.XZ_COEF;
            float num4 = Mathf.Clamp(angCoef, 0f, 60f);
            float num5 = 2f * dist * Mathf.Sin(0.017453292f * num4 / 2f) * xz_COEF + LastSpreadCount;
            float num6 = LastSpreadCount * AimingSettings.Y_TOP_OFFSET_COEF;
            float num7 = -LastSpreadCount * AimingSettings.Y_BOTTOM_OFFSET_COEF;
            AimingType aimingType = botOwner_0.Settings.FileSettings.Core.AimingType;
            if (aimingType != AimingType.normal)
            {
                if (aimingType == AimingType.regular)
                {
                    num2 = GClass783.Random(-num5, num5);
                    y = GClass783.Random(num7, num6);
                    num3 = GClass783.Random(-num5, num5);
                }
            }
            else
            {
                num2 = GClass782.RandomNormal(-num5, num5);
                y = GClass782.RandomNormal(num7, num6);
                num3 = GClass782.RandomNormal(-num5, num5);
            }
            if (bool_3)
            {
                y = AimingSettings.NEXT_SHOT_MISS_Y_OFFSET;
                bool_3 = false;
            }
            if (flag)
            {
                float x = (num2 > 0f) ? AimingSettings.BAD_SHOOTS_OFFSET : (-AimingSettings.BAD_SHOOTS_OFFSET);
                float z = (num3 > 0f) ? AimingSettings.BAD_SHOOTS_OFFSET : (-AimingSettings.BAD_SHOOTS_OFFSET);
                vector3_5 = new Vector3(x, 0f, z);
            }
            else
            {
                vector3_5 = Vector3.zero;
            }
            return new Vector3(num2, y, num3);
        }
        private int method_10(float dist)
        {
            return int_3 + (int)(AimingSettings.BAD_SHOOTS_MAIN_COEF * Mathf.Log(1.2f + dist * 0.2f));
        }
        private void method_11(Vector3 dir)
        {
            vector3_2 = dir;
            botOwner_0.Steering.LookToDirection(dir, 500f);
            botOwner_0.Steering.SetYByDir(vector3_0);
        }
        private void method_12(Vector3 v)
        {
            if (Time.time - float_9 > AimingSettings.OFFSET_RECAL_ANYWAY_TIME)
            {
                method_6(false);
            }
            RealTargetPoint = v;
        }
        private void method_13()
        {
            if (DebugBotData.UseDebugData && DebugBotData.Instance.TrueAim)
            {
                EndTargetPoint = RealTargetPoint;
                return;
            }
            EndTargetPoint = RealTargetPoint + vector3_5 + float_13 * (vector3_4 + botOwner_0.RecoilData.RecoilOffset);
        }


        private const float float_0 = 60f;
        private const float float_1 = 0.4f;
        private const float float_2 = 0.9f;
        private const float float_3 = 2.25f;
        private const float float_4 = 1.5f;

        public bool CanChangePart;
        private bool bool_0;
        private float float_5;
        private float float_6;
        private float float_7;
        private Vector3 vector3_0;
        private Vector3 vector3_1;
        private bool bool_1;
        private float float_8;
        private Vector3 vector3_2;
        private float float_9;
        private Vector3 vector3_3;
        private readonly BifacialTransform bifacialTransform_0;
        private float float_10;
        private bool bool_2;
        private bool bool_3;
        private Vector3 vector3_4;
        private Vector3 vector3_5;
        private readonly BotOwner botOwner_0;
        private float float_11 = -1f;
        private bool bool_4;
        private float float_12;
        private readonly BotDifficultySettingsClass gclass561_0;
        private int int_0;
        private Vector3 vector3_6;
        private AimStatus aimStatus_0;
        private float float_13 = 1f;
        private readonly GClass543 gclass543_0;
        private int int_1;
        private int int_2 = 2;
        private int int_3;

        [CompilerGenerated]
        private Action<Vector3> action_0;
        [CompilerGenerated]
        private float float_14;
        [CompilerGenerated]
        private float float_15;
        [CompilerGenerated]
        private bool bool_5;
        [CompilerGenerated]
        private BotGlobalAimingSettings botGlobalAimingSettings_0;
        [CompilerGenerated]
        private bool bool_6;
        [CompilerGenerated]
        private Vector3 vector3_7;
        [CompilerGenerated]
        private float float_16;

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vector3_3, vector3_2);
        }
        public void DebugDraw()
        {
            Debug.DrawLine(bifacialTransform_0.position, EndTargetPoint, Color.yellow);
            Debug.DrawLine(bifacialTransform_0.position, RealTargetPoint, Color.red);
        }
        public void DrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(RealTargetPoint, GetCurRadius());
            Gizmos.color = Color.green;
            Gizmos.DrawLine(botOwner_0.MyHead.position, EndTargetPoint);
        }
        public void PermanentUpdate()
        {
        }
        public void RotateX(float angToRotate)
        {
        }
        public void RotateY(float deltaAngle)
        {
        }
        public void SetWeapon(Weapon weapon)
        {
        }
        public void SetTracers(bool isTracers)
        {
        }
        public void Dispose()
        {
        }
    }
}
