using SAIN.Components.PlayerComponentSpace.Classes.Equipment;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Info;

namespace SAIN.Components.PlayerComponentSpace.Classes;

public class SAINAIData : PlayerComponentBase
{
    public bool IsAI => PlayerComponent.IsAI;

    public AIGearModifierClass AIGearModifier { get; private set; }

    public PlayerLocationClass PlayerLocation { get; private set; }

    public PlayerAISoundPlayer AISoundPlayer { get; private set; }

    public SAINAIData(GearInfo gearInfo, PlayerComponent component) : base(component)
    {
        PlayerLocation = new PlayerLocationClass(this);
        AIGearModifier = new AIGearModifierClass(this);
        AISoundPlayer = new PlayerAISoundPlayer(this);
    }
}