using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Solo
{
    internal class SearchAction : SAINAction
    {
        public SearchAction(BotOwner bot) : base(bot, nameof(SearchAction))
        {
            SAIN = bot.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

        private SearchClass Search;

        public override void Start()
        {
            Search = new SearchClass(SAIN);
            FindTarget();
        }

        private void FindTarget()
        {
            Vector3 pos = Search.SearchMovePos();
            if (Search?.GoToPoint(pos) != NavMeshPathStatus.PathInvalid)
            {
                TargetPosition = pos;
            }
        }

        private Vector3? TargetPosition;

        public override void Stop()
        {
            TargetPosition = null;
        }

        private float CheckMagTimer;
        private float CheckChamberTimer;
        private float NextCheckTimer;
        private float ReloadTimer;

        public override void Update()
        {
            Shoot.Update();
            CheckWeapon();

            if (TargetPosition != null)
            {
                MoveToEnemy();
            }
            else
            {
                FindTarget();
            }
        }

        private void CheckWeapon()
        {
            if (SAIN.Enemy != null)
            {
                if (SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceSeen > 10f || !SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceEnemyCreated > 10f)
                {
                    if (ReloadTimer < Time.time && SAIN.Decision.SelfActionDecisions.LowOnAmmo(0.5f))
                    {
                        ReloadTimer = Time.time + 10f;
                        SAIN.SelfActions.TryReload();
                    }
                    else if (CheckMagTimer < Time.time && NextCheckTimer < Time.time)
                    {
                        NextCheckTimer = Time.time + 3f;
                        CheckMagTimer = Time.time + 240f * Random.Range(0.5f, 1.5f);
                        SAIN.Player.HandsController.FirearmsAnimator.CheckAmmo();
                    }
                    else if (CheckChamberTimer < Time.time && NextCheckTimer < Time.time)
                    {
                        NextCheckTimer = Time.time + 3f;
                        CheckChamberTimer = Time.time + 240f * Random.Range(0.5f, 1.5f);
                        SAIN.Player.HandsController.FirearmsAnimator.CheckChamber();
                    }
                }
            }
        }

        private void MoveToEnemy()
        {
            if (SAIN.Enemy == null && (BotOwner.Position - TargetPosition.Value).sqrMagnitude < 2f)
            {
                SAIN.Decision.ResetDecisions();
                return;
            }
            CheckShouldSprint();
            Search.Search(!SprintEnabled, SprintEnabled);
            if (Search?.ActiveDestination != null)
            {
                Steer(Search.ActiveDestination);
            }
            else
            {
                SAIN.Steering.SteerByPriority();
            }
        }

        private void CheckShouldSprint()
        {
            if (Search.PeekingCorner)
            {
                return;
            }

            var pers = SAIN.Info.Personality;
            if (RandomSprintTimer < Time.time && SAIN.Info.PersonalitySettings.SprintWhileSearch)
            {
                RandomSprintTimer = Time.time + 3f * Random.Range(0.5f, 2f);
                float chance = pers == SAINPersonality.GigaChad ? 40f : 20f;
                SprintEnabled = EFTMath.RandomBool(chance);
            }
        }

        private bool SprintEnabled = false;
        private float RandomSprintTimer = 0f;

        private void Steer(Vector3 pos)
        {
            if (Search.PeekingCorner)
            {
                SAIN.Mover.SetTargetMoveSpeed(0.25f);
                SAIN.Mover.SetTargetPose(0.75f);
            }
            else if (SprintEnabled)
            {
                SAIN.Mover.SetTargetMoveSpeed(1f);
                SAIN.Mover.SetTargetPose(1f);
            }
            else
            {
                SAIN.Mover.SetTargetMoveSpeed(0.66f);
                SAIN.Mover.SetTargetPose(0.85f);
            }

            if (!SprintEnabled || BotOwner.Memory.IsUnderFire)
            {
                SAIN.Mover.Sprint(false);
                if (!SAIN.Steering.SteerByPriority(false))
                {
                    SAIN.Steering.LookToMovingDirection();
                }
            }
        }

        private readonly SAINComponentClass SAIN;
        public NavMeshPath Path = new NavMeshPath();
    }
}