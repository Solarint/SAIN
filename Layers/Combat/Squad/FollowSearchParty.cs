using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Squad
{
    internal class FollowSearchParty : CombatAction, ISAINAction
    {
        public FollowSearchParty(BotOwner bot) : base(bot, nameof(FollowSearchParty))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
            Bot.Search.ToggleSearch(true, _enemy);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            if (!Bot.Mover.SprintController.Running) {
                Shoot.CheckAimAndFire();
                if (!Bot.Steering.SteerByPriority(_enemy, false)) {
                    Bot.Steering.LookToLastKnownEnemyPosition(_enemy ?? Bot.Enemy);
                }
            }

            if (_nextUpdatePosTime < Time.time) {
                moveToLead(out float nextTime);
                _nextUpdatePosTime = Time.time + nextTime;
            }
        }

        private void moveToLead(out float nextUpdateTime)
        {
            var leader = Bot.Squad.SquadInfo?.LeaderComponent;
            if (leader == null) {
                nextUpdateTime = 1f;
                return;
            }
            if ((_LastLeadPos - leader.Position).sqrMagnitude < 1f) {
                nextUpdateTime = 1f;
                return;
            }
            Vector3? movePosition = getPosNearLead(leader.Position);
            if (movePosition == null) {
                nextUpdateTime = 0.25f;
                return;
            }

            _LastLeadPos = leader.Position;
            float moveDistance = (movePosition.Value - Bot.Position).sqrMagnitude;
            if (moveDistance < 1f) {
                nextUpdateTime = 1f;
                return;
            }

            if (moveDistance > 20f * 20f &&
                Bot.Mover.SprintController.RunToPoint(movePosition.Value, SAINComponent.Classes.Mover.ESprintUrgency.Middle, true)) {
                nextUpdateTime = 2f;
                return;
            }
            if (Bot.Mover.SprintController.Running) {
                nextUpdateTime = 2f;
                return;
            }
            nextUpdateTime = 1f;
            Bot.Mover.GoToPoint(movePosition.Value, out _);
        }

        private Vector3? getPosNearLead(Vector3 leadPos)
        {
            Vector3? result = null;
            if (NavMesh.SamplePosition(leadPos, out var leadHit, 3f, -1)) {
                Vector3 leadDir = Bot.Position - leadHit.position;
                leadDir.y = 0;
                leadDir = leadDir.normalized * 2f;
                if (NavMesh.Raycast(leadHit.position, (leadDir + leadHit.position), out var rayHit, -1)) {
                    result = rayHit.position;
                }
                else {
                    result = leadDir + leadHit.position;
                }
            }
            return result;
        }

        private float _nextUpdatePosTime;
        private Vector3 _LastLeadPos;

        public override void Start()
        {
            _enemy = Bot.Enemy;
            Toggle(true);
            _nextUpdatePosTime = 0f;
            _LastLeadPos = Vector3.zero;
        }

        private Enemy _enemy;

        public override void Stop()
        {
            Toggle(false);
            _enemy = null;
            Bot.Mover.SprintController.CancelRun(0.25f);
        }
    }
}