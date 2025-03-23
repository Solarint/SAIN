using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class HearingBulletAnalysisClass : BotSubClass<SAINHearingSensorClass>, IBotClass
    {
        private const float BULLET_FEEL_DOT_THRESHOLD = 0.75f;
        private const float BULLET_FEEL_MAX_DIST = 600f;

        public HearingBulletAnalysisClass(SAINHearingSensorClass hearing) : base(hearing)
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

        public bool DoIFeelBullet(BotSound sound)
        {
            if (!sound.Info.IsGunShot) {
                return false;
            }
            if (sound.Distance > BULLET_FEEL_MAX_DIST) {
                return false;
            }
            Vector3 directionToBot = (Bot.Position - sound.Info.Position).normalized;
            float dot = Vector3.Dot(sound.Info.SourcePlayer.LookDirection, directionToBot);
            return dot >= BULLET_FEEL_DOT_THRESHOLD;
        }

        public bool DidShotFlyByMe(BotSound sound)
        {
            if (!sound.BulletData.BulletFelt) {
                return false;
            }
            if (BaseClass.SoundInput.IgnoreUnderFire) {
                return false;
            }

            var player = sound.Info.SourcePlayer;
            Vector3 projectionPoint = calcProjectionPoint(player, sound.Distance);
            float pointDistanceSqr = (projectionPoint - Bot.Position).sqrMagnitude;

            sound.BulletData.ProjectionPoint = projectionPoint;

            float maxDist = SAINPlugin.LoadedPreset.GlobalSettings.Mind.SUPP_DISTANCE_SCALE_END;
            float maxDistSqr = maxDist * maxDist;
            if (pointDistanceSqr > maxDistSqr) {
                return false;
            }

            // if the direction the player shot hits a wall, and the point that they hit is further than our input max distance, the shot did not fly by the bot.
            Vector3 firePort = player.Transform.WeaponFirePort;
            Vector3 direction = projectionPoint - firePort;
            if (Physics.Raycast(firePort, direction, out var hit, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask) &&
                (hit.point - Bot.Position).sqrMagnitude > maxDistSqr) {
                return false;
            }

            if (SAINPlugin.DebugSettings.Logs.DebugHearing) {
                DebugGizmos.Sphere(projectionPoint, 0.25f, Color.red, true, 60f);
                DebugGizmos.Line(projectionPoint, firePort, Color.red, 0.1f, true, 60f, true);
            }

            sound.BulletData.ProjectionPointDistance = Mathf.Sqrt(pointDistanceSqr);
            return true;
        }

        private Vector3 calcProjectionPoint(PlayerComponent playerComponent, float realDistance)
        {
            Vector3 weaponPointDir = playerComponent.Transform.WeaponPointDirection;
            Vector3 shotPos = playerComponent.Transform.WeaponFirePort;
            Vector3 projectionPoint = shotPos + (weaponPointDir * realDistance);
            return projectionPoint;
        }
    }
}