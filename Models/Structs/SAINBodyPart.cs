using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Models.Structs;

public struct SAINBodyPart
{
    public readonly EBodyPart Type;
    public readonly BifacialTransform Transform;
    public readonly List<BodyPartCollider> Colliders;

    public SAINBodyPart(EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
    {
        Type = bodyPart;
        Transform = transform;
        Colliders = colliders;
    }
}

public struct SAINBodyPartRaycast
{
    public EBodyPart PartType;
    public EBodyPartColliderType ColliderType;
    public Vector3 CastPoint;
}