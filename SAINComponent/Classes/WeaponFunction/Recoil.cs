using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Recoil : SAINBase, ISAINClass
    {
        public Recoil(SAINComponentClass sain) : base(sain)
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

        public Vector3 CalculateRecoil(Vector3 targetpoint)
        {
            float distance = SAIN.DistanceToAimTarget;

            // Reduces scatter recoil at very close range. Clamps Distance between 3 and 20 then scale to 0.25 to 1.
            // So if a target is 3m or less Distance, their recoil scaling will be 25% its original value
            distance = Mathf.Clamp(distance, 3f, 20f);
            distance /= 20f;
            distance = distance * 0.75f + 0.25f;

            float weaponhorizrecoil = CalcRecoil(SAIN.Info.WeaponInfo.RecoilForceUp);
            float weaponvertrecoil = CalcRecoil(SAIN.Info.WeaponInfo.RecoilForceBack);

            float addRecoil = SAINPlugin.LoadedPreset.GlobalSettings.Shoot.AddRecoil;
            float horizRecoil = (1f * weaponhorizrecoil + addRecoil) * distance;
            float vertRecoil = (1f * weaponvertrecoil + addRecoil) * distance;

            float maxrecoil = SAINPlugin.LoadedPreset.GlobalSettings.Shoot.MaxRecoil * distance;

            float randomHorizRecoil = Random.Range(-horizRecoil, horizRecoil);
            float randomvertRecoil = Random.Range(-vertRecoil, vertRecoil);

            Vector3 vector = new Vector3(targetpoint.x + randomHorizRecoil, targetpoint.y + randomvertRecoil, targetpoint.z + randomHorizRecoil);
            vector = MathHelpers.VectorClamp(vector, -maxrecoil, maxrecoil) * RecoilMultiplier;
            return vector;
        }

        private float RecoilMultiplier => Mathf.Round(SAIN.Info.FileSettings.Shoot.RecoilMultiplier * GlobalSettings.Shoot.GlobalRecoilMultiplier * 100f) / 100f;

        float CalcRecoil(float recoilVal)
        {
            float result = recoilVal / RecoilBaseline;
            result *= SAIN.Info.WeaponInfo.FinalModifier;
            return result;
        }

        public Vector3 CalculateDecay(Vector3 oldVector, out float rate)
        {
            var mode = SAIN.Info.WeaponInfo.CurrentWeapon.SelectedFireMode;
            if (mode == EFireMode.fullauto || mode == EFireMode.burst)
            {
                rate = Time.time + (FullAutoTimePerShot / 3f);
            }
            else
            {
                rate = Time.time + (SemiAutoTimePerShot / 3f);
            }
            return Vector3.Lerp(Vector3.zero, oldVector, SAINPlugin.LoadedPreset.GlobalSettings.Shoot.RecoilDecay);
        }

        public float RecoilTimeWait
        {
            get
            {
                if (SAIN.Info.WeaponInfo.IsSetFullAuto() || SAIN.Info.WeaponInfo.IsSetBurst())
                {
                    return Time.time + FullAutoTimePerShot * 0.8f;
                }
                else
                {
                    return Time.time + SemiAutoTimePerShot * 0.8f;
                }
            }
        }

        private float FullAutoTimePerShot
        {
            get
            {
                float roundspersecond = SingleFireRate / 60;

                float secondsPerShot = 1f / roundspersecond;

                return secondsPerShot;
            }
        }

        private float SingleFireRate
        {
            get
            {
                var template = SAIN.Info.WeaponInfo.CurrentWeapon?.Template;
                if (template != null)
                {
                    return template.SingleFireRate;
                }
                return 600f;
            }
        }

        private float SemiAutoTimePerShot
        {
            get
            {
                return 1f / (SingleFireRate / 60f);
            }
        }

        private float RecoilBaseline
        {
            get
            {
                if (ModDetection.RealismLoaded)
                {
                    return 225f;
                }
                else
                {
                    return 112f;
                }
            }
        }
    }
}
