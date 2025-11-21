using EFT;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyPartDataClass
{
    public Dictionary<ERaycastCheck, RaycastResult> RaycastResults { get; private set; } = [];
    public float TimeSeen { get; private set; }

    public bool CanBeSeen { get; private set; }
    public bool LineOfSight { get; private set; }
    public bool CanShoot { get; private set; }

    private readonly Dictionary<EBodyPartColliderType, BodyPartCollider> _colliderDictionary = [];

    public EnemyPartDataClass(EBodyPart bodyPart, BifacialTransform transform, List<BodyPartCollider> colliders)
    {
        BodyPart = bodyPart;
        Transform = transform;
        Colliders = colliders;
        _indexMax = colliders.Count - 1;

        foreach (BodyPartCollider collider in colliders)
        {
            if (!_colliderDictionary.ContainsKey(collider.BodyPartColliderType))
            {
                _colliderDictionary.Add(collider.BodyPartColliderType, collider);
            }
        }

        RaycastResults.Add(ERaycastCheck.LineofSight, new RaycastResult());
        RaycastResults.Add(ERaycastCheck.Shoot, new RaycastResult());
        RaycastResults.Add(ERaycastCheck.Vision, new RaycastResult());
    }

    public void Update(float currentTime)
    {
        const float SUCCESS_PERIOD = 0.25f;
        float lineOfSightSuccessTime = RaycastResults[ERaycastCheck.LineofSight].TimeLastSuccess;
        LineOfSight = currentTime - lineOfSightSuccessTime <= SUCCESS_PERIOD;
        float shootSuccessTime = RaycastResults[ERaycastCheck.Shoot].TimeLastSuccess;
        CanShoot = currentTime - shootSuccessTime <= SUCCESS_PERIOD;
        if (!LineOfSight)
        {
            CanBeSeen = false;
            TimeSeen = -1f;
            return;
        }
        float visionSuccessTime = RaycastResults[ERaycastCheck.Vision].TimeLastSuccess;
        CanBeSeen = currentTime - visionSuccessTime <= SUCCESS_PERIOD;
        if (!CanBeSeen)
        {
            TimeSeen = -1f;
            return;
        }
        if (TimeSeen <= 0f) TimeSeen = Time.time;
    }

    public void SetLineOfSight(Vector3 castPoint, EBodyPartColliderType colliderType, RaycastHit raycastHit, ERaycastCheck type, float time)
    {
        RaycastResults[type].Update(castPoint, _colliderDictionary[colliderType], raycastHit, time);
    }

    public SAINBodyPartRaycast GetRaycast()
    {
        BodyPartCollider collider = GetCollider();
      
        return new SAINBodyPartRaycast
        {
            CastPoint = GetCastPoint(collider),
            PartType = BodyPart,
            ColliderType = collider.BodyPartColliderType
        };
    }

    public readonly EBodyPart BodyPart;
    public readonly List<BodyPartCollider> Colliders;
    public readonly BifacialTransform Transform;

    private BodyPartCollider GetCollider()
    {
        BodyPartCollider collider = Colliders[_index];
        _index++;
        if (_index > _indexMax)
        {
            _index = 0;
        }
        return collider;
    }

    private int _index;
    private readonly int _indexMax;
  
    private Vector3 GetCastPoint(BodyPartCollider collider)
    {
        float size = GetColliderMinSize(collider);
        //Logger.LogInfo(size);
        Vector3 random = UnityEngine.Random.insideUnitSphere * size;
        Vector3 result = collider.Collider.ClosestPoint(collider.transform.position + random);
        return result;
    }

    private float GetColliderMinSize(BodyPartCollider collider)
    {
        if (collider.Collider == null)
        {
            return 0f;
        }
        Vector3 bounds = collider.Collider.bounds.size;
        float lowest = bounds.x;
        if (bounds.y < lowest)
        {
            lowest = bounds.y;
        }
        if (bounds.z < lowest)
        {
            lowest = bounds.z;
        }
        return lowest;
    }
}