using SAIN.SAINComponent.Classes.Info;

namespace SAIN.Components.PlayerComponentSpace.Classes;

public abstract class AIDataBase
{
    public AIDataBase(SAINAIData aidata)
    {
        AIData = aidata;
    }

    protected readonly SAINAIData AIData;
    protected GearInfo GearInfo => AIData.PlayerComponent.Equipment.GearInfo;
    protected bool IsAI => AIData.IsAI;
}