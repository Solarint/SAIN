using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Layers.Logic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public class LeanComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();

            Lean = new BotLean(BotOwner);
            SideStep = new BotSideStep(BotOwner);
            BlindFire = new BotBlindFire(BotOwner);

            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private void Update()
        {
            if (SAIN.BotActive && TargetPosition != null && !SAIN.GameIsEnding && SAIN.CurrentDecision != SAINLogicDecision.Surgery && SAIN.CurrentDecision != SAINLogicDecision.Reload && SAIN.CurrentDecision != SAINLogicDecision.RunForCover && SAIN.CurrentDecision != SAINLogicDecision.FirstAid && SAIN.CurrentDecision != SAINLogicDecision.RunAwayGrenade)
            {
                if (LeanCoroutine == null)
                {
                    LeanCoroutine = StartCoroutine(BotLeanLoop());

                    SideStepCoroutine = StartCoroutine(BotSideStepLoop());

                    BlindFireCoroutine = StartCoroutine(BotBlindFireLoop());
                }
            }
            else
            {
                if (LeanCoroutine != null)
                {
                    StopCoroutine(LeanCoroutine);
                    LeanCoroutine = null;

                    StopCoroutine(SideStepCoroutine);
                    SideStepCoroutine = null;

                    StopCoroutine(BlindFireCoroutine);
                    BlindFireCoroutine = null;
                }
            }
        }

        private IEnumerator BotLeanLoop()
        {
            var wait = new WaitForSeconds(0.5f);

            while (true)
            {
                Lean.FindLeanDirectionRayCast(TargetPosition.Value);

                Lean.SetLean(Lean.Angle);

                SideStep.Update();

                yield return wait;
            }
        }

        private IEnumerator BotSideStepLoop()
        {
            var wait = new WaitForSeconds(0.5f);

            while (true)
            {
                //SideStep.Update();

                yield return wait;
            }
        }

        private IEnumerator BotBlindFireLoop()
        {
            var wait = new WaitForSeconds(0.25f);

            while (true)
            {
                BlindFire.Update(TargetPosition.Value);

                yield return wait;
            }
        }

        public Vector3? TargetPosition
        {
            get
            {
                if (BotOwner.Memory.GoalEnemy != null)
                {
                    BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.body, out BodyPartClass body);
                    return body.Position;
                }
                else if (BotOwner.Memory.GoalTarget?.GoalTarget?.Position != null)
                {
                    return BotOwner.Memory.GoalTarget.GoalTarget.Position;
                }
                else
                {
                    return null;
                }
            }
        }

        public LeanSetting LeanDirection => Lean.LeanDirection;

        public bool BotIsLeaning => LeanDirection == LeanSetting.Left || LeanDirection == LeanSetting.Right;

        public BotLean Lean { get; private set; }

        public BotSideStep SideStep { get; private set; }

        public BotBlindFire BlindFire { get; private set; }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private Coroutine LeanCoroutine;

        private Coroutine SideStepCoroutine;

        private Coroutine BlindFireCoroutine;

        private ManualLogSource Logger;

        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

        public class BotLean : SAINBot
        {
            public LeanSetting LeanDirection => RayCast.LeanDirection;
            public LeanRayCast RayCast { get; private set; }

            public BotLean(BotOwner bot) : base(bot)
            {
                RayCast = new LeanRayCast(bot);
                Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            }

            public void SetLean(float num)
            {
                BotOwner.GetPlayer.MovementContext.SetTilt(num);
            }

            public float Angle
            {
                get
                {
                    float angle;
                    switch (LeanDirection)
                    {
                        case LeanSetting.Left:
                            angle = -5f;
                            break;

                        case LeanSetting.Right:
                            angle = 5f;
                            break;

                        default:
                            angle = 0f;
                            break;
                    }
                    return angle;
                }
            }

            public LeanSetting FindLeanDirectionRayCast(Vector3 targetPos)
            {
                return RayCast.FindLeanDirectionRayCast(targetPos);
            }

            private LeanSetting FindLeanDirection(Vector3 targetPos)
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(BotOwner.Transform.position, targetPos, -1, path))
                {
                    // Corner 0 is at BotOwner position. So we need corner 1 and 2 to check lean angle.
                    if (path.corners.Length > 2)
                    {
                        Vector3 cornerADirection = (path.corners[1] - BotOwner.Transform.position).normalized;

                        var dirToEnemy = (targetPos - BotOwner.Transform.position).normalized;
                        Quaternion rotation = Quaternion.Euler(0, 90, 0);

                        var rightOfEnemy = rotation * dirToEnemy;

                        if (Vector3.Dot(cornerADirection, rightOfEnemy) > 0f)
                        {
                            return LeanSetting.Right;
                        }
                        else
                        {
                            return LeanSetting.Left;
                        }
                    }
                }

                return LeanSetting.None;
            }

            private float LeanFromCover(CoverPoint coverPoint, Vector3 targetPos)
            {
                float angle = 0f;
                if (coverPoint != null)
                {
                    Vector3 dirNormal = (targetPos - coverPoint.Position).normalized;
                    angle = Vector3.SignedAngle(coverPoint.DirectionToCollider.normalized, dirNormal, Vector3.up);
                }
                return angle;
            }

            private readonly ManualLogSource Logger;

            public class LeanRayCast : SAIN.SAINBot
            {
                public LeanSetting LeanDirection { get; set; }

                public LeanRayCast(BotOwner bot) : base(bot)
                {
                    Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {bot.name}: ");
                }

                public LeanSetting FindLeanDirectionRayCast(Vector3 targetPos)
                {
                    if (RayTimer < Time.time)
                    {
                        RayTimer = Time.time + 0.33f;

                        RightLineOfSight = CheckOffSetRay(targetPos, 90f, 0.66f, out var rightOffset);
                        RightLineOfSightOffset = rightOffset;

                        RightHalfLineOfSight = CheckOffSetRay(targetPos, 90f, 0.33f, out var rightHalfOffset);
                        RightHalfLineOfSightOffset = rightHalfOffset;

                        LeftLineOfSight = CheckOffSetRay(targetPos, -90f, 0.66f, out var leftOffset);
                        LeftLineOfSightOffset = leftOffset;

                        LeftHalfLineOfSight = CheckOffSetRay(targetPos, -90f, 0.33f, out var leftHalfOffset);
                        LeftLineOfSightOffset = leftHalfOffset;
                    }

                    var setting = GetSettingFromResults();
                    LeanDirection = setting;
                    return setting;
                }

                private float RayTimer = 0f;

                public LeanSetting GetSettingFromResults()
                {
                    LeanSetting setting;

                    if ((LeftLineOfSight || LeftHalfLineOfSight) && !RightLineOfSight)
                    {
                        setting = LeanSetting.Left;
                    }
                    else if (!LeftLineOfSight && (RightLineOfSight || RightHalfLineOfSight))
                    {
                        setting = LeanSetting.Right;
                    }
                    else
                    {
                        setting = LeanSetting.None;
                    }

                    return setting;
                }

                private bool CheckOffSetRay(Vector3 targetPos, float angle, float dist, out Vector3 offSet)
                {
                    var dirToEnemy = (targetPos - BotOwner.Position).normalized;

                    Quaternion rotation = Quaternion.Euler(0, angle, 0);

                    offSet = rotation * dirToEnemy;

                    offSet = offSet.normalized * dist;

                    offSet += BotOwner.Position;
                    offSet.y += 1.35f;

                    return LineOfSight(offSet, targetPos);
                }

                private bool LineOfSight(Vector3 start, Vector3 target)
                {
                    var direction = target - start;
                    float distance = Mathf.Clamp(direction.magnitude, 0f, 8f);
                    //DebugGizmos.SingleObjects.Ray(start, direction, Color.yellow, distance, 0.01f, true, 0.25f, true);

                    if (!Physics.Raycast(start, direction, distance, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        return true;
                    }
                    return false;
                }

                public bool LeftLineOfSight { get; set; }
                public Vector3 LeftLineOfSightOffset { get; set; }

                public bool LeftHalfLineOfSight { get; set; }
                public Vector3 LeftHalfLineOfSightOffset { get; set; }

                public bool RightLineOfSight { get; set; }
                public Vector3 RightLineOfSightOffset { get; set; }

                public bool RightHalfLineOfSight { get; set; }
                public Vector3 RightHalfLineOfSightOffset { get; set; }

                protected ManualLogSource Logger;
            }
        }

        public class BotSideStep : SAINBot
        {
            public SideStepSetting CurrentSideStep { get; private set; }

            public BotSideStep(BotOwner bot) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            }

            public void Update()
            {
                if (!SAIN.BotActive)
                {
                    return;
                }

                if (BotOwner.Memory.GoalEnemy == null)
                {
                    return;
                }

                var lean = SAIN.Lean.LeanDirection;
                var move = BotOwner.GetPlayer.MovementContext;
                var enemy = BotOwner.Memory.GoalEnemy;

                float value = 0f;
                SideStepSetting setting = SideStepSetting.None;

                if (!enemy.CanShoot && enemy.IsVisible)
                {
                    switch (lean)
                    {
                        case LeanSetting.Left:
                            value = -1f;
                            setting = SideStepSetting.Left;
                            break;

                        case LeanSetting.Right:
                            value = 1f;
                            setting = SideStepSetting.Right;
                            break;

                        default:
                            break;
                    }
                }

                move.SetSidestep(value);

                CurrentSideStep = setting;
            }

            private readonly ManualLogSource Logger;
        }

        public class BotBlindFire : SAINBot
        {
            public BotBlindFire(BotOwner bot) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            }

            public void Update(Vector3 targetPos)
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    return;
                }

                var enemy = BotOwner.Memory.GoalEnemy;
                int blindfire = 0;

                if (!enemy.CanShoot)
                {
                    if (RayCastCheck(BotOwner.WeaponRoot.position, targetPos))
                    {
                        Vector3 rayPoint = BotOwner.LookSensor._headPoint;
                        rayPoint.y += 0.1f;

                        if (!RayCastCheck(rayPoint, targetPos))
                        {
                            blindfire = 1;
                        }
                    }
                }

                if (blindfire == 1 && BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets)
                {
                    SetBlindFire(blindfire);
                    BotOwner.Steering.LookToPoint(targetPos);
                    //BotOwner.ShootData.Shoot();
                }
                else
                {
                    SetBlindFire(0);
                }
            }

            private bool RayCastCheck(Vector3 start, Vector3 targetPos)
            {
                Vector3 direction = targetPos - start;
                float magnitude = (targetPos - start).magnitude;
                return Physics.Raycast(start, direction, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
            }

            private void SetBlindFire(int value)
            {
                BotOwner.GetPlayer.MovementContext.SetBlindFire(value);
            }

            private int GetBlindFire => BotOwner.GetPlayer.MovementContext.BlindFire;

            private readonly ManualLogSource Logger;
        }
    }
}