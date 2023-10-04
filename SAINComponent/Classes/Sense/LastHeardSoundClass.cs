using EFT;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class LastHeardSound
    {
        public LastHeardSound(IPlayer enemy, Vector3 pos, AISoundType type, float power)
        {
            Enemy = enemy;
            Position = pos;
            SoundType = type;
            HeardTime = Time.time;
            SoundPower = power;
        }

        public bool IsClose(Vector3 botPos)
        {
            float dist = (botPos - Position).magnitude;
            return SoundType == AISoundType.step ? dist < 30f : dist < 80f;
        }

        public float SoundPower { get; private set; }
        public IPlayer Enemy { get; private set; }
        public Vector3 Position { get; private set; }
        public float HeardTime { get; private set; }
        public AISoundType SoundType { get; private set; }
        public float TimeSinceHeard => Time.time - HeardTime;
    }
}
