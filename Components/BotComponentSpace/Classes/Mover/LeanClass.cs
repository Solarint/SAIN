using EFT;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Linq;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class LeanClass : BotBase, IBotClass
    {
        private const float LEAN_UPDATE_FOUND_FREQ = 0.75f;
        private const float LEAN_UPDATE_NOT_FOUND_FREQ = 0.25f;
        private const float LEAN_RAYCAST_OFFSET_DIST = 0.66f;
        private const float LEAN_MAX_RAYCAST_DIST = 16f;
        private const float RESET_LEAN_AFTER_TIME = 1f;
        private const float MAX_CORNER_DISTANCE_LEAN = 20f;
        private const float MAX_CORNER_DISTANCE_LEAN_SQR = MAX_CORNER_DISTANCE_LEAN * MAX_CORNER_DISTANCE_LEAN;

        public LeanSetting LeanDirection { get; private set; }
        public LeanSetting LastLeanDirection { get; private set; }

        public LeanClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
        }

        private static readonly ECombatDecision[] DontLean =
        {
            ECombatDecision.Retreat,
            ECombatDecision.RunToCover,
            ECombatDecision.RunAway,
            ECombatDecision.MeleeAttack,
        };

        public void Update()
        {
            updateLean();
        }

        private void updateLean()
        {
            if (!checkShallLean()) {
                return;
            }
            if (_leanTimer < Time.time) {
                var enemy = Bot.CurrentTarget.CurrentTargetEnemy;
                findLean(enemy);
                float timeAdd = LeanDirection == LeanSetting.None ? LEAN_UPDATE_NOT_FOUND_FREQ : LEAN_UPDATE_FOUND_FREQ;
                _leanTimer = Time.time + timeAdd;
            }
        }

        private bool checkShallLean()
        {
            if (!Bot.Info.FileSettings.Move.LEAN_TOGGLE || !GlobalSettingsClass.Instance.Move.LEAN_TOGGLE) {
                ResetLean();
                return false;
            }
            if (!Bot.SAINLayersActive) {
                ResetLean();
                return false;
            }
            if (Bot.Mover.SprintController.Running || Player.IsSprintEnabled) {
                ResetLean();
                return false;
            }
            if (Bot.Enemy?.IsVisible == true && Bot.Decision.CurrentSelfDecision != ESelfDecision.None) {
                ResetLean();
                return false;
            }
            if (IsHoldingLean) {
                return false;
            }
            var CurrentDecision = Bot.Decision.CurrentCombatDecision;
            var enemy = Bot.CurrentTarget.CurrentTargetEnemy;
            if (enemy == null || Player.IsSprintEnabled || DontLean.Contains(CurrentDecision) || Bot.Suppression.IsHeavySuppressed) {
                ResetLean();
                return false;
            }
            if (GlobalSettingsClass.Instance.General.AILimit.LimitAIvsAIGlobal
                && enemy.IsAI
                && Bot.CurrentAILimit != AILimitSetting.None) {
                ResetLean();
                return false;
            }
            if (CurrentDecision == ECombatDecision.HoldInCover) {
                return false;
            }
            return true;
        }

        private void findLean(Enemy enemy)
        {
            var lastKnownPlace = enemy.KnownPlaces.LastKnownPlace;
            if (lastKnownPlace == null) {
                setLean(LeanSetting.None);
                return;
            }

            DirectLineOfSight = CheckOffSetRay(lastKnownPlace.Position, 0f, 0f, out var direct);
            if (DirectLineOfSight) {
                if (Time.time - _timeLastLeaned > RESET_LEAN_AFTER_TIME)
                    setLean(LeanSetting.None);

                return;
            }

            var blindCornerLean = FindLeanFromBlindCornerAngle(enemy, 1f);
            if (blindCornerLean != LeanSetting.None) {
                setLean(blindCornerLean);
                return;
            }

            var raycastLean = FindLeanDirectionRayCast(lastKnownPlace.Position);
            if (raycastLean != LeanSetting.None || Time.time - _timeLastLeaned > RESET_LEAN_AFTER_TIME)
                setLean(raycastLean);
        }

        public LeanSetting FindLeanFromBlindCornerAngle(Enemy enemy, float minAngle = -1f)
        {
            var blindCorner = enemy.Path.EnemyCorners.GetCorner(ECornerType.Blind);
            if (blindCorner == null) {
                return LeanSetting.None;
            }
            float signedAngle = blindCorner.SignedAngleToTarget;
            if (signedAngle == 0f) {
                return LeanSetting.None;
            }
            if (minAngle > 0f && Mathf.Abs(signedAngle) < minAngle) {
                return LeanSetting.None;
            }

            Vector3 direction = blindCorner.GroundPosition - Bot.Position;
            if (direction.sqrMagnitude > MAX_CORNER_DISTANCE_LEAN_SQR) {
                return LeanSetting.None;
            }

            LeanSetting result = signedAngle > 0 ? LeanSetting.Left : LeanSetting.Right;
            return result;
        }

        private float _stopHoldLeanTime;

        public bool IsHoldingLean => _stopHoldLeanTime > Time.time;

        public void HoldLean(float duration)
        {
            if (LeanDirection != LeanSetting.None) {
                _stopHoldLeanTime = Time.time + duration;
            }
        }

        public void Dispose()
        {
        }

        private float _leanTimer = 0f;

        public void ResetLean()
        {
            setLean(LeanSetting.None);
        }

        public LeanSetting FindLeanDirectionRayCast(Vector3 targetPos)
        {
            RightLos = CheckOffSetRay(targetPos, 90f, LEAN_RAYCAST_OFFSET_DIST, out var rightOffset);
            if (!RightLos) {
                RightLosPos = rightOffset;
                rightOffset.y = BotOwner.Position.y;
                float halfDist1 = (rightOffset - BotOwner.Position).magnitude / 2f;
                RightHalfLos = CheckOffSetRay(targetPos, 90f, halfDist1, out var rightHalfOffset);
                if (!RightHalfLos)
                    RightHalfLosPos = rightHalfOffset;
                else
                    RightHalfLosPos = null;
            }
            else {
                RightLosPos = null;
                RightHalfLosPos = null;
            }

            LeftLos = CheckOffSetRay(targetPos, -90f, LEAN_RAYCAST_OFFSET_DIST, out var leftOffset);
            if (!LeftLos) {
                LeftLosPos = leftOffset;
                leftOffset.y = BotOwner.Position.y;
                float halfDist2 = (leftOffset - BotOwner.Position).magnitude / 2f;
                LeftHalfLos = CheckOffSetRay(targetPos, -90f, halfDist2, out var leftHalfOffset);

                if (!LeftHalfLos)
                    LeftHalfLosPos = leftHalfOffset;
                else
                    LeftHalfLosPos = null;
            }
            else {
                LeftLosPos = null;
                LeftHalfLosPos = null;
            }
            return GetSettingFromResults();
        }

        private void setLean(LeanSetting leanSetting)
        {
            if (leanSetting != LeanSetting.None)
                _timeLastLeaned = Time.time;

            LastLeanDirection = LeanDirection;
            LeanDirection = leanSetting;
            Bot.Mover.FastLean(leanSetting);
        }

        private float _timeLastLeaned;

        public LeanSetting GetSettingFromResults()
        {
            LeanSetting setting;

            if (DirectLineOfSight) {
                return LeanSetting.None;
            }

            if ((LeftLos || LeftHalfLos) && !RightLos) {
                setting = LeanSetting.Left;
            }
            else if (!LeftLos && (RightLos || RightHalfLos)) {
                setting = LeanSetting.Right;
            }
            else {
                setting = LeanSetting.None;
            }

            return setting;
        }

        private bool CheckOffSetRay(Vector3 targetPos, float angle, float dist, out Vector3 Point)
        {
            Vector3 startPos = BotOwner.Position;
            startPos.y = Bot.Transform.HeadPosition.y;

            if (dist > 0f) {
                var dirToEnemy = (targetPos - BotOwner.Position).normalized;

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                Vector3 direction = rotation * dirToEnemy;

                Point = FindOffset(startPos, direction, dist);

                if ((Point - startPos).magnitude < dist / 3f) {
                    return true;
                }
            }
            else {
                Point = startPos;
            }

            bool LOS = LineOfSight(Point, targetPos);

            Point.y = BotOwner.Position.y;

            return LOS;
        }

        private bool LineOfSight(Vector3 start, Vector3 target)
        {
            var direction = target - start;
            float distance = Mathf.Clamp(direction.magnitude, 0f, LEAN_MAX_RAYCAST_DIST);
            return !Physics.Raycast(start, direction, distance, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private Vector3 FindOffset(Vector3 start, Vector3 direction, float distance)
        {
            if (Physics.Raycast(start, direction, out var hit, distance, LayerMaskClass.HighPolyWithTerrainMask)) {
                return hit.point;
            }
            else {
                return start + direction.normalized * distance;
            }
        }

        public bool DirectLineOfSight { get; set; }

        public bool LeftLos { get; set; }
        public Vector3? LeftLosPos { get; set; }

        public bool LeftHalfLos { get; set; }
        public Vector3? LeftHalfLosPos { get; set; }

        public bool RightLos { get; set; }
        public Vector3? RightLosPos { get; set; }

        public bool RightHalfLos { get; set; }
        public Vector3? RightHalfLosPos { get; set; }
    }
}