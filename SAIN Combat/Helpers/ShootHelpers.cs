using EFT;
using EFT.InventoryLogic;
using Combat.Components;
using UnityEngine;
using static Combat.UserSettings.FullAutoConfig;
using static Combat.UserSettings.RecoilScatterConfig;
using static Combat.UserSettings.SemiAutoConfig;

namespace Combat.Helpers
{
    public class Shoot
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
                modifier = weaponinfo.FinalModifier;
            }

            float k = 0.08f * modifier; // How fast for the burst length to falloff with distance
            float scaledDistance = InverseScaleWithLogisticFunction(distance, k, 20f);

            scaledDistance = Mathf.Clamp(scaledDistance, 0.001f, 1f);

            if (distance > 80f) scaledDistance = 0.001f;
            else if (distance < 8f) scaledDistance = 1f;

            return scaledDistance * BurstLengthModifier.Value;
        }

        public static Vector3 Recoil(Vector3 targetpoint, float horizrecoil, float vertrecoil, float modifier, float distance)
        {
            // Reduces scatter recoil at very close range. Clamps distance between 3 and 20 then scale to 0.25 to 1.
            // So if a target is 3m or less distance, their recoil scaling will be 25% its original value
            distance = Mathf.Clamp(distance, 3f, 20f);
            distance = distance / 20f;
            distance = distance * 0.75f + 0.25f;

            float weaponhorizrecoil = (horizrecoil / 300f) * modifier;
            float weaponvertrecoil = (vertrecoil / 300f) * modifier;

            float horizRecoil = (1f * weaponhorizrecoil + AddRecoil.Value) * distance;
            float vertRecoil = (1f * weaponvertrecoil + AddRecoil.Value) * distance;

            float maxrecoil = MaxScatter.Value * distance;

            float randomHorizRecoil = UnityEngine.Random.Range(-horizRecoil, horizRecoil);
            float randomvertRecoil = UnityEngine.Random.Range(-vertRecoil, vertRecoil);

            Vector3 vector = new Vector3(targetpoint.x + randomHorizRecoil, targetpoint.y + randomvertRecoil, targetpoint.z + randomHorizRecoil);
            Vector3 clamped = new Vector3(Mathf.Clamp(vector.x, -maxrecoil, maxrecoil), Mathf.Clamp(vector.y, -maxrecoil, maxrecoil), Mathf.Clamp(vector.z, -maxrecoil, maxrecoil));

            return clamped;
        }

        public static float FullAutoTimePerShot(int bFirerate)
        {
            float roundspersecond = bFirerate / 60;

            float secondsPerShot = 1f / roundspersecond;

            return secondsPerShot;
        }

        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return (float)System.Math.Round(scaledValue, 3);
        }
    }
}