using EFT;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Firerate : BotBase, IBotClass
    {
        public Firerate(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public float SemiAutoROF()
        {
            WeaponInfoClass weaponInfo = Bot.Info.WeaponInfo;
            if (weaponInfo == null)
            {
                return 1f;
            }

            if (Bot.IsCheater)
            {
                return 0f;
            }

            float minTime = 0.1f; // minimum time per shot
            float maxTime = 4f; // maximum time per shot
            float EnemyDistance = (BotOwner.AimingManager.CurrentAiming.RealTargetPoint - BotOwner.WeaponRoot.position).magnitude;

            float rate = EnemyDistance / (PerMeter / weaponInfo.FinalModifier);
            float final = Mathf.Clamp(rate, minTime, maxTime);

            // Sets a different time between shots if a weapon is full auto or burst and the enemy isn't close
            if (weaponInfo.IsFireModeSet(EFireMode.fullauto))
            {
                final = Mathf.Clamp(final * 0.25f, 0.001f, 1f);
            }
            if (weaponInfo.IsFireModeSet(EFireMode.burst))
            {
                final = Mathf.Clamp(final, 0.1f, 3f);
            }

            final /= Bot.Info.FileSettings.Shoot.FireratMulti;

            // Final Result which is randomized +- 15%
            float finalTime = final * Random.Range(0.85f, 1.15f);

            finalTime = Mathf.Round(finalTime * 100f) / 100f;

            //Logger.LogDebug(finalTime);

            return finalTime;
        }

        public float PerMeter
        {
            get
            {
                var perMeterDictionary = GlobalSettings?.Shoot?.WeaponPerMeter;
                var weapInfo = Bot?.Info?.WeaponInfo;

                if (perMeterDictionary != null && weapInfo != null)
                {
                    if (perMeterDictionary.TryGetValue(weapInfo.EWeaponClass, out float perMeter))
                    {
                        return perMeter;
                    }
                    if (perMeterDictionary.TryGetValue(EWeaponClass.Default, out perMeter))
                    {
                        return perMeter;
                    }
                }
                return 80f;
            }
        }
    }
}
