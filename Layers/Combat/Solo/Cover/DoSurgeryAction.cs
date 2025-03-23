using EFT;
using System.Collections;
using System.Text;
using UnityEngine;
using DrakiaXYZ.BigBrain.Brains;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class DoSurgeryAction : CombatAction, ISAINAction
    {
        public DoSurgeryAction(BotOwner bot) : base(bot, "Surgery")
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            if (Bot.Medical.Surgery.AreaClearForSurgery) {
                Bot.Mover.PauseMovement(30);
                Bot.Mover.SprintController.CancelRun();
                Bot.Mover.SetTargetMoveSpeed(0f);
                Bot.Cover.DuckInCover();
                tryStartSurgery();
            }
            else {
                BotOwner.Mover.MovementResume();
                Bot.Mover.SetTargetMoveSpeed(1);
                Bot.Mover.SetTargetPose(1);

                Bot.Medical.Surgery.SurgeryStarted = false;
                Bot.Medical.TryCancelHeal();
                Bot.Mover.DogFight.DogFightMove(false);
            }

            if (!Bot.Steering.SteerByPriority(null, false) &&
                !Bot.Steering.LookToLastKnownEnemyPosition(Bot.Enemy)) {
                Bot.Steering.LookToRandomPosition();
            }
        }

        private bool tryStartSurgery()
        {
            if (tryStart()) {
                return true;
            }
            if (checkFullHeal()) {
                return true;
            }
            return false;
        }

        private bool tryStart()
        {
            var surgery = BotOwner.Medecine.SurgicalKit;
            if (_startSurgeryTime < Time.time
                && !surgery.Using
                && surgery.ShallStartUse()) {
                Bot.Medical.Surgery.SurgeryStarted = true;
                surgery.ApplyToCurrentPart(new System.Action(onSurgeryDone));
                return true;
            }
            return false;
        }

        private bool checkFullHeal()
        {
            if (Bot.Medical.Surgery.SurgeryStarted = true &&
                _actionStartedTime + 30f < Time.time) {
                Bot.Player?.ActiveHealthController?.RestoreFullHealth();
                Bot.Decision.ResetDecisions(true);
                return true;
            }
            return false;
        }

        private void onSurgeryDone()
        {
            Bot.Medical.Surgery.SurgeryStarted = false;
            _actionStartedTime = Time.time;
            _startSurgeryTime = Time.time + 1f;

            if (BotOwner.Medecine.SurgicalKit.HaveWork) {
                if (Bot.Enemy == null || Bot.Enemy.TimeSinceSeen > 90f) {
                    Bot.Player?.ActiveHealthController?.RestoreFullHealth();
                    Bot.Decision.ResetDecisions(true);
                }
                return;
            }
            Bot.Decision.ResetDecisions(true);
        }

        public override void Start()
        {
            Toggle(true);
            Bot.Mover.PauseMovement(3f);
            _startSurgeryTime = Time.time + 1f;
            _actionStartedTime = Time.time;
        }

        private float _startSurgeryTime;
        private float _actionStartedTime;

        public override void Stop()
        {
            Toggle(false);
            Bot.Cover.CheckResetCoverInUse();
            Bot.Medical.Surgery.SurgeryStarted = false;
            BotOwner.MovementResume();
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"Health Status {Bot.Memory.Health.HealthStatus}");
            stringBuilder.AppendLine($"Surgery Started? {Bot.Medical.Surgery.SurgeryStarted}");
            stringBuilder.AppendLine($"Time Since Surgery Started {Time.time - Bot.Medical.Surgery.SurgeryStartTime}");
            stringBuilder.AppendLine($"Area Clear? {Bot.Medical.Surgery.AreaClearForSurgery}");
            stringBuilder.AppendLine($"ShallStartUse Surgery? {BotOwner.Medecine.SurgicalKit.ShallStartUse()}");
            stringBuilder.AppendLine($"IsBleeding? {BotOwner.Medecine.FirstAid.IsBleeding}");
        }
    }
}