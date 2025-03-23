using EFT;
using SAIN.Components;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Components.BotController;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class HearingInputClass : BotSubClass<SAINHearingSensorClass>, IBotClass
    {
        public bool IgnoreUnderFire { get; private set; }
        public bool IgnoreHearing { get; private set; }

        private const float IMPACT_HEAR_FREQUENCY = 0.5f;
        private const float IMPACT_HEAR_FREQUENCY_FAR = 0.05f;
        private const float IMPACT_MAX_HEAR_DISTANCE = 50f * 50f;
        private const float IMPACT_DISPERSION = 5f * 5f;

        public HearingInputClass(SAINHearingSensorClass hearing) : base(hearing)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
            SAINBotController.Instance.BotHearing.AISoundPlayed += soundHeard;
            SAINBotController.Instance.BotHearing.BulletImpact += bulletImpacted;
        }

        public void Update()
        {
            checkResetHearing();
        }

        private void checkResetHearing()
        {
            if (!IgnoreHearing) {
                if (IgnoreUnderFire)
                    IgnoreUnderFire = false;
                return;
            }
            if (_ignoreUntilTime > 0 &&
                _ignoreUntilTime < Time.time) {
                IgnoreHearing = false;
                IgnoreUnderFire = false;
                return;
            }
            if (Bot.EnemyController.EnemyLists.GetEnemyList(EEnemyListType.Visible)?.Count > 0) {
                IgnoreHearing = false;
                IgnoreUnderFire = false;
                return;
            }
        }

        public void Dispose()
        {
            SAINBotController.Instance.BotHearing.AISoundPlayed -= soundHeard;
            SAINBotController.Instance.BotHearing.BulletImpact -= bulletImpacted;
        }

        private void soundHeard(
            SAINSoundType soundType,
            Vector3 soundPosition,
            PlayerComponent playerComponent,
            float power,
            float volume)
        {
            if (volume <= 0 || !canHearSounds()) {
                return;
            }
            if (playerComponent.ProfileId == Bot.ProfileId) {
                return;
            }
            if (!soundListenerStarted(playerComponent)) {
                return;
            }
            bool isGunshot = soundType.IsGunShot();
            if (IgnoreHearing && !isGunshot) {
                return;
            }
            Enemy enemy = Bot.EnemyController.GetEnemy(playerComponent.ProfileId, true);
            if (enemy == null) {
                if (BotOwner.BotsGroup.IsEnemy(playerComponent.IPlayer)) {
                    enemy = Bot.EnemyController.CheckAddEnemy(playerComponent.IPlayer);
                }
                if (enemy == null) {
                    return;
                }
            }

            if (!PlayerComponent.AIData.PlayerLocation.InBunker) {
                var weather = SAINWeatherClass.Instance;
                if (weather != null) {
                    if (PlayerComponent.Player.AIData.EnvironmentId == 0) {
                        power *= weather.RainSoundModifierOutdoor;
                    }
                    else {
                        power *= weather.RainSoundModifierIndoor;
                    }
                }
            }
            float baseRange = power * volume;
            if (!isGunshot &&
                enemy.RealDistance > baseRange) {
                return;
            }

            var info = new SoundInfoData {
                SourcePlayer = playerComponent,
                IsAI = playerComponent.IsAI,
                Position = soundPosition,
                Power = power,
                Volume = volume,
                SoundType = soundType,
                IsGunShot = isGunshot
            };
            BotSound sound = new BotSound(info, enemy, baseRange);
            BaseClass.ReactToHeardSound(sound);
        }

        private void bulletImpacted(EftBulletClass bullet)
        {
            if (!canHearSounds()) {
                return;
            }
            if (_nextHearImpactTime > Time.time) {
                return;
            }
            if (Bot.HasEnemy) {
                return;
            }
            var player = bullet.Player?.iPlayer;
            if (player == null) {
                return;
            }
            var enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
            if (enemy == null) {
                return;
            }
            if (!soundListenerStarted(enemy.EnemyPlayerComponent)) {
                return;
            }
            if (Bot.PlayerComponent.AIData.PlayerLocation.InBunker != enemy.EnemyPlayerComponent.AIData.PlayerLocation.InBunker) {
                return;
            }
            float distance = (bullet.CurrentPosition - Bot.Position).sqrMagnitude;
            if (distance > IMPACT_MAX_HEAR_DISTANCE) {
                _nextHearImpactTime = Time.time + IMPACT_HEAR_FREQUENCY_FAR;
                return;
            }
            _nextHearImpactTime = Time.time + IMPACT_HEAR_FREQUENCY;

            float dispersion = distance / IMPACT_DISPERSION;
            Vector3 random = UnityEngine.Random.onUnitSphere;
            random.y = 0;
            random = random.normalized * dispersion;
            Vector3 estimatedPos = enemy.EnemyPosition + random;

            HearingReport report = new HearingReport {
                position = estimatedPos,
                soundType = SAINSoundType.BulletImpact,
                placeType = EEnemyPlaceType.Hearing,
                isDanger = distance < 25f * 25f,
                shallReportToSquad = true,
            };
            enemy.Hearing.SetHeard(report);
        }

        private bool canHearSounds()
        {
            if (!Bot.BotActive) {
                return false;
            }
            if (Bot.GameEnding) {
                return false;
            }
            return true;
        }

        private bool soundListenerStarted(PlayerComponent player)
        {
            if (!player.Person.AIInfo.IsAI) {
                return true;
            }
            if (!_hearingStarted) {
                if (!PlayerComponent.AIData.AISoundPlayer.SoundMakerStarted) {
                    return false;
                }
                _hearingStarted = true;
            }
            return true;
        }

        private bool _hearingStarted;

        public bool SetIgnoreHearingExternal(bool value, bool ignoreUnderFire, float duration, out string reason)
        {
            if (Bot.Enemy?.IsVisible == true) {
                reason = "Enemy Visible";
                return false;
            }
            if (BotOwner.Memory.IsUnderFire && !ignoreUnderFire) {
                reason = "Under Fire";
                return false;
            }

            IgnoreUnderFire = ignoreUnderFire;
            IgnoreHearing = value;
            if (value && duration > 0f) {
                _ignoreUntilTime = Time.time + duration;
            }
            else {
                _ignoreUntilTime = -1f;
            }
            reason = string.Empty;
            return true;
        }

        private float _nextHearImpactTime;
        private float _ignoreUntilTime;
    }
}