using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Layers.Combat.Solo;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Collections;
using UnityEngine;

namespace SAIN.Layers.Combat.Squad
{
    internal class SuppressAction : CombatAction, ISAINAction
    {
        public SuppressAction(BotOwner bot) : base(bot, nameof(SuppressAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            var enemy = Bot.Enemy;
            if (enemy != null) {
                if (enemy.IsVisible && enemy.CanShoot) {
                    Bot.Mover.StopMove();
                    Shoot.CheckAimAndFire();
                    return;
                }

                if (Bot.ManualShoot.CanShoot(true) &&
                    FindSuppressionTarget(out var target) &&
                    CanSeeSuppressionTarget(target)) {
                    _manualShooting = true;
                    Bot.Mover.StopMove();

                    bool hasMachineGun = Bot.Info.WeaponInfo.EWeaponClass == EWeaponClass.machinegun;
                    if (hasMachineGun
                        && Bot.Mover.Prone.ShallProne(true)) {
                        Bot.Mover.Prone.SetProne(true);
                    }

                    bool shot = Bot.ManualShoot.TryShoot(true, target.Value, true, EShootReason.SquadSuppressing);

                    if (shot) {
                        enemy.Status.EnemyIsSuppressed = true;
                        float waitTime = hasMachineGun ? 0.1f : 0.5f;
                        _nextShotTime = Time.time + (waitTime * Random.Range(0.75f, 1.25f));
                    }
                    return;
                }

                Vector3? lastKnown = enemy.LastKnownPosition;
                if (lastKnown != null) {
                    Bot.Mover.GoToPoint(lastKnown.Value, out _, -1, false, false, false);
                }
            }

            resetManualShoot();
            if (!Bot.Steering.SteerByPriority(enemy, false)) {
                Bot.Steering.LookToLastKnownEnemyPosition(enemy);
            }
        }

        private void resetManualShoot()
        {
            if (_manualShooting) {
                _manualShooting = false;
                Bot.ManualShoot.TryShoot(false, Vector3.zero);
            }
        }

        private bool _manualShooting;

        private float _nextShotTime;

        private bool FindSuppressionTarget(out Vector3? pos)
        {
            pos = Bot.Enemy?.SuppressionTarget;
            return pos != null;
        }

        private bool CanSeeSuppressionTarget(Vector3? target)
        {
            if (target == null) {
                _canSeeSuppTarget = false;
            }
            else if (_nextCheckVisTime < Time.time) {
                _nextCheckVisTime = Time.time + 0.5f;
                Vector3 myHead = Bot.Transform.HeadPosition;
                _canSeeSuppTarget = !Physics.Raycast(myHead, target.Value - myHead, (target.Value - myHead).magnitude * 0.8f);
            }
            return _canSeeSuppTarget;
        }

        private bool _canSeeSuppTarget;

        private float _nextCheckVisTime;

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
            resetManualShoot();
        }
    }
}