using SAIN.Models.Enums;
using System.Collections.Generic;

namespace SAIN.Components.BotController.PeacefulActions;

public class PeacefulActionSet : Dictionary<EPeacefulAction, IPeacefulActionController>
{
    public void CheckExecute(Dictionary<string, BotZoneData> datas)
    {
        foreach (var action in this.Values)
            foreach (var data in datas.Values)
                action.CheckExecute(data);
    }
}