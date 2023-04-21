using EFT;
using EFT.InventoryLogic;
using SAIN.Combat.Components;
using UnityEngine;
using static SAIN.Combat.Configs.Firerate;
using static SAIN.Combat.Configs.FullAutoConfig;
using static SAIN.Combat.Configs.AimingConfig;

namespace SAIN.Combat.Helpers
{
    public class ShootPatchHelpers
    {
        public static float SemiAutoROF(float EnemyDistance, float permeter, Weapon.EFireMode firemode)
        {
            float minTime = 0.125f; // minimum time per shot
            float maxTime = 4f; // maximum time per shot

            float final = Mathf.Clamp(EnemyDistance / permeter, minTime, maxTime);

            // Sets a different time between shots if a weapon is full auto or burst and the enemy isn't close
            if ((firemode == Weapon.EFireMode.fullauto || firemode == Weapon.EFireMode.burst) && EnemyDistance > 30f)
            {
                final = Mathf.Clamp(final, 0.75f, 3f);
            }

            // Final Result which is randomized +- 15%
            float finalTime = final * UnityEngine.Random.Range(0.85f, 1.15f) / RateofFire.Value;

            return finalTime;
        }
        public static float FullAutoLength(BotOwner bot, float distance)
        {
            // Enables advanced modifiers if simple mode is off
            float modifier = 1f;
            if (!SimpleModeBurst.Value)
            {
                WeaponInfo weaponinfo = bot.gameObject.GetComponent<WeaponInfo>();
                modifier = weaponinfo.ShootModifier;
            }

            float k = 0.08f * modifier; // How fast for the burst length to falloff with distance
            float scaledDistance = InverseScaleWithLogisticFunction(distance, k, 20f);

            scaledDistance = Mathf.Clamp(scaledDistance, 0.001f, 1f);

            if (distance > 80f) scaledDistance = 0.001f;
            else if (distance < 8f) scaledDistance = 1f;

            return scaledDistance * BurstLengthModifier.Value;
        }
        public static Vector3 Recoil(Vector3 targetpoint, float recoiltotal, float modifier)
        {
            float vertRecoil = 0.5f * modifier + AddRecoil.Value;
            float horizRecoil = 0.5f * modifier + AddRecoil.Value;
            float maxrecoil = MaxScatter.Value;

            float randomHorizRecoil = UnityEngine.Random.Range(-horizRecoil, horizRecoil);
            float randomvertRecoil = UnityEngine.Random.Range(-vertRecoil, vertRecoil);

            targetpoint = Vector3.Lerp(Vector3.zero, targetpoint, 0.66f);
            Vector3 vector = new Vector3(targetpoint.x + randomHorizRecoil, targetpoint.y + randomvertRecoil, targetpoint.z + randomHorizRecoil);
            Vector3 clamped = new Vector3(Mathf.Clamp(vector.x, -maxrecoil, maxrecoil), Mathf.Clamp(vector.y, -maxrecoil, maxrecoil), Mathf.Clamp(vector.z, -maxrecoil, maxrecoil));

            return clamped;
        }

        public static float FullAutoROF(int bFirerate)
        {
            float roundspersecond = bFirerate / 60f;

            float roundspermilsec = roundspersecond / 1000f;

            return roundspermilsec;
        }
        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return (float)System.Math.Round(scaledValue, 3);
        }
    }
}