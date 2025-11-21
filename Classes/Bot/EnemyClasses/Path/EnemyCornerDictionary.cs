using EFT;
using SAIN.Classes.Transform;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.Models.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyCornerDictionary : Dictionary<ECornerType, EnemyCorner>
{
    public EnemyCornerDictionary(PlayerTransformClass transform, BifacialTransform weaponRoot)
    {
    }

    public Vector3? GroundPosition(ECornerType type)
    {
        var corner = GetCorner(type);
        return corner?.GroundPosition;
    }

    public EnemyCorner GetCorner(ECornerType cornerType)
    {
        if (this.TryGetValue(cornerType, out EnemyCorner corner))
        {
            return corner;
        }
        return null;
    }

    public void AddOrReplace(ECornerType type, EnemyCorner corner)
    {
        if (corner == null)
        {
            this.Remove(type);
            return;
        }
        if (!this.ContainsKey(type))
        {
            this.Add(type, corner);
            return;
        }
        this[type] = corner;
    }
}