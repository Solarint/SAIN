using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using System.Text;
using UnityEngine;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;
using UnityEngine.AI;
using SAIN.Helpers;

namespace SAIN.Layers.Combat.Run
{
    internal class GetUnstuckAction : CombatAction
    {
        public GetUnstuckAction(BotOwner bot) : base(bot, nameof(GetUnstuckAction))
        {
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);
            Bot.Steering.LookToMovingDirection();

            Vector3? unstuckDestination = null;
            var coverPoints = Bot.Cover.CoverPoints;
            if (coverPoints.Count > 0)
            {
                for (int i = 0; i < coverPoints.Count; i++)
                {
                    var cover = coverPoints[i];
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(cover.Position, Bot.Position, -1, path))
                    {
                        unstuckDestination = new Vector3?(path.corners[path.corners.Length - 1]);
                        break;
                    }
                }
            }

            if (unstuckDestination != null)
            {
                BotOwner.Mover.GoToByWay(new Vector3[] { Bot.Position, unstuckDestination.Value }, -1f);
            }
        }

        public override void Start()
        {
            Bot.Mover.StopMove();
        }

        public override void Stop()
        {
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {

        }
    }
}