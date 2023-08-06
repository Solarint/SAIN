using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Firerate : SAINBase, ISAINClass
    {
        public Firerate(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            var type = BotOwner.WeaponManager.WeaponAIPreset.WeaponAIPresetType;
            if (type == EWeaponAIPresetType.MainWeaponAuto || type == EWeaponAIPresetType.MainWeaponSingle)
            {
            }
            else
            {

                //BotOwner.WeaponManager.WeaponAIPreset.HoldTriggerUpTime = new Func<float>(SemiAutoROF);
            }
        }

        public void Dispose()
        {
        }

        public float SemiAutoROF()
        {
            float minTime = 0.1f; // minimum time per shot
            float maxTime = 4f; // maximum time per shot
            float EnemyDistance = (BotOwner.AimingData.RealTargetPoint - BotOwner.WeaponRoot.position).magnitude;

            float rate = EnemyDistance / (PerMeter / SAIN.Info.WeaponInfo.FinalModifier);
            float final = Mathf.Clamp(rate, minTime, maxTime);

            // Sets a different time between shots if a weapon is full auto or burst and the enemy isn't close
            if (SAIN.Info.WeaponInfo.IsSetFullAuto() || SAIN.Info.WeaponInfo.IsSetBurst())
            {
                final = Mathf.Clamp(final, 0.1f, 3f);
            }

            final /= SAIN.Info.FileSettings.Shoot.FireratMulti;

            // Final Result which is randomized +- 15%
            float finalTime = final * Random.Range(0.85f, 1.15f);

            finalTime = Mathf.Round(finalTime * 100f) / 100f;

            Logger.LogDebug(finalTime);

            return finalTime;
        }

        public float PerMeter
        {
            get
            {
                float perMeter;
                // Higher is faster firerate // Selects a the time for 1 second of wait time for every x meters
                switch (SAIN.Info.WeaponInfo.WeaponClass)
                {
                    case "assaultCarbine":
                    case "assaultRifle":
                    case "machinegun":
                        perMeter = 120f;
                        break;

                    case "smg":
                        perMeter = 130f;
                        break;

                    case "pistol":
                        perMeter = 90f;
                        break;

                    case "marksmanRifle":
                        perMeter = 90f;
                        break;

                    case "sniperRifle":
                        perMeter = 70f;
                        // VSS and VAL Exception
                        if (SAIN.Info.WeaponInfo.AmmoCaliber == "9x39")
                        {
                            perMeter = 120f;
                        }
                        break;

                    case "shotgun":
                    case "grenadeLauncher":
                    case "specialWeapon":
                        perMeter = 70f;
                        break;

                    default:
                        perMeter = 120f;
                        break;
                }
                return perMeter;
            }
        }
    }
}
