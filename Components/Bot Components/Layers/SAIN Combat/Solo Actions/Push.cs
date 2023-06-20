using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class RushEnemy : CustomLogic
    {
        public RushEnemy(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            if (SAIN.Enemy == null)
            {
                return;
            }

            SAIN.Mover.SetTargetPose(1f);
            SAIN.Mover.SetTargetMoveSpeed(1f);

            Vector3[] EnemyPath = SAIN.Enemy.Path.corners;
            Vector3 EnemyPos = SAIN.Enemy.Position;
            if (NewDestTimer < Time.time)
            {
                NewDestTimer = Time.time + 1f;
                Vector3 Destination = EnemyPos;
                /*
                if (SAIN.Info.BotPersonality == BotPersonality.GigaChad)
                {
                    if (EnemyPath.Length > 2)
                    {
                        Vector3 SecondToLastCorner = EnemyPath[EnemyPath.Length - 3];
                        Vector3 LastCornerDirection = LastCorner - SecondToLastCorner;
                        Vector3 AddToLast = LastCornerDirection.normalized * 3f;
                        Vector3 widePush = LastCorner + AddToLast;
                        if (SAIN.Mover.CanGoToPoint(widePush, out Vector3 PointToGo))
                        {
                            Destination = PointToGo;
                        }
                    }
                    else
                    {
                        Destination = Vector3.Lerp(EnemyPos, LastCorner, 0.75f);
                    }
                }
                else
                {
                    if (EnemyPath.Length > 2)
                    {
                        Vector3 SecondToLastCorner = EnemyPath[EnemyPath.Length - 3];
                        Vector3 LastCornerDirection = LastCorner - SecondToLastCorner;
                        Vector3 AddToLast = LastCornerDirection.normalized * 0.15f;
                        Vector3 shortPush = LastCorner - AddToLast;
                        if (SAIN.Mover.CanGoToPoint(shortPush, out Vector3 PointToGo))
                        {
                            Destination = PointToGo;
                        }
                    }
                    else
                    {
                        Destination = LastCorner;
                    }
                }
                */
                SAIN.Mover.GoToPoint(Destination);
            }

            if (SAIN.Enemy.InLineOfSight)
            {
                SAIN.Mover.Sprint(false);
                SAIN.Steering.LookToEnemy();
            }

            Shoot.Update();
        }

        private float NewDestTimer = 0f;
        private Vector3? PushDestination;

        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;

        public override void Start()
        {
            SAIN.Mover.Sprint(true);
        }

        public override void Stop()
        {
        }
    }
}