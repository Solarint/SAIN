using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{

    public class AmmoShootabilityClass
    {
        public AmmoShootabilityClass()
        {
        }

        public void UpdateValues()
        {
            Values = GetValuesFromClass.UpdateValues(Caliber.Default, Default, this, Values);
        }

        public float Get(string caliber)
        {
            float modifier;
            if (System.Enum.TryParse(caliber, out Caliber result))
            {
                modifier = Get(result);
            }
            else
            {
                Logger.LogError($"{caliber} could not parse");
                modifier = Default;
            }
            return modifier;
        }

        public float Get(Caliber key)
        {
            if (Values.Count == 0)
            {
                UpdateValues();
            }
            if (Values.ContainsKey(key))
            {
                return (float)Values[key];
            }
            Logger.LogWarning($"{key} does not exist in {GetType()} Dictionary");
            return Default;
        }

        public static Dictionary<object, object> Values = new Dictionary<object, object>();

        private const string Description = "Lower is BETTER. How Shootable this ammo type is, affects semi auto firerate and full auto burst length";

        [DefaultValue(0.2f)]
        [AmmoCaliber(Caliber.Caliber9x18PM)]
        [NameAndDescription(nameof(Caliber9x18PM), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber9x18PM = 0.2f;

        [DefaultValue(0.25f)]
        [AmmoCaliber(Caliber.Caliber9x19PARA)]
        [NameAndDescription(nameof(Caliber9x19PARA), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber9x19PARA = 0.25f;

        [DefaultValue(0.3f)]
        [AmmoCaliber(Caliber.Caliber46x30)]
        [NameAndDescription(nameof(Caliber46x30), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber46x30 = 0.3f;

        [DefaultValue(0.3f)]
        [AmmoCaliber(Caliber.Caliber9x21)]
        [NameAndDescription(nameof(Caliber9x21), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber9x21 = 0.3f;

        [DefaultValue(0.35f)]
        [AmmoCaliber(Caliber.Caliber57x28)]
        [NameAndDescription(nameof(Caliber57x28), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber57x28 = 0.35f;

        [DefaultValue(0.4f)]
        [AmmoCaliber(Caliber.Caliber762x25TT)]
        [NameAndDescription(nameof(Caliber762x25TT), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber762x25TT = 0.4f;

        [DefaultValue(0.4f)]
        [AmmoCaliber(Caliber.Caliber1143x23ACP)]
        [NameAndDescription(nameof(Caliber1143x23ACP), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber1143x23ACP = 0.4f;

        [DefaultValue(0.65f)]
        [AmmoCaliber(Caliber.Caliber9x33R)]
        [NameAndDescription(nameof(Caliber9x33R), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber9x33R = 0.65f;

        [DefaultValue(0.5f)]
        [AmmoCaliber(Caliber.Caliber545x39)]
        [NameAndDescription(nameof(Caliber545x39), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber545x39 = 0.5f;

        [DefaultValue(0.5f)]
        [AmmoCaliber(Caliber.Caliber556x45NATO)]
        [NameAndDescription(nameof(Caliber556x45NATO), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber556x45NATO = 0.5f;

        [DefaultValue(0.55f)]
        [AmmoCaliber(Caliber.Caliber9x39)]
        [NameAndDescription(nameof(Caliber9x39), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber9x39 = 0.55f;

        [DefaultValue(0.55f)]
        [AmmoCaliber(Caliber.Caliber762x35)]
        [NameAndDescription(nameof(Caliber762x35), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber762x35 = 0.55f;

        [DefaultValue(0.65f)]
        [AmmoCaliber(Caliber.Caliber762x39)]
        [NameAndDescription(nameof(Caliber762x39), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber762x39 = 0.65f;

        [DefaultValue(0.65f)]
        [AmmoCaliber(Caliber.Caliber366TKM)]
        [NameAndDescription(nameof(Caliber366TKM), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber366TKM = 0.65f;

        [DefaultValue(0.7f)]
        [AmmoCaliber(Caliber.Caliber762x51)]
        [NameAndDescription(nameof(Caliber762x51), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber762x51 = 0.7f;

        [DefaultValue(0.75f)]
        [AmmoCaliber(Caliber.Caliber127x55)]
        [NameAndDescription(nameof(Caliber127x55), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber127x55 = 0.75f;

        [DefaultValue(0.8f)]
        [AmmoCaliber(Caliber.Caliber762x54R)]
        [NameAndDescription(nameof(Caliber762x54R), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber762x54R = 0.8f;

        [DefaultValue(1.0f)]
        [AmmoCaliber(Caliber.Caliber86x70)]
        [NameAndDescription(nameof(Caliber86x70), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber86x70 = 1.0f;

        [DefaultValue(0.65f)]
        [AmmoCaliber(Caliber.Caliber20g)]
        [NameAndDescription(nameof(Caliber20g), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber20g = 0.65f;

        [DefaultValue(0.7f)]
        [AmmoCaliber(Caliber.Caliber12g)]
        [NameAndDescription(nameof(Caliber12g), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber12g = 0.7f;

        [DefaultValue(0.75f)]
        [AmmoCaliber(Caliber.Caliber23x75)]
        [NameAndDescription(nameof(Caliber23x75), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber23x75 = 0.75f;

        [DefaultValue(1f)]
        [AmmoCaliber(Caliber.Caliber26x75)]
        [NameAndDescription(nameof(Caliber26x75), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber26x75 = 1f;

        [DefaultValue(1f)]
        [AmmoCaliber(Caliber.Caliber30x29)]
        [NameAndDescription(nameof(Caliber30x29), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber30x29 = 1f;

        [DefaultValue(1f)]
        [AmmoCaliber(Caliber.Caliber40x46)]
        [NameAndDescription(nameof(Caliber40x46), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber40x46 = 1f;

        [DefaultValue(1f)]
        [AmmoCaliber(Caliber.Caliber40mmRU)]
        [NameAndDescription(nameof(Caliber40mmRU), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber40mmRU = 1f;

        [DefaultValue(0.25f)]
        [AmmoCaliber(Caliber.Caliber127x108)]
        [NameAndDescription(nameof(Caliber127x108), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Caliber127x108 = 0.25f;

        public static readonly float Default = 0.5f;
    }
}