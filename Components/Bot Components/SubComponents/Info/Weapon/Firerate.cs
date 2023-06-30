using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using static SAIN.Helpers.DebugGizmos;
using static SAIN.UserSettings.ShootConfig;

namespace SAIN.Classes
{
    public class Firerate : SAINWeapon
    {
        public Firerate(BotOwner owner) : base(owner) { }

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
            float finalTime = final * Random.Range(0.85f, 1.15f) / RateofFire.Value;

            return finalTime;
        }

        public float FullAutoBurstLength(BotOwner BotOwner, float distance)
        {
            var component = BotOwner.GetComponent<SAINComponent>();

            if (component == null)
            {
                return 0.001f;
            }

            float modifier = component.Info.WeaponInfo.FinalModifier;

            float k = 0.08f * modifier; // How fast for the burst length to falloff with distance
            float scaledDistance = MathHelpers.InverseScaleWithLogisticFunction(distance, k, 20f);

            scaledDistance = Mathf.Clamp(scaledDistance, 0.001f, 1f);

            if (distance > 80f)
            {
                scaledDistance = 0.001f;
            }
            else if (distance < 8f)
            {
                scaledDistance = 1f;
            }

            return scaledDistance * BurstLengthModifier.Value;
        }

        public float FullAutoTimePerShot(int bFirerate)
        {
            float roundspersecond = bFirerate / 60;

            float secondsPerShot = 1f / roundspersecond;

            return secondsPerShot;
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
