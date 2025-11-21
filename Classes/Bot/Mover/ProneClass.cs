using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Reflection;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class ProneClass : BotBase
{
    private float _nextChangeProneTime { get; set; }
    private bool _canshoot { get; set; }
    private float _nextCheckShootTime { get; set; }

    public ProneClass(BotComponent sain) : base(sain)
    {
    }

    public void SetProne(bool value)
    {
        BotOwner.BotLay.IsLay = value;
    }

    public bool ShallProne(bool withShoot, float mindist = 25f)
    {
        if (!Bot.Info.FileSettings.Move.PRONE_TOGGLE || !GlobalSettingsClass.Instance.Move.PRONE_TOGGLE)
        {
            return false;
        }
        if (Player.MovementContext.CanProne)
        {
            var enemy = Bot.GoalEnemy;
            if (enemy != null)
            {
                float distance = (enemy.EnemyPosition - Bot.Position).sqrMagnitude;
                if (distance > mindist * mindist)
                {
                    if (withShoot)
                    {
                        return CanShootFromProne(enemy.EnemyPosition);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    public bool ShallProneHide(Enemy enemy, float mindist = 10f)
    {
        if (enemy == null)
        {
            return false;
        }
        if (!Bot.Info.FileSettings.Move.PRONE_TOGGLE || !GlobalSettingsClass.Instance.Move.PRONE_TOGGLE)
        {
            return false;
        }
        if (_nextChangeProneTime > Time.time)
        {
            return Player.IsInPronePose;
        }

        if (Bot.Decision.CurrentSelfDecision == ESelfActionType.None)
        {
            return false;
        }

        if (!Player.MovementContext.CanProne)
        {
            return false;
        }
        if (enemy.KnownPlaces.BotDistanceFromLastKnown < mindist)
        {
            return false;
        }
        if (enemy.IsVisible)
        {
            return true;
        }
        return Player.IsInPronePose;


        //Vector3? lastKnownPos = enemy.LastKnownPosition;
        //if (lastKnownPos == null)
        //{
        //    return false;
        //}
       //// bool isUnderDuress = Bot.Decision.CurrentSelfDecision != ESelfActionType.None || Bot.Suppression.IsHeavySuppressed;
        //bool isUnderDuress = false;
        //bool shallProne = isUnderDuress || !CheckShootProne(lastKnownPos.Value, enemy);
        //if (shallProne)
        //{
        //    _nextChangeProneTime = Time.time + 3f;
        //}
        //return shallProne;
    }

    private bool CheckShootProne(Vector3? lastKnownPos, Enemy enemy)
    {
        if (enemy.GetVisibilePathPoint(out Vector3 point))
        {
            return CanShootFromProne(point);
        }
        else
        {
            return CanShootFromProne(lastKnownPos.Value);
        }
    }

    public bool CanShootFromProne(Vector3 target)
    {
        Vector3 vector = Bot.Transform.Position + Vector3.up * 0.14f;
        Vector3 vector2 = target + Vector3.up - vector;
        Vector3 from = vector2;
        from.y = vector.y;
        float num = Vector3.Angle(from, vector2);
        float lay_DOWN_ANG_SHOOT = HelpersGClass.LAY_DOWN_ANG_SHOOT;
        return num <= Mathf.Abs(lay_DOWN_ANG_SHOOT) && Vector.CanShootToTarget(new ShootPointClass(target, 1f), vector, BotOwner.LookSensor.Mask, true);
    }
}