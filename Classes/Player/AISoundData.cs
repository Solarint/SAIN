using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public struct AISoundData(SoundEvent InSound, BotComponent InBot, float InPlayerDistance, Enemy InEnemy)
{
    public bool Reported = false;
    public readonly SoundEvent Sound = InSound;
    public readonly BotComponent Bot = InBot;
    public readonly Enemy Enemy = InEnemy;
    public readonly float PlayerDistance = InPlayerDistance;
    public readonly float SoundTravelTime = InPlayerDistance / InSound.SoundSpeed;
    public readonly Player HeardPlayer => Sound.GetPlayer();
    public readonly PlayerComponent HeardPlayerComponent => Sound.PlayerComponent;
    public readonly SAINSoundType SoundType => Sound.SoundType;
    public readonly bool IsGunShot => Sound.IsGunShot;
    public readonly string HeardProfileId => Sound.ProfileId;
    public readonly bool IsAI => Sound.IsAI;
    public readonly int EnvironmentId => Sound.EnvironmentId;
    public Vector3 Position => Sound.Position;

    public readonly bool CanReport(float ReactionDelay) => Sound.IsValid() && Time.time - Sound.TimeCreated >= SoundTravelTime + ReactionDelay;
}