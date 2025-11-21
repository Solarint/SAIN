using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Models.Structs;

public struct BotPeekPlan(Vector3 start, Vector3 end, Vector3 dangerPoint)
{
    public PeekPosition PeekStart { get; private set; } = new PeekPosition(start, dangerPoint);
    public PeekPosition PeekEnd { get; private set; } = new PeekPosition(end, dangerPoint);
    public Vector3 DangerPoint { get; private set; } = dangerPoint;

    private bool CheckIfLeanable(float signAngle, float limit = 1f)
    {
        return Mathf.Abs(signAngle) > limit;
    }

    public LeanSetting GetDirectionToLean(float signAngle)
    {
        if (CheckIfLeanable(signAngle))
        {
            return signAngle > 0 ? LeanSetting.Right : LeanSetting.Left;
        }
        return LeanSetting.None;
    }

}