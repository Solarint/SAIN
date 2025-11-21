using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info;

public class BotProfile : BotBase
{
    public BotProfile(BotComponent sain) : base(sain)
    {
        Name = sain.BotOwner.name;

        var profile = sain.BotOwner.Profile;
        Side = profile.Side;
        WildSpawnType = profile.Info.Settings.Role;
        BotDifficulty = profile.Info.Settings.BotDifficulty;
        PlayerLevel = profile.Info.Level;

        IsBoss = WildSpawnType.IsBoss();
        IsFollower = WildSpawnType.IsFollower();
        WildSpawnType.IsSectant();
        IsScav = EnumValues.WildSpawn.IsScav(WildSpawnType);
        IsPMC = WildSpawnType.IsPmcBot();
        IsPlayerScav = IsScav && SAINEnableClass.IsPlayerScav(profile);
        SetDiffModifier(BotDifficulty);
    }

    public float DifficultyModifier { get; private set; }
    public float DifficultyModifierSqrt { get; private set; }
    public float PowerLevel => BotOwner.AIData.PowerOfEquipment;

    public readonly string Name;
    public readonly string NickName;
    public readonly bool IsBoss;
    public readonly bool IsFollower;
    public readonly bool IsScav;
    public readonly bool IsPMC;
    public readonly bool IsPlayerScav;
    public readonly BotDifficulty BotDifficulty;
    public readonly WildSpawnType WildSpawnType;
    public readonly EPlayerSide Side;
    public readonly int PlayerLevel;

    private void SetDiffModifier(BotDifficulty difficulty)
    {
        float modifier = 1f;

        var sainSettings = SAINPlugin.LoadedPreset.BotSettings.SAINSettings;
        if (sainSettings.ContainsKey(WildSpawnType))
        {
            modifier = sainSettings[WildSpawnType].DifficultyModifier;
        }

        switch (difficulty)
        {
            case BotDifficulty.easy:
                modifier *= 0.5f;
                break;

            case BotDifficulty.normal:
                modifier *= 1.0f;
                break;

            case BotDifficulty.hard:
                modifier *= 1.5f;
                break;

            case BotDifficulty.impossible:
                modifier *= 1.75f;
                break;

            default:
                break;
        }

        DifficultyModifier = modifier.Round100();
        DifficultyModifierSqrt = Mathf.Sqrt(modifier).Round100();
    }
}