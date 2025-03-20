using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers.Combat.Run
{
    internal class CrawlAction : CombatAction, ISAINAction
    {
        public CrawlAction(BotOwner bot) : base(bot, nameof(CrawlAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            //Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);

            if (nextRandomRunTime > Time.time && (_runDestination - Bot.Position).sqrMagnitude < 1f)
            {
                nextRandomRunTime = 0f;
            }

            if (nextRandomRunTime < Time.time &&
                findRandomPlace(out var path) &&
                Bot.Mover.GoToPoint(_runDestination, out _, -1f, true))
            {
                nextRandomRunTime = Time.time + 20f;
            }
        }

        private Vector3 _runDestination;
        private float nextRandomRunTime;

        public override void Start()
        {
            Toggle(true);
        }

        private bool findRandomPlace(out NavMeshPath path)
        {
            for (int i = 0; i < 10;  i++)
            {
                Vector3 random = UnityEngine.Random.onUnitSphere * 100f;
                if (NavMesh.SamplePosition(random + Bot.Position, out var hit, 10f, -1))
                {
                    path = new NavMeshPath();
                    if (NavMesh.CalculatePath(Bot.Position, hit.position, -1, path))
                    {
                        _runDestination = path.corners[path.corners.Length - 1];
                        return true;
                    }
                }
            }
            path = null;
            return false;
        }

        public override void Stop()
        {
            Toggle(false);
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
        }
    }
}