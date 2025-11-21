using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.Info;
using System.Text;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class Recoil(BotComponent sain) : BotBase(sain)
{
    public float ArmInjuryModifier => calcModFromInjury(Bot.Medical.HitReaction.LeftArmInjury) * calcModFromInjury(Bot.Medical.HitReaction.RightArmInjury);
    private static bool _debugRecoilLogs => SAINPlugin.DebugSettings.Logs.DebugRecoilCalculations;
    private static float _recoilDecayCoef => SAINPlugin.LoadedPreset.GlobalSettings.Shoot.BOT_RECOIL_DECAY_COEF;
    private bool _recoilFinished;
    private bool _armsInjured => Bot.Medical.HitReaction.ArmsInjured;
    private float RecoilMultiplier => Mathf.Round(Bot.Info.FileSettings.Shoot.RecoilMultiplier * GlobalSettings.Shoot.BOT_RECOIL_COEF * 100f) / 100f;

    public override void Init()
    {
        PlayerComponent.OnShoot += WeaponShot;
        base.Init();
    }

    public override void Dispose()
    {
        PlayerComponent.OnShoot -= WeaponShot;
        base.Dispose();
    }

    public void CalcRecoilDecay()
    {
        if (!_recoilFinished)
        {
            float decayTime = Time.fixedDeltaTime * _recoilDecayCoef;
            _currentRecoilHorizAngle = Mathf.LerpAngle(0, _currentRecoilHorizAngle, 1f - decayTime);
            _currentRecoilVertAngle = Mathf.LerpAngle(0, _currentRecoilVertAngle, 1f - decayTime);
            if (_currentRecoilHorizAngle <= 0.001f && _currentRecoilVertAngle < 0.001f)
            {
                _recoilFinished = true;
                _currentRecoilHorizAngle = 0f;
                _currentRecoilVertAngle = 0f;
            }
        }
    }

    public void WeaponShot(WeaponInfo WeaponInfo, Vector3 force)
    {
        if (Bot.IsCheater)
        {
            //Logger.LogDebug("cheato");
            return;
        }
        calculateRecoil(WeaponInfo.Weapon);
    }

    private float _currentRecoilHorizAngle;
    private float _currentRecoilVertAngle;

    public Vector3 ApplyRecoil(Vector3 lookdirection)
    {
        return Vector.Rotate(lookdirection, _currentRecoilHorizAngle, _currentRecoilVertAngle, 0f);
    }

    private void calculateRecoil(Weapon weapon)
    {
        if (weapon == null)
        {
#if DEBUG
            Logger.LogError("Weapon Null!");
#endif
            return;
        }
        
        _recoilFinished = false;
        float addRecoil = SAINPlugin.LoadedPreset.GlobalSettings.Shoot.BOT_RECOIL_ADD;
        float recoilMod = calcRecoilMod();
        float recoilTotal = weapon.RecoilTotal;
        float recoilNum = calcRecoilNum(recoilTotal) + addRecoil;
        float calcdRecoil = recoilNum * recoilMod;

        _currentRecoilVertAngle = Random.Range(calcdRecoil / 3f, calcdRecoil) * randomSign();
        _currentRecoilHorizAngle = Random.Range(calcdRecoil / 3f, calcdRecoil) * randomSign();
        
#if DEBUG
        if (_debugRecoilLogs)
            Logger.LogDebug($"Recoil! New Recoil: [{_currentRecoilVertAngle}:{_currentRecoilHorizAngle}] " +
            $"recoilNum: [{recoilNum}] calcdRecoil: [{calcdRecoil}] : " +
            $"Modifiers [ Add: [{addRecoil}] Multi: [{recoilMod}] Weapon RecoilTotal [{recoilTotal}]] Shoot Modifier: [{Bot.Info.WeaponInfo.FinalModifier}]");
#endif
    }
     
    private float randomSign()
    {
        return EFTMath.RandomBool() ? -1 : 1;
    }

    private float calcModFromInjury(EInjurySeverity severity)
    {
        switch (severity)
        {
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

        if (Player.IsInPronePose)
        {
            recoilMod *= 0.7f;
        }
        else if (Player.Pose == EPlayerPose.Duck)
        {
            recoilMod *= 0.9f;
        }

        if (BotOwner.WeaponManager?.ShootController?.IsAiming == true)
        {
            recoilMod *= 0.9f;
        }
        if (Bot.Transform.VelocityData.VelocityMagnitudeNormal < 0.1f)
        {
            recoilMod *= 0.85f;
        }
        if (_armsInjured)
        {
            recoilMod *= Mathf.Sqrt(ArmInjuryModifier);
        }

        return recoilMod;
    }

    private float calcRecoilNum(float recoilVal)
    {
        float result = recoilVal / _shootSettings.BOT_RECOIL_BASELINE;
        if (ModDetection.RealismLoaded)
        {
            result = recoilVal / _shootSettings.BOT_RECOIL_BASELINE_REALISM;
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
    private static ShootSettings _shootSettings => GlobalSettingsClass.Instance.Shoot;
}