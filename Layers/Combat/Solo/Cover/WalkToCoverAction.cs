using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Text;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class WalkToCoverAction : CombatAction, ISAINAction
    {
        public WalkToCoverAction(BotOwner bot) : base(bot, nameof(WalkToCoverAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        private float _nextUpdateCoverTime;

        public override void Update(CustomLayer.ActionData data)
        {
            Bot.Mover.SetTargetMoveSpeed(1f);
            Bot.Mover.SetTargetPose(1f);

            if (Bot.Enemy == null) {
                return;
            }

            if (Bot.Cover.CoverPoints.Count == 0) {
                Bot.Mover.DogFight.DogFightMove(true);
                EngageEnemy();
                return;
            }

            if (_nextUpdateCoverTime < Time.time) {
                _nextUpdateCoverTime = Time.time + 0.1f;

                findCover();
                reCheckCover();
            }

            if (Bot.Cover.CoverInUse == null) {
                Bot.Mover.DogFight.DogFightMove(false);
            }

            EngageEnemy();
        }

        private void findCover()
        {
            CoverPoint coverInUse = Bot.Cover.CoverInUse;
            if (coverInUse == null || coverInUse.CoverData.IsBad) {
                //if (shallFallback())
                //{
                //    RecalcPathTimer = Time.time + 1f;
                //    return;
                //}

                Bot.Cover.SortPointsByPathDist();
                var points = Bot.Cover.CoverPoints;
                for (int i = 0; i < points.Count; i++) {
                    var coverPoint = points[i];
                    if (checkMoveToCover(coverPoint)) {
                        RecalcPathTimer = Time.time + 1f;
                        return;
                    }
                }
            }
        }

        private bool shallFallback()
        {
            return Bot.Decision.CurrentSelfDecision != ESelfDecision.None &&
                checkMoveToCover(Bot.Cover.FallBackPoint);
        }

        private void reCheckCover()
        {
            CoverPoint coverInUse = Bot.Cover.CoverInUse;
            if (coverInUse != null
                && RecalcPathTimer < Time.time) {
                RecalcPathTimer = Time.time + 1f;
                if (!checkMoveToCover(coverInUse)) {
                    Bot.Cover.CoverInUse = null;
                    _nextUpdateCoverTime = -1f;
                }
            }
        }

        private bool checkMoveToCover(CoverPoint coverPoint)
        {
            if (coverPoint != null &&
                !coverPoint.Spotted &&
                !coverPoint.CoverData.IsBad &&
                Bot.Mover.GoToPoint(coverPoint.Position, out _, -1, false, true, true)) {
                Bot.Cover.CoverInUse = coverPoint;
                _coverDestination = coverPoint;
                return true;
            }
            return false;
        }

        private float RecalcPathTimer = 0f;

        private CoverPoint _coverDestination;
        private float _suppressTime;

        private void EngageEnemy()
        {
            if (!Bot.Enemy.IsVisible &&
                Time.time - _timeStart > 1f &&
                BotOwner.WeaponManager.HaveBullets &&
                Bot.Enemy.TimeSinceLastKnownUpdated < 30f) {
                Vector3? suppressTarget = Bot.Enemy?.SuppressionTarget;
                if (suppressTarget != null) {
                    SuppressPosition(suppressTarget.Value);
                    return;
                }
            }

            if (_suppressTime < Time.time)
                resetSuppressing();

            if (!Bot.Steering.SteerByPriority(null, false))
                Bot.Steering.LookToLastKnownEnemyPosition(Bot.Enemy);

            Shoot.CheckAimAndFire();
        }

        private bool suppressing;

        private Vector3? findSuppressTarget()
        {
            return Bot.Enemy?.SuppressionTarget;
        }

        private void SuppressPosition(Vector3 position)
        {
            if (_suppressTime > Time.time) {
                return;
            }
            if (!Bot.ManualShoot.TryShoot(true, position, true, EShootReason.WalkToCoverSuppress)) {
                return;
            }

            suppressing = true;
            Bot.Enemy.Status.EnemyIsSuppressed = true;

            float timeAdd;
            if (Bot.Info.WeaponInfo.EWeaponClass == EWeaponClass.machinegun) {
                timeAdd = 0.05f * Random.Range(0.75f, 1.25f);
            }
            else {
                timeAdd = 0.25f * Random.Range(0.66f, 1.33f);
            }

            _suppressTime = Time.time + timeAdd;
        }

        public override void Start()
        {
            Toggle(true);
            _timeStart = Time.time;
        }

        public override void Stop()
        {
            Toggle(false);

            Bot.Mover.DogFight.ResetDogFightStatus();
            Bot.Cover.CheckResetCoverInUse();
            resetSuppressing();
        }

        private void resetSuppressing()
        {
            if (suppressing) {
                suppressing = false;
                Bot.ManualShoot.TryShoot(false, Vector3.zero);
                if (Bot.Enemy != null) {
                    Bot.Enemy.Status.EnemyIsSuppressed = false;
                }
            }
        }

        private float _timeStart;

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Walk To Cover Info");
            var cover = Bot.Cover;
            stringBuilder.AppendLabeledValue("CoverFinder State", $"{cover.CurrentCoverFinderState}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Cover Count", $"{cover.CoverPoints.Count}", Color.white, Color.yellow, true);
            if (Bot.CurrentTargetPosition != null) {
                stringBuilder.AppendLabeledValue("Current Target Position", $"{Bot.CurrentTargetPosition.Value}", Color.white, Color.yellow, true);
            }
            else {
                stringBuilder.AppendLabeledValue("Current Target Position", null, Color.white, Color.yellow, true);
            }

            if (_coverDestination != null) {
                stringBuilder.AppendLine("Cover Destination");
                stringBuilder.AppendLabeledValue("Status", $"{_coverDestination.StraightDistanceStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Height / Value", $"{_coverDestination.CoverHeight} {_coverDestination.HardData.Value}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Path Length", $"{_coverDestination.PathLength}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Straight Distance", $"{(_coverDestination.Position - Bot.Position).magnitude}", Color.white, Color.yellow, true);
            }
        }
    }
}