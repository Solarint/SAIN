﻿using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class Firerate : SAINWeaponInfoAbstract
    {
        public Firerate(BotOwner owner, SAINBotInfo info) : base(owner, info) { }

        public void Update()
        {
            var type = BotOwner.WeaponManager.WeaponAIPreset.WeaponAIPresetType;
            if (type == EWeaponAIPresetType.MainWeaponAuto || type == EWeaponAIPresetType.MainWeaponSingle)
            {
            }
            else
            {

                BotOwner.WeaponManager.WeaponAIPreset.HoldTriggerUpTime = new Func<float>(SemiAutoROF);
            }
        }

        public float SemiAutoROF()
        {
            float minTime = 0.1f; // minimum time per shot
            float maxTime = 4f; // maximum time per shot
            float EnemyDistance = (BotOwner.AimingData.RealTargetPoint - BotOwner.WeaponRoot.position).magnitude;
            float permeter = EnemyDistance / (PerMeter / WeaponInfo.FinalModifier);
            float final = Mathf.Clamp(permeter, minTime, maxTime);

            // Sets a different time between shots if a weapon is full auto or burst and the enemy isn't close
            if ((CurrentWeapon.SelectedFireMode == Weapon.EFireMode.fullauto || CurrentWeapon.SelectedFireMode == Weapon.EFireMode.burst))
            {
                final = Mathf.Clamp(final, 0.2f, 3f);
            }

            // Final Result which is randomized +- 15%
            float finalTime = final * Random.Range(0.85f, 1.15f);

            return finalTime;
        }

        public float PerMeter
        {
            get
            {
                float perMeter;
                // Higher is faster firerate // Selects a the time for 1 second of wait time for every x meters
                switch (WeaponClass)
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
                        if (AmmoCaliber == "9x39")
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
