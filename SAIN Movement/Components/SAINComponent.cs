using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Layers.Logic;
using SAIN_Helpers;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        public bool FallingBack = false;
        public bool InCover = false;
        public float CoverRatio = 0f;

        public bool ActivateLayers => BotOwner.BotState == EBotState.Active && BotOwner.Memory.GoalEnemy != null && BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade == null;

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource($": {BotOwner.name}" + GetType().Name);

            Core = BotOwner.GetComponent<SAINCoreComponent>();
            LeanComponent = BotOwner.GetOrAddComponent<LeanComponent>();
            Hide = BotOwner.GetOrAddComponent<CoverComponent>();

            CheckForNull();

            Init();
        }

        private void Update()
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                if (CheckSelfCoverTimer < Time.time)
                {
                    CheckSelfCoverTimer = Time.time + CheckSelfFreq;

                    Vector3 debugPos = BotOwner.Memory.GoalEnemy.CurrPosition;
                    debugPos.y += 0.5f;

                    if (Cover.CheckSelfForCover(out CoverRatio, 0.33f))
                    {
                        DebugDrawer.Line(BotOwner.MyHead.position, debugPos, 0.05f, Color.white, CheckSelfFreq);
                        InCover = true;
                    }
                    else
                    {
                        DebugDrawer.Line(BotOwner.MyHead.position, debugPos, 0.05f, Color.red, CheckSelfFreq);
                        InCover = false;
                    }
                }
            }
        }

        private void CheckForNull()
        {
            if (Core == null)
            {
                Logger.LogError($"Core Component Null");
            }
            if (LeanComponent == null)
            {
                Logger.LogError($"Lean Component Null");
            }
            if (Hide == null)
            {
                Logger.LogError($"Hide Component Null");
            }
        }

        private void Init()
        {
            MovementLogic = new MovementLogic(BotOwner);
            Dodge = new BotDodge(BotOwner);
            Decisions = new DecisionLogic(BotOwner);
            Steering = new UpdateSteering(BotOwner);
            Targeting = new UpdateTarget(BotOwner);
            Move = new UpdateMove(BotOwner);
            Cover = new CoverLogic(BotOwner);

            DebugDrawList = new DebugGizmos.DrawLists(Core.BotColor, Core.BotColor);
        }

        public void Dispose()
        {
            Hide.Dispose();
            LeanComponent.Dispose();

            Destroy(this);
        }

        public CoverComponent Hide { get; private set; }
        public MovementLogic MovementLogic { get; private set; }
        public BotDodge Dodge { get; private set; }
        public DecisionLogic Decisions { get; private set; }
        public UpdateSteering Steering { get; private set; }
        public UpdateTarget Targeting { get; private set; }
        public UpdateMove Move { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverLogic Cover { get; private set; }
        public LeanComponent LeanComponent { get; private set; }
        public SAINCoreComponent Core { get; private set; }
        public BotSettings BotSettings { get; private set; } = new BotSettings();
        public BotOwner BotOwner { get; private set; }

        protected ManualLogSource Logger;
        private float CheckSelfCoverTimer = 0f;
        private float CheckSelfFreq = 0.25f;
    }

    public class BotSettings
    {
        public readonly float FightIn = 60f;
        public readonly float FightOut = 70f;

        public readonly float DogFightIn = 10f;
        public readonly float DogFightOut = 15f;

        public readonly float LowAmmoThresh0to1 = 0.3f;
    }
}