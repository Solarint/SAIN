using UnityEngine;

namespace SAIN.Models.Structs;

public struct SAINHardColliderData
{
    public SAINHardColliderData(Collider collider)
    {
        Collider = collider;
        Position = collider.transform.position;
    }

    public Collider Collider { get; }
    public Vector3 Position { get; }
}