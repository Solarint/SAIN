using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Classes;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;
using SAIN.Layers.Logic;

namespace SAIN.Components
{
    public class SAINComponent : MonoBehaviour
    {
        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource($": {BotOwner.name}" + GetType().Name);

            Core = BotOwner.GetComponent<SAINCoreComponent>();
            LeanComponent = BotOwner.GetOrAddComponent<LeanComponent>();
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Hide = BotOwner.GetOrAddComponent<HideComponent>();

            CheckForNull();

            Init();
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
            if (CoverFinder == null)
            {
                Logger.LogError($"CoverFinder Component Null");
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
            CoverFinder.Dispose();

            Destroy(this);
        }

        public HideComponent Hide { get; private set; }
        public MovementLogic MovementLogic { get; private set; }
        public BotDodge Dodge { get; private set; }
        public DecisionLogic Decisions { get; private set; }
        public UpdateSteering Steering { get; private set; }
        public UpdateTarget Targeting { get; private set; }
        public UpdateMove Move { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverLogic Cover { get; private set; }
        public LeanComponent LeanComponent { get; private set; }
        public CoverFinderComponent CoverFinder { get; private set; }
        public SAINCoreComponent Core { get; private set; }
        public BotSettings BotSettings { get; private set; } = new BotSettings();
        public BotOwner BotOwner { get; private set; }

        protected ManualLogSource Logger;
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