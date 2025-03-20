using EFT;
using SAIN.Components;
using System.Collections;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINFriendlyFireClass : BotBase, IBotClass
    {
        public bool ClearShot => FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
        public FriendlyFireStatus FriendlyFireStatus { get; private set; }

        public SAINFriendlyFireClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
        }

        public void Update()
        {
            if (FriendlyFireStatus == FriendlyFireStatus.FriendlyBlock)
            {
                StopShooting();
            }
        }

        public void Dispose()
        {
        }

        public bool CheckFriendlyFire(Vector3? target = null)
        {
            FriendlyFireStatus = checkFriendlyFire(target);
            return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
        }

        private FriendlyFireStatus checkFriendlyFire(Vector3? target = null)
        {
            var members = Bot.Squad?.Members;
            if (members == null || members.Count <= 1)
            {
                return FriendlyFireStatus.None;
            }

            if (target != null)
            {
                return checkFriendlyFire(target.Value);
            }

            var aimData = BotOwner?.AimingManager.CurrentAiming;
            if (aimData == null)
            {
                return FriendlyFireStatus.None;
            }


            FriendlyFireStatus friendlyFire = checkFriendlyFire(aimData.RealTargetPoint);
            if (friendlyFire != FriendlyFireStatus.FriendlyBlock)
            {
                friendlyFire = checkFriendlyFire(aimData.EndTargetPoint);
            }

            return friendlyFire;
        }

        private FriendlyFireStatus checkFriendlyFire(Vector3 target)
        {
            var hits = sphereCastAll(target);
            int count = hits.Length;
            if (count == 0)
            {
                return FriendlyFireStatus.None;
            }

            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.collider == null)
                    continue;

                Player player = GameWorldComponent.Instance.GameWorld.GetPlayerByCollider(hit.collider);
                if (player == null)
                    continue;
                if (player.ProfileId == Bot.ProfileId)
                    continue;

                if (!Bot.EnemyController.IsPlayerAnEnemy(player.ProfileId))
                    return FriendlyFireStatus.FriendlyBlock;
            }
            return FriendlyFireStatus.Clear;
        }

        private Collider sphereCast(Vector3 target)
        {
            Vector3 firePort = Bot.Transform.WeaponFirePort;
            float distance = (target - firePort).magnitude + 1;
            float sphereCastRadius = 0.2f;

            var hits = Physics.SphereCastAll(firePort, sphereCastRadius, Bot.Transform.WeaponPointDirection, distance, LayerMaskClass.PlayerMask);

            Physics.SphereCast(firePort, sphereCastRadius, Bot.Transform.WeaponPointDirection, out var hit, distance, LayerMaskClass.PlayerMask);
            return hit.collider;
        }

        private RaycastHit[] sphereCastAll(Vector3 target)
        {
            Vector3 firePort = Bot.Transform.WeaponFirePort;
            float distance = (target - firePort).magnitude + 1;
            float sphereCastRadius = 0.2f;
            var hits = Physics.SphereCastAll(firePort, sphereCastRadius, Bot.Transform.WeaponPointDirection, distance, LayerMaskClass.PlayerMask);
            return hits;
        }

        private bool isColliderEnemy(Collider collider)
        {
            if (collider == null)
                return false;

            Player player = GameWorldComponent.Instance.GameWorld.GetPlayerByCollider(collider);
            return player != null && Bot.EnemyController.IsPlayerAnEnemy(player.ProfileId);
        }

        public void StopShooting()
        {
            BotOwner.ShootData?.EndShoot();
        }

        private const float FRIENDLYFIRE_FREQUENCY = 1f / FRIENDLYFIRE_FPS;
        private const float FRIENDLYFIRE_FPS = 30f;
    }
}