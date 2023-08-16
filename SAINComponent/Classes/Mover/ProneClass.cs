using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.SubComponents.CoverFinder;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class ProneClass : SAINBase, ISAINClass
    {
        public ProneClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        static ProneClass()
        {
            BotLayProperty = AccessTools.Property(typeof(BotOwner), "BotLay").PropertyType.GetProperty("IsLay");
        }

        private static readonly PropertyInfo BotLayProperty;

        public bool IsProne => BotOwner.BotLay.IsLay;

        public void SetProne(bool value)
        {
            BotLayProperty.SetValue(BotLay, value);
        }

        public bool ShallProne(CoverPoint point, bool withShoot)
        {
            var status = point.CoverStatus;
            if (status == CoverStatus.FarFromCover || status == CoverStatus.None)
            {
                if (Player.MovementContext.CanProne)
                {
                    var enemy = SAIN.Enemy;
                    if (enemy != null)
                    {
                        float distance = (enemy.EnemyPosition - SAIN.Transform.Position).magnitude;
                        if (distance > 20f)
                        {
                            if (withShoot)
                            {
                                return CanShootFromProne(enemy.EnemyPosition);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool ShallProne(bool withShoot, float mindist = 30f)
        {
            if (Player.MovementContext.CanProne)
            {
                var enemy = SAIN.Enemy;
                if (enemy != null)
                {
                    float distance = (enemy.EnemyPosition - SAIN.Transform.Position).magnitude;
                    if (distance > mindist)
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

        public bool ShallProneHide(float mindist = 30f)
        {
            if (Player.MovementContext.CanProne)
            {
                var enemy = SAIN.Enemy;
                if (enemy != null)
                {
                    float distance = (enemy.EnemyPosition - SAIN.Transform.Position).magnitude;
                    if (distance > mindist)
                    {
                        return !CanShootFromProne(enemy.EnemyPosition);
                    }
                }
            }
            return false;
        }

        public bool ShallGetUp(float mindist = 30f)
        {
            if (BotLay.IsLay)
            {
                var enemy = SAIN.Enemy;
                if (enemy == null)
                {
                    return true;
                }
                float distance = (enemy.EnemyPosition - SAIN.Transform.Position).magnitude;
                if (distance > mindist)
                {
                    return !IsChestPosVisible(enemy.EnemyHeadPosition);
                }
            }
            return false;
        }

        public bool IsChestPosVisible(Vector3 enemyHeadPos)
        {
            Vector3 botPos = SAIN.Transform.Position;
            botPos += Vector3.up * 1f;
            Vector3 direction = botPos - enemyHeadPos;
            return !Physics.Raycast(enemyHeadPos, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public bool CanShootFromProne(Vector3 target)
        {
            Vector3 vector = SAIN.Transform.Position + Vector3.up * 0.14f;
            Vector3 vector2 = target + Vector3.up - vector;
            Vector3 from = vector2;
            from.y = vector.y;
            float num = Vector3.Angle(from, vector2);
            float lay_DOWN_ANG_SHOOT = HelpersGClass.LAY_DOWN_ANG_SHOOT;
            return num <= Mathf.Abs(lay_DOWN_ANG_SHOOT) && Vector.CanShootToTarget(new ShootPointClass(target, 1f), vector, BotOwner.LookSensor.Mask, true);
        }

        public BotLayClass BotLay => BotOwner.BotLay;
    }
}
