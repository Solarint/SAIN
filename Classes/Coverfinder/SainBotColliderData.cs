using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public struct SainBotColliderData(Collider collider)
{
    public Collider Collider = collider;
    public CoverPoint CoverPoint = null;
    public float SqrMagnitude;

    public static bool operator ==(SainBotColliderData left, SainBotColliderData right)
    {
        return Equals(left.Collider, right.Collider);
    }

    public static bool operator !=(SainBotColliderData left, SainBotColliderData right)
    {
        return !(left == right);
    }

    public static bool operator ==(SainBotColliderData left, Collider right)
    {
        return Equals(left.Collider, right);
    }

    public static bool operator !=(SainBotColliderData left, Collider right)
    {
        return !(left == right);
    }

    public static bool operator ==(Collider left, SainBotColliderData right)
    {
        return Equals(left, right.Collider);
    }

    public static bool operator !=(Collider left, SainBotColliderData right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is SainBotColliderData other)
        {
            return this == other;
        }
        if (obj is Collider collider)
        {
            return this == collider;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Collider != null ? Collider.GetHashCode() : 0;
    }
}