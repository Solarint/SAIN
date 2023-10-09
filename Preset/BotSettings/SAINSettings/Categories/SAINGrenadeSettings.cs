
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINGrenadeSettings
    {
        [NameAndDescription(
            "Can Throw at Visible Enemies",
            "Toggles bots throwing grenades directly at enemies they can see.")]
        [Default(false)]
        public bool CAN_THROW_STRAIGHT_CONTACT = false;

        [NameAndDescription(
            "Since Since Enemy Seen before Throw",
            "How long it has been since a bot's enemy has been visible before a bot can consider throwing a grenade.")]
        [Default(3f)]
        [MinMax(0.0f, 30f, 100f)]
        public float TimeSinceSeenBeforeThrow = 3f;

        [NameAndDescription(
            "Time Before Next Throw",
            "How much time to wait before a bot is allowed to throw another grenade.")]
        [Default(2f)]
        [MinMax(0.01f, 30f, 100f)]
        public float DELTA_NEXT_ATTEMPT = 2f;

        [NameAndDescription(
            "Minimum Friendly Distance to Target",
            "How close a friendly bot can be in Meters to a bot's grenade target before it stops them from throwing it.")]
        [Default(8f)]
        [MinMax(0.01f, 30f, 100f)]
        [CopyValue]
        public float MIN_DIST_NOT_TO_THROW = 8f;

        [NameAndDescription(
            "Grenade Spread",
            "How much distance, in meters, to randomize a bot's throw target position.")]
        [Default(0.5f)]
        [MinMax(0f, 5f, 100f)]
        [CopyValue]
        public float GrenadePrecision = 0.5f;

        [NameAndDescription(
            "Minimum Target Distance",
            "Minimum distance an enemy can be for them to consider throwing a grenade.")]
        [Default(8f)]
        [MinMax(0.0f, 40f, 10f)]
        public float MIN_THROW_GRENADE_DIST = 8f;

        [Hidden]
        public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

        [Hidden]
        public float DELTA_GRENADE_START_TIME = 0.0f;

        [Hidden]
        public int BEWARE_TYPE = 3;
    }
}