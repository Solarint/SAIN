using EFT;
using EFT.InventoryLogic;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using System.Collections;
using System.Text;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public class Recoil : BotBase, IBotClass
    {
        public Vector3 CurrentRecoilOffset { get; private set; } = Vector3.zero;

        private Vector3 _lookDir => Player.LookDirection * 3f;
        public float ArmInjuryModifier => calcModFromInjury(Bot.Medical.HitReaction.LeftArmInjury) * calcModFromInjury(Bot.Medical.HitReaction.RightArmInjury);
        private readonly StringBuilder _debugString = new StringBuilder();
        private static bool _debugRecoilLogs => SAINPlugin.DebugSettings.Logs.DebugRecoilCalculations;
        private static float _recoilDecayCoef => SAINPlugin.LoadedPreset.GlobalSettings.Shoot.RECOIL_DECAY_COEF;
        private float _barrelRecoveryTime;
        private bool _recoilFinished;
        private bool _armsInjured => Bot.Medical.HitReaction.ArmsInjured;
        private float RecoilMultiplier => Mathf.Round(Bot.Info.FileSettings.Shoot.RecoilMultiplier * GlobalSettings.Shoot.RecoilMultiplier * 100f) / 100f;

        public Recoil(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            calcDecay();
        }

        public void Dispose()
        {
        }

        private void calcDecay()
        {
            if (_recoilFinished) {
                return;
            }

            float decayTime = Time.deltaTime * _recoilDecayCoef;
            _barrelRecoveryTime += decayTime;

            if (_barrelRecoveryTime >= 1) {
                _barrelRecoveryTime = 0;
                _recoilFinished = true;
                CurrentRecoilOffset = Vector3.zero;
                return;
            }

            CurrentRecoilOffset = Vector3.Lerp(CurrentRecoilOffset, Vector3.zero, _barrelRecoveryTime);
        }

        public void WeaponShot()
        {
            if (Bot.IsCheater) {
                Logger.LogDebug("cheato");
                return;
            }

            calculateRecoil();
            _recoilFinished = false;
        }

        private void calculateRecoil()
        {
            Weapon weapon = Bot.Info?.WeaponInfo?.CurrentWeapon;
            if (weapon == null) {
                Logger.LogError("Weapon Null!");
                return;
            }

            float addRecoil = SAINPlugin.LoadedPreset.GlobalSettings.Shoot.AddRecoil;
            float recoilMod = calcRecoilMod();
            float recoilTotal = weapon.RecoilTotal;
            float recoilNum = calcRecoilNum(recoilTotal) + addRecoil;
            float calcdRecoil = recoilNum * recoilMod;

            float randomvertRecoil = Random.Range(calcdRecoil / 2f, calcdRecoil) * randomSign();
            float randomHorizRecoil = Random.Range(calcdRecoil / 2f, calcdRecoil) * randomSign();

            Vector3 dir = Bot.Transform.WeaponPointDirection;
            Vector3 result = Vector.Rotate(dir, randomHorizRecoil, randomvertRecoil, 0f);
            result -= dir;
            CurrentRecoilOffset += result;

            if (SAINPlugin.DebugSettings.Gizmos.DebugDrawRecoilGizmos) {
                DebugGizmos.Ray(Bot.Transform.WeaponFirePort, dir * BotOwner.AimingManager.CurrentAiming.LastDist2Target, Color.red, BotOwner.AimingManager.CurrentAiming.LastDist2Target, 0.02f, true, 10f);
            }

            if (_debugRecoilLogs)
                Logger.LogDebug($"Recoil! New Recoil: [{result.magnitude}] " +
                $"Current Total Recoil Magnitude: [{CurrentRecoilOffset.magnitude}] " +
                $"recoilNum: [{recoilNum}] calcdRecoil: [{calcdRecoil}] : " +
                $"Randomized Vert [{randomvertRecoil}] : Randomized Horiz [{randomHorizRecoil}] " +
                $"Modifiers [ Add: [{addRecoil}] Multi: [{recoilMod}] Weapon RecoilTotal [{recoilTotal}]] Shoot Modifier: [{Bot.Info.WeaponInfo.FinalModifier}]");
        }

        private float randomSign()
        {
            return EFTMath.RandomBool() ? -1 : 1;
        }

        private float calcModFromInjury(EInjurySeverity severity)
        {
            switch (severity) {
                default:
                    return 1f;

                case EInjurySeverity.Injury:
                    return 1.15f;

                case EInjurySeverity.HeavyInjury:
                    return 1.35f;

                case EInjurySeverity.Destroyed:
                    return 1.65f;
            }
        }

        private float calcRecoilMod()
        {
            float recoilMod = 1f * RecoilMultiplier;

            if (Player.IsInPronePose) {
                recoilMod *= 0.7f;
            }
            else if (Player.Pose == EPlayerPose.Duck) {
                recoilMod *= 0.9f;
            }

            if (BotOwner.WeaponManager?.ShootController?.IsAiming == true) {
                recoilMod *= 0.9f;
            }
            if (Bot.Transform.VelocityMagnitudeNormal < 0.1f) {
                recoilMod *= 0.85f;
            }
            if (_armsInjured) {
                recoilMod *= Mathf.Sqrt(ArmInjuryModifier);
            }

            return recoilMod;
        }

        private float calcRecoilNum(float recoilVal)
        {
            float result = recoilVal / _shootSettings.RECOIL_BASELINE;
            if (ModDetection.RealismLoaded) {
                result = recoilVal / _shootSettings.RECOIL_BASELINE_REALISM;
            }
            result *= shootModClamped();
            result *= UnityEngine.Random.Range(0.8f, 1.2f);
            return result;
        }

        private float shootModClamped()
        {
            return Mathf.Clamp(_shootModifier, 0.5f, 2f);
        }

        private float _shootModifier => Bot.Info.WeaponInfo.FinalModifier;
        private ShootSettings _shootSettings => GlobalSettingsClass.Instance.Shoot;
    }
}