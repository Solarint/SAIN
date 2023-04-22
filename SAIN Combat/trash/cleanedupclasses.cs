using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace bsgcrap
{
    public class ScatterComponent
    {
        public BotOwner botOwner_0;
        public float YRecoil { get; private set; }
        public float CurScatering
        {
            get => scatter;
            set
            {
                bool isPeace = botOwner_0.Memory.IsPeace;
                float currentWorkingScatter = botOwner_0.Settings.Current.CurrentWorkingScatter;
                float currentMinScatter = botOwner_0.Settings.Current.CurrentMinScatter;
                float currentMaxScatter = botOwner_0.Settings.Current.CurrentMaxScatter;

                if (isPeace)
                {
                    IsMinScattering = false;

                    if (currentWorkingScatter <= currentMaxScatter && currentWorkingScatter >= currentMinScatter)
                    {
                        scatter = Mathf.Clamp(value, currentWorkingScatter, currentMaxScatter);
                        return;
                    }

                    scatter = Mathf.Clamp(value, currentMinScatter, currentMaxScatter);

                    return;
                }
                else
                {
                    if (value <= currentMinScatter)
                    {
                        IsMinScattering = true;
                        scatter = currentMinScatter;
                        return;
                    }

                    if (value >= currentMaxScatter)
                    {
                        IsMinScattering = false;
                        scatter = currentMaxScatter;
                        return;
                    }

                    IsMinScattering = false;

                    scatter = value;

                    return;
                }
            }
        }
        public bool IsMinScattering { get; private set; }
        public void Activate()
        {
            float handDamageScatteringMinMax = botOwner_0.Settings.FileSettings.Scattering.HandDamageScatteringMinMax;
            float handDamageAccuracySpeed = botOwner_0.Settings.FileSettings.Scattering.HandDamageAccuracySpeed;

            botOwner_0.Medecine.FirstAid.OnNoDamagedParts += Dispose;

            gclass557_0 = new GClass557(1f, handDamageAccuracySpeed, 1f, 1f, 1f, handDamageScatteringMinMax, 1f, 1f, 1f);

            scatter = botOwner_0.Settings.Current.CurrentMaxScatter;
        }
        public void ShootDone(Weapon weapon)
        {
            float recoilTotal = weapon.RecoilTotal;
            YRecoil += recoilTotal * botOwner_0.Settings.FileSettings.Scattering.RecoilYCoef;
            float num = botOwner_0.WeaponManager.IsNowAutomatic ? botOwner_0.Settings.FileSettings.Scattering.RecoilControlCoefShootDoneAuto : botOwner_0.Settings.FileSettings.Scattering.RecoilControlCoefShootDone;
            float num2 = recoilTotal * num;
            CurScatering += num2;
        }
        public void ManualUpdate(bool hardAim)
        {
            YRecoil = Mathf.Clamp(YRecoil + botOwner_0.Settings.FileSettings.Scattering.RecoilYCoefSppedDown * Time.deltaTime, 0f, botOwner_0.Settings.FileSettings.Scattering.RecoilYMax);

            if (botOwner_0.FlashGrenade.IsFlashed || botOwner_0.SmokeGrenade.IsInSmoke || (InjuredMove && MoveShooting))
            {
                UsingTracers = true;
            }

            float num = 1f;
            if (hardAim)
            {
                num *= botOwner_0.Settings.FileSettings.Scattering.SpeedUpAim;
            }
            if (UsingTracers)
            {
                num *= botOwner_0.Settings.FileSettings.Scattering.TracerCoef;
            }
            if (MoveShooting)
            {
                num *= botOwner_0.Settings.FileSettings.Scattering.MovingSlowCoef;
            }

            float num2;
            if (!botOwner_0.Mover.Sprinting && !ToUpBotSpeed)
            {
                num2 = botOwner_0.Settings.Current.CurrentScatteringUp * num;
            }
            else
            {
                num2 = botOwner_0.Settings.FileSettings.Scattering.SpeedDown;
            }

            if (num2 < 0f)
            {
                CurScatering -= num2 * Time.deltaTime;
            }
            else if (!ToLowBotSpeed)
            {
                CurScatering -= num2 * Time.deltaTime;
            }

            ToLowBotSpeed = false;
            ToUpBotSpeed = false;
        }
        public void GetHit(DamageInfo dmgInfo)
        {
            CurScatering += dmgInfo.Damage * botOwner_0.Settings.FileSettings.Scattering.FromShot;

            if (!gclass557_0.IsApplyed && (botOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.LeftArm) || botOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.RightArm)))
            {
                botOwner_0.Settings.Current.Apply(gclass557_0, -1f);
            }

            if (!InjuredMove)
            {
                InjuredMove = (botOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.LeftLeg) || botOwner_0.Medecine.FirstAid.IsPartDamaged(EBodyPart.RightLeg));
            }
        }
        public void RotateX(float angToRotate)
        {
            AngleMoveScatter(angToRotate);
        }
        public void RotateY(float angToRotate)
        {
        }
        public void BotMove(float dist)
        {
            MoveShooting = false;

            float num = dist / Time.deltaTime;

            if (num > botOwner_0.Settings.FileSettings.Scattering.ToUpBotSpeed)
            {
                MoveShooting = true;
                ToUpBotSpeed = true;
                return;
            }

            if (num > botOwner_0.Settings.FileSettings.Scattering.ToLowBotSpeed)
            {
                MoveShooting = true;
                ToLowBotSpeed = true;
                return;
            }

            if (num > botOwner_0.Settings.FileSettings.Scattering.ToSlowBotSpeed)
            {
                MoveShooting = true;
                ToSlowBotSpeed = true;
                return;
            }

            MoveShooting = false;
        }
        public void DrawGizmosSelected()
        {
            Vector3 position = botOwner_0.MyHead.position;
            Vector3 realTargetPoint = botOwner_0.AimingData.RealTargetPoint;
            float magnitude = (realTargetPoint - position).magnitude;
            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            float radius = magnitude * botOwner_0.Settings.Current.CurrentMaxScatter;
            Gizmos.DrawWireSphere(realTargetPoint, radius);
            Gizmos.color = new Color(0f, 1f, 1f);
            float radius2 = magnitude * botOwner_0.Settings.Current.CurrentMinScatter;
            Gizmos.DrawWireSphere(realTargetPoint, radius2);
        }
        public void Panic()
        {
        }
        public void SetTracers(bool isTracers)
        {
            UsingTracers = isTracers;
        }
        public void PoseChange(float f)
        {
            CurScatering += f * botOwner_0.Settings.FileSettings.Scattering.PoseChnageCoef;
        }
        public void Lay(bool b)
        {
            CurScatering += botOwner_0.Settings.FileSettings.Scattering.LayFactor;
        }
        private void DoneHealing()
        {
            if (gclass557_0.IsApplyed)
            {
                botOwner_0.Settings.Current.Dismiss(gclass557_0);
            }

            if (InjuredMove)
            {
                InjuredMove = false;
            }
        }
        private void AngleMoveScatter(float angToRotate)
        {
            float num = Mathf.Abs(angToRotate) / Time.deltaTime;

            if (num > botOwner_0.Settings.FileSettings.Scattering.ToLowBotAngularSpeed)
            {
                ToLowAngSpeed = true;
                return;
            }

            if (num > botOwner_0.Settings.FileSettings.Scattering.ToStopBotAngularSpeed)
            {
                ToStopBotAngularSpeed = true;
            }
        }
        public void Dispose()
        {
            botOwner_0.Medecine.FirstAid.OnNoDamagedParts -= DoneHealing;
        }
        private float scatter;
        private bool UsingTracers;
        private bool ToLowAngSpeed;
        private bool ToStopBotAngularSpeed;
        private bool ToUpBotSpeed;
        private bool ToLowBotSpeed;
        private bool ToSlowBotSpeed;
        private bool MoveShooting;
        private bool InjuredMove;
        private GClass557 gclass557_0;
    }
    public class AimingComponent
    {
        private BotOwner botOwner_0;
        public event Action<Vector3> OnSettingsTarget
        {
            add
            {
                Action<Vector3> action = action_0;

                Action<Vector3> action2;
                do
                {
                    action2 = action;
                    Action<Vector3> value2 = (Action<Vector3>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<Vector3>>(ref action_0, value2, action2);
                }
                while (action != action2);
            }

            remove
            {
                Action<Vector3> action = action_0;

                Action<Vector3> action2;
                do
                {
                    action2 = action;
                    Action<Vector3> value2 = (Action<Vector3>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<Vector3>>(ref action_0, value2, action2);
                }
                while (action != action2);
            }
        }
        public bool HardAim { get; private set; }
        public Vector3 From { get; private set; }
        public float SinDist { get; private set; }
        public Vector3 To { get; private set; }
        public Vector3 TargetToLook { get; private set; }
        public ScatterComponent ScatteringData { get; }
        public Vector3 EndTargetPoint => TargetToLook;
        public bool AlwaysTurnOnLight => _alwaysTurnOnLight;
        public bool IsReady
        {
            get
            {
                if (bool_0 || bool_1)
                {
                    return false;
                }
                if (ScatteringData.IsMinScattering)
                {
                    return true;
                }
                float num = float_0;
                return ScatteringData.CurScatering <= num;
            }
        }
        public Vector3 RealTargetPoint { get; private set; }
        public float LastDist2Target { get; private set; }

        public void GetHit(DamageInfo damageInfo)
        {
            ScatteringData.GetHit(damageInfo);
        }
        public void Activate()
        {
            ginterface110_0 = (botOwner_0.GetPlayer.HandsController as GInterface110);

            bool_2 = (UnityEngine.Random.Range(0, 100) < 50);

            botOwner_0.ShootData.OnTriggerPressed += method_3;

            botOwner_0.Mover.OnPoseChange += method_2;

            botOwner_0.BotLay.OnLay += method_1;

            ScatteringData.Activate();

            gclass557_0 = new GClass557(1f, 1f, 1f, 1f, 1f, 1f, 1f, botOwner_0.Settings.FileSettings.Scattering.ToCaution, 1f);
        }
        public void NodeUpdate()
        {
            aimStatus_0 = AimStatus.Aiming;

            method_6(RealTargetPoint, botOwner_0.MyHead.position);
        }
        public void LoseTarget()
        {
            aimStatus_0 = AimStatus.NoTarget;

            if (HardAim)
            {
                HardAim = false;

                ginterface110_0.SetAim(HardAim);
            }
        }
        public void SetTarget(Vector3 trg)
        {
            float sqrMagnitude = (trg - RealTargetPoint).sqrMagnitude;

            bool_0 = (sqrMagnitude > botOwner_0.Settings.FileSettings.Scattering.DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT);

            RealTargetPoint = trg;
        }
        public void SetNextAimingDelay(float nextAimingDelay)
        {
        }
        public void TriggerPressedDone()
        {
        }
        public void ShootDone(Weapon weapon)
        {
            ScatteringData.ShootDone(weapon);
        }
        public void Panic()
        {
            ScatteringData.Panic();
        }
        public void DrawGizmosSelected()
        {
            ScatteringData.DrawGizmosSelected();
            Gizmos.color = new Color(0.3f, 0.3f, 0.9f, 1f);
            Gizmos.DrawSphere(To, 0.11f);
            Gizmos.color = new Color(0.3f, 0.3f, 0.9f, 1f);
            Gizmos.DrawSphere(From, 0.11f);
            float num = method_4(LastDist2Target);
            Gizmos.color = new Color(0f, 0.95f, 0f, 1f);
            Gizmos.DrawWireSphere(RealTargetPoint, ScatteringData.CurScatering * LastDist2Target);
            Gizmos.color = (IsReady ? new Color(0.2f, 0.8f, 0.2f, 0.9f) : new Color(0.8f, 0.2f, 0.2f, 0.9f));
            Gizmos.DrawWireSphere(RealTargetPoint, num * LastDist2Target);
            if (IsReady)
            {
                Gizmos.color = new Color(0.1f, 1f, 0.1f, 1f);
            }
            else
            {
                Gizmos.color = new Color(1f, 0.1f, 0.1f, 1f);
            }
            Gizmos.DrawLine(TargetToLook, TargetToLook + new Vector3(0f, ScatteringData.YRecoil, 0f));
            Gizmos.DrawSphere(TargetToLook, 0.21f);
        }
        public void PermanentUpdate()
        {
            method_0();
            ScatteringData.ManualUpdate(HardAim);
        }
        public void RotateX(float angToRotate)
        {
            ScatteringData.RotateX(angToRotate);
        }
        public void RotateY(float deltaAngle)
        {
            ScatteringData.RotateY(deltaAngle);
        }
        public void SetWeapon(Weapon weapon)
        {
            MagazineClass currentMagazine = weapon.GetCurrentMagazine();

            bool tracers = false;
            if (currentMagazine != null && currentMagazine.Cartridges.Count > 0 && currentMagazine.Cartridges != null && currentMagazine.Cartridges.Last != null)
            {
                tracers = ((BulletClass)currentMagazine.Cartridges.Last).Tracer;
            }

            SetTracers(tracers);

            botOwner_0.Settings.CurrentScatteringSetting.SetWeapon(weapon);
        }
        public void SetTracers(bool isTracers)
        {
            ScatteringData.SetTracers(isTracers);
        }
        public void Move(float delta = 0f)
        {
        }
        public void NextShotMiss()
        {
        }
        private void method_0()
        {
            bool flag;

            if ((flag = (((bool_2 && botOwner_0.Tactic.IsCurTactic(BotGroupClass.BotCurrentTactic.Attack)) || botOwner_0.Memory.IsInCover) && aimStatus_0 != AimStatus.NoTarget)) != HardAim)
            {
                ginterface110_0.SetAim(flag);
            }

            HardAim = flag;
        }
        private void method_1(bool obj)
        {
            ScatteringData.Lay(obj);
        }
        private void method_2(float obj)
        {
            ScatteringData.PoseChange(obj);
        }
        private void method_3()
        {
            float bulletCount = (float)botOwner_0.WeaponManager.Reload.BulletCount;
            int maxBulletCount = botOwner_0.WeaponManager.Reload.MaxBulletCount;

            if (bulletCount / (float)maxBulletCount < botOwner_0.Settings.FileSettings.Scattering.Caution)
            {
                botOwner_0.Settings.Current.Apply(gclass557_0, -1f);
                return;
            }

            botOwner_0.Settings.Current.Dismiss(gclass557_0);
        }
        private float method_4(float dist)
        {
            if (dist < botOwner_0.Settings.Current.PriorityScatter1meter)
            {
                return botOwner_0.Settings.Current.PriorityScatter1meter;
            }

            if (dist < botOwner_0.Settings.Current.PriorityScatter10meter)
            {
                return method_5(dist, 1f, 10f, botOwner_0.Settings.Current.PriorityScatter1meter, botOwner_0.Settings.Current.PriorityScatter10meter);
            }

            if (dist < botOwner_0.Settings.Current.PriorityScatter100meter)
            {
                return method_5(dist, 10f, 100f, botOwner_0.Settings.Current.PriorityScatter1meter, botOwner_0.Settings.Current.PriorityScatter10meter);
            }

            return botOwner_0.Settings.Current.PriorityScatter100meter;
        }
        private float method_5(float cur, float min, float max, float lowVal, float hightVal)
        {
            float num = (cur - min) / (max - min);

            return (hightVal - lowVal) * num + lowVal;
        }
        private void method_6(Vector3 trg, Vector3 center)
        {
            Vector3 v = trg - center;

            LastDist2Target = v.magnitude;

            bool_1 = (LastDist2Target < botOwner_0.Settings.FileSettings.Scattering.DIST_NOT_TO_SHOOT);

            vector3_0 = GClass782.NormalizeFastSelf(v);

            float num = v.magnitude * ScatteringData.CurScatering;

            Vector3 a = GClass782.Rotate90(vector3_0, GClass782.SideTurn.left);

            Vector3 a2 = GClass782.Rotate90(vector3_0, GClass782.SideTurn.right);

            SinDist = num * botOwner_0.Settings.FileSettings.Scattering.AMPLITUDE_FACTOR;

            From = a * num + trg;

            To = a2 * num + trg;

            method_7();

            float_0 = method_4(LastDist2Target);

            Vector3 a3 = new Vector3(TargetToLook.x, TargetToLook.y + ScatteringData.YRecoil, TargetToLook.z);

            method_8(a3 - center);
        }
        private void method_7()
        {
            float_1 += botOwner_0.Settings.FileSettings.Scattering.AMPLITUDE_SPEED * Time.deltaTime;

            if (float_1 > 1f)
            {
                float_1 = 0f;
                bool_3 = !bool_3;
            }

            float num = bool_3 ? (1f - float_1) : float_1;

            TargetToLook = Vector3.Lerp(From, To, num);

            float num2 = num * 6.2831855f;

            float num3 = bool_3 ? Mathf.Sin(num2) : Mathf.Sin(num2 - 3.1415927f);

            float num4 = SinDist * num3;

            TargetToLook = new Vector3(TargetToLook.x, TargetToLook.y + num4, TargetToLook.z);
        }
        private void method_8(Vector3 dir)
        {
            botOwner_0.Steering.LookToDirection(dir, 500f);

            botOwner_0.Steering.SetYByDir(dir);
        }
        public void Dispose()
        {
            ScatteringData.Dispose();

            botOwner_0.Mover.OnPoseChange -= method_2;

            botOwner_0.ShootData.OnTriggerPressed -= method_3;

            botOwner_0.BotLay.OnLay -= method_1;
        }

        public bool _alwaysTurnOnLight;
        private float float_0;
        private bool bool_0;
        private bool bool_1;
        private bool bool_2;
        private Vector3 vector3_0;
        private GClass557 gclass557_0;
        private GInterface110 ginterface110_0;
        private AimStatus aimStatus_0 = AimStatus.NoTarget;
        private float float_1;
        private bool bool_3;
        private Action<Vector3> action_0;
    }
}
