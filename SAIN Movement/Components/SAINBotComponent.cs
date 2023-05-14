using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Classes;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;
using SAIN.Layers.Logic;

namespace SAIN.Components
{
    public class SAINBotComponent : MonoBehaviour
    {
        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource($": {BotOwner.name}" + GetType().Name);

            GetOrAddComponents();
            Init();
        }
        private void Init()
        {
            MovementLogic = new MovementLogic(BotOwner);
            Dodge = new BotDodge(BotOwner);
            Decisions = new Decisions(BotOwner);
            Steering = new UpdateSteering(BotOwner);
            Targeting = new UpdateTarget(BotOwner);
            Move = new UpdateMove(BotOwner);
            Cover = new CoverLogic(BotOwner);

            DebugDrawList = new DebugGizmos.DrawLists(Core.BotColor, Core.BotColor);
        }
        private void GetOrAddComponents()
        {
            Core = BotOwner.GetComponent<SAINCore>();
            LeanComponent = BotOwner.GetOrAddComponent<LeanComponent>();
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
        }
        public void Dispose()
        {
            StopAllCoroutines();

            LeanComponent.Dispose();
            CoverFinder.Dispose();

            Destroy(this);
        }

        public MovementLogic MovementLogic { get; private set; }
        public BotDodge Dodge { get; private set; }
        public Decisions Decisions { get; private set; }
        public UpdateSteering Steering { get; private set; }
        public UpdateTarget Targeting { get; private set; }
        public UpdateMove Move { get; private set; }
        public DebugGizmos.DrawLists DebugDrawList { get; private set; }
        public CoverLogic Cover { get; private set; }
        public LeanComponent LeanComponent { get; private set; }
        public CoverFinderComponent CoverFinder { get; private set; }
        public SAINCore Core { get; private set; }

        protected BotOwner BotOwner;
        protected ManualLogSource Logger;
    }
}