using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class SAINBotUnstuck : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private RaycastHit StuckHit = new RaycastHit();
        private float DebugStuckTimer = 0f;
        private float CheckStuckTimer = 0f;
        public float TimeSinceStuck => Time.time - TimeStuck;
        public float TimeStuck { get; private set; }

        private float CheckMoveTimer = 0f;

        private Vector3 LastPos = Vector3.zero;

        private ManualLogSource Logger;

        public float TimeSpentNotMoving => Time.time - TimeStartNotMoving;
        public float TimeStartNotMoving { get; private set; }

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (SAIN.BotActive && !SAIN.GameIsEnding)
            {
                if (CheckMoveTimer < Time.time)
                {
                    bool botWasMoving = BotIsMoving;
                    CheckMoveTimer = Time.time + 0.33f;
                    BotIsMoving = (LastPos - BotOwner.Position).sqrMagnitude > 0.01f;
                    LastPos = BotOwner.Position;

                    if (botWasMoving && !BotIsMoving)
                    {
                        TimeStartNotMoving = Time.time;
                    }
                    else if (BotIsMoving)
                    {
                        TimeStartNotMoving = 0f;
                    }
                }

                if (CheckStuckTimer < Time.time)
                {
                    CheckStuckTimer = Time.time + 0.25f;
                    bool stuck = BotStuckOnObject() || BotStuckOnPlayer();
                    if (!BotIsStuck && stuck)
                    {
                        TimeStuck = Time.time;
                    }
                    BotIsStuck = stuck;
                }

                if (BotIsStuck)
                {
                    if (DebugStuckTimer < Time.time && TimeSinceStuck > 1f)
                    {
                        DebugStuckTimer = Time.time + 3f;
                        Logger.LogWarning($"[{BotOwner.name}] has been stuck for [{TimeSinceStuck}] seconds on [{StuckHit.transform.name}] object at [{StuckHit.transform.position}] with Current Decision as [{SAIN.CurrentDecision}]");
                    }
                    if (JumpTimer < Time.time && TimeSinceStuck > 1f)
                    {
                        JumpTimer = Time.time + 1f;
                        SAIN.Mover.TryJump();
                    }
                }
            }
        }

        public bool BotIsStuck { get; private set; }

        private bool CanBeStuckDecisions(SAINSoloDecision decision)
        {
            return decision == SAINSoloDecision.Search || decision == SAINSoloDecision.WalkToCover || decision == SAINSoloDecision.DogFight || decision == SAINSoloDecision.RunToCover || decision == SAINSoloDecision.RunAway || decision == SAINSoloDecision.UnstuckSearch || decision == SAINSoloDecision.UnstuckDogFight || decision == SAINSoloDecision.UnstuckMoveToCover;
        }

        public bool BotStuckOnPlayer()
        {
            var decision = SAIN.CurrentDecision;
            if (!BotIsMoving && CanBeStuckDecisions(decision))
            {
                if (BotOwner.Mover == null)
                {
                    return false;
                }
                Vector3 botPos = BotOwner.Position;
                botPos.y += 0.4f;
                Vector3 moveDir = BotOwner.Mover.DirCurPoint;
                moveDir.y = 0;
                Vector3 lookDir = BotOwner.LookDirection;
                lookDir.y = 0;

                var moveHits = Physics.SphereCastAll(botPos, 0.15f, moveDir, 0.5f, LayerMaskClass.PlayerMask);
                if (moveHits.Length > 0)
                {
                    foreach (var move in moveHits)
                    {
                        if (move.transform.name != BotOwner.name)
                        {
                            StuckHit = move;
                            return true;
                        }
                    }
                }

                var lookHits = Physics.SphereCastAll(botPos, 0.15f, lookDir, 0.5f, LayerMaskClass.PlayerMask);
                if (lookHits.Length > 0)
                {
                    foreach (var look in lookHits)
                    {
                        if (look.transform.name != BotOwner.name)
                        {
                            StuckHit = look;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool BotStuckOnObject()
        {
            if (CanBeStuckDecisions(SAIN.CurrentDecision) && !BotIsMoving && !BotOwner.DoorOpener.Interacting && SAIN.Decision.TimeSinceChangeDecision > 0.5f)
            {
                if (BotOwner.Mover == null)
                {
                    return false;
                }
                Vector3 botPos = BotOwner.Position;
                botPos.y += 0.4f;
                Vector3 moveDir = BotOwner.Mover.DirCurPoint;
                moveDir.y = 0;
                if (Physics.SphereCast(botPos, 0.15f, moveDir, out var hit, 0.25f, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    StuckHit = hit;
                    return true;
                }
            }
            return false;
        }

        public bool BotIsMoving { get; private set; }

        private float JumpTimer = 0f;
    }
}
