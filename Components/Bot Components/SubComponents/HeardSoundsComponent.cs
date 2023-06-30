using BepInEx.Logging;
using EFT;
using SAIN.Classes.Mover;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UISoundsWrapper;

namespace SAIN.Classes
{
    public class SoundsController : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private void Update()
        {
            UpdateSoundsList();
        }

        private void UpdateSoundsList()
        {
            if (CheckSoundsLimiter < Time.time)
            {
                CheckSoundsLimiter = Time.time + 1f;
                ClearOld(VisibleSounds, 5f);
                ClearOld(HeardSounds, 30f);
            }
            NewestSound = Newest(HeardSounds);
            NewestVisibleSound = Newest(VisibleSounds);
        }

        private HeardSound Newest(List<HeardSound> sounds)
        {
            return sounds.Count > 0 ? sounds[sounds.Count - 1] : null;
        }

        private void ClearOld(List<HeardSound> sounds, float timeSinceCreated)
        {
            for (int i = sounds.Count - 1; i >= 0; i--)
            {
                if (sounds[i].TimeSinceCreated > timeSinceCreated || sounds[i].Visited)
                {
                    sounds.RemoveAt(i);
                }
            }
        }

        private float CheckSoundsLimiter;
        public HeardSound NewestVisibleSound { get; private set; }
        public HeardSound NewestSound { get; private set; }

        public void AddSound(Vector3 position, AISoundType soundType, IAIDetails createdPerson)
        {
            HeardSound sound = new HeardSound(BotOwner, position, soundType, createdPerson);
            HeardSounds.Add(sound);
            if (sound.VisibleSource)
            {
                VisibleSounds.Add(sound);
            }
        }

        public List<HeardSound> VisibleSounds { get; } = new List<HeardSound>();
        public List<HeardSound> HeardSounds { get; } = new List<HeardSound>();

        protected ManualLogSource Logger;
    }

    public class HeardSound
    {
        public HeardSound(BotOwner bot, Vector3 position, AISoundType soundType, IAIDetails createdPerson)
        {
            Id = Guid.NewGuid().ToString();
            BotOwner = bot;
            SourcePosition = position;
            GunSound = soundType == AISoundType.gun || soundType == AISoundType.silencedGun;
            TimeCreated = Time.time;
            PersonCreated = createdPerson;
            IsSoundVisible();
        }

        public bool IsSoundVisible()
        {
            if (VisCheckLimiter < Time.time)
            {
                VisCheckLimiter = Time.time + 0.5f;
                Vector3 start = BotOwner.LookSensor._headPoint;
                Vector3 direction = SourcePosition - start;
                float rayDist = Mathf.Clamp(direction.magnitude, 0f, 100f);
                VisibleSource = !Physics.SphereCast(new Ray(start, direction), 0.05f, rayDist, LayerMaskClass.HighPolyWithTerrainMask);
            }
            return VisibleSource;
        }

        private float VisCheckLimiter;
        private BotOwner BotOwner;

        public string Id { get; private set; }
        public IAIDetails PersonCreated { get; private set; }
        public float TimeSinceCreated => Time.time - TimeCreated;
        public float TimeCreated { get; private set; }
        public bool GunSound { get; private set; }
        public bool VisibleSource { get; private set; }
        public Vector3 SourcePosition { get; private set; }
        public bool Visited => (BotOwner.Position - SourcePosition).sqrMagnitude < 4f && TimeSinceCreated > 1f;
    }
}