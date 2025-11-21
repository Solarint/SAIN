using SAIN.Components.PlayerComponentSpace;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyPartsClass
{
    public EnemyPartsClass(PlayerComponent enemyPlayerComp)
    {
        CreatePartDatas(enemyPlayerComp);
        PartsArray = [.. Parts.Values];
    }

    public bool CanBeSeen { get; private set; }

    public bool LineOfSight { get; private set; }

    public bool CanShoot { get; private set; }

    public Dictionary<EBodyPart, EnemyPartDataClass> Parts { get; } = [];

    public EnemyPartDataClass[] PartsArray { get; private set; }

    public void Update(float currentTime)
    {
        CanBeSeen = false;
        LineOfSight = false;
        CanShoot = false;
        foreach (var part in PartsArray)
        {
            part.Update(currentTime);
            if (!CanShoot && part.CanShoot) CanShoot = true;
            if (!LineOfSight && part.LineOfSight) LineOfSight = true;
            if (!CanBeSeen && part.CanBeSeen) CanBeSeen = true;
        }
    }

    private void CreatePartDatas(PlayerComponent enemyPlayer)
    {
        var parts = enemyPlayer.BodyParts.Parts;
        foreach (var bodyPart in parts)
        {
            Parts.Add(bodyPart.Key, new EnemyPartDataClass(bodyPart.Key, bodyPart.Value.Transform, bodyPart.Value.Colliders));
        }
    }
}