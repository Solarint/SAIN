using SAIN.Models.Enums;
using System.Collections.Generic;

namespace SAIN.Components.BotController.PeacefulActions;

public interface IPeacefulActionController
{
    bool Active { get; }

    int Count { get; }

    EPeacefulAction Action { get; }

    void CheckExecute(BotZoneData data);

    List<IPeacefulActionExecutor> ActiveActions { get; }
}

public interface IBotPeacefulAction
{
    bool Complete { get; }

    void Update();

    void Start();

    void Stop();
}