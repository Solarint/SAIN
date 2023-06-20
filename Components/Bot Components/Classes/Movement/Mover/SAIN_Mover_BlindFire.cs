using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes.Mover
{
    public class SAIN_Mover_BlindFire : SAINBot
    {
        public SAIN_Mover_BlindFire(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void Update()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy == null || BlindFireTimer > Time.time)
            {
                return;
            }

            Vector3 targetPos = enemy.EnemyChestPosition;

            int blindfire = 0;

            if (!enemy.CanShoot)
            {
                if (CurrentBlindFireSetting == 0)
                {
                }
                if (RayCastCheck(BotOwner.WeaponRoot.position, targetPos))
                {
                    Vector3 rayPoint = BotOwner.LookSensor._headPoint;
                    rayPoint.y += 0.1f;

                    if (!RayCastCheck(rayPoint, targetPos))
                    {
                        blindfire = 1;
                    }
                }
            }

            if (blindfire == 1 && BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets)
            {
                SetBlindFire(blindfire);
                SAIN.Steering.LookToPoint(targetPos);
                //BotOwner.ShootData.Shoot();
            }
            else
            {
                SetBlindFire(0);
            }

            if (CurrentBlindFireSetting == 0)
            {
            }
        }

        public int CurrentBlindFireSetting { get; private set; }
        private float BlindFireTimer = 0f;

        private bool RayCastCheck(Vector3 start, Vector3 targetPos)
        {
            Vector3 direction = targetPos - start;
            float magnitude = (targetPos - start).magnitude;
            return Physics.Raycast(start, direction, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public void SetBlindFire(int value)
        {
            CurrentBlindFireSetting = value;
            BotOwner.GetPlayer.MovementContext.SetBlindFire(value);
        }

        private int GetBlindFire => BotOwner.GetPlayer.MovementContext.BlindFire;

        private readonly ManualLogSource Logger;
    }
}