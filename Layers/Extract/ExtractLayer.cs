using EFT;
using SAIN.Components.BotController;

namespace SAIN.Layers
{
    internal class ExtractLayer : SAINLayer
    {
        public static readonly string Name = BuildLayerName<ExtractLayer>();

        public ExtractLayer(BotOwner bot, int priority) : base(bot, priority, Name)
        {
        }

        public override Action GetNextAction()
        {
            return new Action(typeof(ExtractAction), "Extract");
        }

        public override bool IsActive()
        {
            if (SAIN == null) return false;

            if (!SAIN.Info.FileSettings.Mind.EnableExtracts)
            {
                return false;
            }

            if (!ExtractFromTime() && !ExtractFromInjury() && !ExtractFromLoot() && !ExtractFromExternal())
            {
                return false;
            }

            if (SAIN.Memory.ExfilPosition == null)
            {
                BotExtractManager.TryFindExfilForBot(SAIN);
                return false;
            }

            if (BotExtractManager.GetTimeRemainingForExfil(SAIN.Memory.ExfilPoint) == 0)
            {
                float distance = (BotOwner.Position - SAIN.Memory.ExfilPosition.Value).sqrMagnitude;

                if (distance > ExtractAction.MinDistanceToStartExtract)
                {
                    SAIN.Memory.ExfilPoint = null;
                    SAIN.Memory.ExfilPosition = null;
                    return false;
                }
            }

            return true;
        }

        private bool ExtractFromTime()
        {
            float percentageLeft = BotController.BotExtractManager.PercentageRemaining;
            if (percentageLeft <= SAIN.Info.PercentageBeforeExtract)
            {
                if (!Logged)
                {
                    Logged = true;
                    Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract with [{percentageLeft}] of the raid remaining.");
                }
                if (SAIN.Enemy == null)
                {
                    return true;
                }
                else if (BotController.BotExtractManager.TimeRemaining < 120)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ExtractFromInjury()
        {
            if (SAIN.Memory.Dying && !BotOwner.Medecine.FirstAid.HaveSmth2Use)
            {
                if (!Logged)
                {
                    Logged = true;
                    Logger.LogInfo($"[{BotOwner.name}] Is Moving to Extract because of heavy injury and lack of healing items.");
                }
                if (SAIN.Enemy == null)
                {
                    return true;
                }
                else if (SAIN.Enemy.TimeSinceSeen > 30f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ExtractFromLoot()
        {
            return false;
        }

        private bool ExtractFromExternal()
        {
            return SAIN.Info.ForceExtract;
        }

        private bool Logged = false;

        public override bool IsCurrentActionEnding()
        {
            return false;
        }
    }
}