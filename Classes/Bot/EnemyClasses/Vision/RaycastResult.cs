using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class RaycastResult
{
    public float TimeLastChecked { get; private set; }
    public float TimeLastSuccess { get; private set; }
    public RaycastHit LastRaycastHit { get; private set; }
    public BodyPartCollider LastSuccessBodyPart { get; private set; }
    public Vector3? LastSuccessPoint { get; private set; }

    public void Update(Vector3 castPoint, BodyPartCollider bodyPartCollider, RaycastHit raycastHit, float time)
    {
        TimeLastChecked = time;
        LastRaycastHit = raycastHit;

        if (raycastHit.collider == null)
        {
            LastSuccessBodyPart = bodyPartCollider;
            LastSuccessPoint = castPoint;
            TimeLastSuccess = time;
        }
        else
        {
            LastSuccessBodyPart = null;
            LastSuccessPoint = null;
        }
    }

}