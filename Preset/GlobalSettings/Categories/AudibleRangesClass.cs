using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class AudibleRangesClass
    {
        public AudibleRangesClass()
        {
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

        public void UpdateValues()
        {
            Values = GetValuesFromClass.UpdateValues(Caliber.Default, Default, this, Values);
        }

        [JsonIgnore]
        [AdvancedOptions(false, true)]
        public Dictionary<object, object> Values = new Dictionary<object, object>();

        [AdvancedOptions(false, true)]
        private const string Description = "The Distance this bullet caliber can be heard by AI";

        [DefaultValue(125f)]
        [AmmoCaliber(Caliber.Caliber9x18PM)]
        [NameAndDescription(nameof(Caliber9x18PM), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber9x18PM = 125f;

        [DefaultValue(125f)]
        [AmmoCaliber(Caliber.Caliber9x19PARA)]
        [NameAndDescription(nameof(Caliber9x19PARA), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber9x19PARA = 125f;

        [DefaultValue(135f)]
        [AmmoCaliber(Caliber.Caliber46x30)]
        [NameAndDescription(nameof(Caliber46x30), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber46x30 = 135;

        [DefaultValue(130f)]
        [AmmoCaliber(Caliber.Caliber9x21)]
        [NameAndDescription(nameof(Caliber9x21), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber9x21 = 130;

        [DefaultValue(140f)]
        [AmmoCaliber(Caliber.Caliber57x28)]
        [NameAndDescription(nameof(Caliber57x28), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber57x28 = 140;

        [DefaultValue(140f)]
        [AmmoCaliber(Caliber.Caliber762x25TT)]
        [NameAndDescription(nameof(Caliber762x25TT), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber762x25TT = 140;

        [DefaultValue(140f)]
        [AmmoCaliber(Caliber.Caliber1143x23ACP)]
        [NameAndDescription(nameof(Caliber1143x23ACP), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber1143x23ACP = 140;

        [DefaultValue(130f)]
        [AmmoCaliber(Caliber.Caliber9x33R)]
        [NameAndDescription(nameof(Caliber9x33R), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber9x33R = 130;

        [DefaultValue(180f)]
        [AmmoCaliber(Caliber.Caliber545x39)]
        [NameAndDescription(nameof(Caliber545x39), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber545x39 = 180;

        [DefaultValue(180f)]
        [AmmoCaliber(Caliber.Caliber556x45NATO)]
        [NameAndDescription(nameof(Caliber556x45NATO), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber556x45NATO = 180;

        [DefaultValue(180f)]
        [AmmoCaliber(Caliber.Caliber9x39)]
        [NameAndDescription(nameof(Caliber9x39), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber9x39 = 180;

        [DefaultValue(180f)]
        [AmmoCaliber(Caliber.Caliber762x35)]
        [NameAndDescription(nameof(Caliber762x35), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber762x35 = 180;

        [DefaultValue(200f)]
        [AmmoCaliber(Caliber.Caliber762x39)]
        [NameAndDescription(nameof(Caliber762x39), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber762x39 = 200;

        [DefaultValue(200f)]
        [AmmoCaliber(Caliber.Caliber366TKM)]
        [NameAndDescription(nameof(Caliber366TKM), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber366TKM = 200;

        [DefaultValue(225f)]
        [AmmoCaliber(Caliber.Caliber762x51)]
        [NameAndDescription(nameof(Caliber762x51), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber762x51 = 225;

        [DefaultValue(225f)]
        [AmmoCaliber(Caliber.Caliber127x55)]
        [NameAndDescription(nameof(Caliber127x55), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber127x55 = 225;

        [DefaultValue(275f)]
        [AmmoCaliber(Caliber.Caliber762x54R)]
        [NameAndDescription(nameof(Caliber762x54R), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber762x54R = 275;

        [DefaultValue(300f)]
        [AmmoCaliber(Caliber.Caliber86x70)]
        [NameAndDescription(nameof(Caliber86x70), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber86x70 = 300;

        [DefaultValue(225f)]
        [AmmoCaliber(Caliber.Caliber20g)]
        [NameAndDescription(nameof(Caliber20g), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber20g = 225;

        [DefaultValue(225f)]
        [AmmoCaliber(Caliber.Caliber12g)]
        [NameAndDescription(nameof(Caliber12g), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber12g = 225;

        [DefaultValue(210f)]
        [AmmoCaliber(Caliber.Caliber23x75)]
        [NameAndDescription(nameof(Caliber23x75), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber23x75 = 210;

        [DefaultValue(50f)]
        [AmmoCaliber(Caliber.Caliber26x75)]
        [NameAndDescription(nameof(Caliber26x75), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber26x75 = 50;

        [DefaultValue(50)]
        [AmmoCaliber(Caliber.Caliber30x29)]
        [NameAndDescription(nameof(Caliber30x29), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber30x29 = 50;

        [DefaultValue(50)]
        [AmmoCaliber(Caliber.Caliber40x46)]
        [NameAndDescription(nameof(Caliber40x46), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber40x46 = 50;

        [DefaultValue(50)]
        [AmmoCaliber(Caliber.Caliber40mmRU)]
        [NameAndDescription(nameof(Caliber40mmRU), Description)]
        [MinMaxRound(30f, 500f, 1f)]
        public float Caliber40mmRU = 50;

        public static readonly float Default = 150f;
    }
}