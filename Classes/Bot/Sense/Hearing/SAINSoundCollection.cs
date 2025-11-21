using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINSoundCollection
{
    public class SAINSound
    {
        public SAINSound()
        {
            TimeCreated = Time.time;
        }

        public readonly float TimeCreated;

        public string SourcePlayerProfileId;
        public Vector3 Position;
        public float SoundPower;
        public bool WasHeard;
        public bool BulletFelt;
        public float DistanceAtCreation;
        public bool IsTooFar;
        public bool IsTooOld;
        public bool IsCheckedByBot;
    }

    public SAINSoundCollection(IPlayer iPlayer)
    {
        IPlayer = iPlayer;
        Player = GameWorldInfo.GetAlivePlayer(iPlayer);
        TimeCreated = Time.time;

        // Randomize the cleanup time to avoid doing it in batches.
        randomizationFactor = UnityEngine.Random.Range(0.75f, 1.25f);
    }

    public void UpdatePlayer()
    {
        if (getPlayerTimer < Time.time && Player == null && IPlayer != null)
        {
            getPlayerTimer = Time.time + 1f;
            Player = GameWorldInfo.GetAlivePlayer(IPlayer);
            if (Player == null)
            {
                //Logger.LogError("Player Null!");
            }
        }
    }

    private float getPlayerTimer;

    public IPlayer IPlayer { get; private set; }
    public int Count => SoundList.Count;
    public Player Player { get; private set; }
    public List<SAINSound> SoundList { get; private set; } = new List<SAINSound>();
    public float TimeCreated { get; private set; }

    public float TimeCleanedUp { get; private set; }

    private const float expireTime = 60f;
    private const float expireDistSqr = 2500f;
    private const float cleanupFreq = 5f;
    private float randomizationFactor;

    public void Cleanup(bool force = false)
    {
        float time = Time.time;

        if (force)
        {
            SoundList.Clear();
            return;
        }

        // Randomize the cleanup time to avoid doing it in batches.
        if (TimeCleanedUp < time + cleanupFreq * randomizationFactor)
        {
            TimeCleanedUp = time;
            UpdatePlayer();

            SoundsToRemove.Clear();
            for (int i = 0; i < SoundList.Count; i++)
            {
                SAINSound sound = SoundList[i];

                if (!force && sound != null && IPlayer != null)
                {
                    if (sound.TimeCreated + expireTime < time)
                    {
                        sound.IsTooOld = true;
                    }
                    else if (Player != null)
                    {
                        float distanceSqr = (sound.Position - Player.Position).sqrMagnitude;
                        if (distanceSqr > expireDistSqr)
                        {
                            sound.IsTooFar = true;
                        }
                    }
                }

                bool remove =
                    IPlayer == null
                    || Player == null
                    || sound.IsTooOld
                    || sound.IsTooFar;

                if (remove)
                {
                    SoundsToRemove.Add(sound);
                }
            }

            if (SoundsToRemove.Count > 0)
            {
                string debugText = $"Cleaning up {SoundsToRemove.Count} sounds... ";
                bool debugOn = SAINPlugin.DebugSettings.Logs.DebugHearing;

                for (int i = 0; i < SoundsToRemove.Count; i++)
                {
                    SAINSound soundToRemove = SoundsToRemove[i];
                    if (debugOn)
                    {
                        ESoundCleanupReason reason = GetCleanupReason(soundToRemove, IPlayer, Player, force);
                        debugText += $" [ [{reason}] ]";
                    }

                    SoundList.Remove(soundToRemove);
                }
                SoundsToRemove.Clear();

                if (debugOn)
                {
                    Logger.LogDebug(debugText);
                }
            }
        }
    }

    private static ESoundCleanupReason GetCleanupReason(SAINSound sound, IPlayer iplayer, Player player, bool forced)
    {
        ESoundCleanupReason reason = ESoundCleanupReason.None;
        if (forced)
        {
            reason = ESoundCleanupReason.Forced;
        }
        else if (iplayer == null)
        {
            reason = ESoundCleanupReason.IPlayerNull;
        }
        else if (player == null)
        {
            reason = ESoundCleanupReason.PlayerNull;
        }
        else if (sound == null)
        {
            reason = ESoundCleanupReason.SoundNull;
        }
        else if (sound.IsTooOld)
        {
            reason = ESoundCleanupReason.TooOld;
        }
        else if (sound.IsTooFar)
        {
            reason = ESoundCleanupReason.TooFar;
        }
        return reason;
    }

    private readonly List<SAINSound> SoundsToRemove = new();
}