using UnityEngine;

namespace SAIN.Components;

public class CoverManagerClass : BotManagerBase
{
    public CoverManagerClass(BotManagerComponent controller) : base(controller)
    {
    }

    public Collider CoverCollider { get; private set; }
    public void Init(Collider coverCollider)
    {
        CoverCollider = coverCollider;
    }
}