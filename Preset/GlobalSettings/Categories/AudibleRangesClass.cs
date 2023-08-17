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
            if (System.Enum.TryParse(caliber, out ICaliber result))
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

        public float Get(ICaliber key)
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
            Values = GetValuesFromClass.UpdateValues(ICaliber.Default, Default, this, Values);
        }

        [JsonIgnore][Advanced(AdvancedEnum.Hidden)] public Dictionary<object, object> Values = new Dictionary<object, object>();

        [Advanced(AdvancedEnum.Hidden)]
        private const string Description = "The Distance this bullet caliber can be heard by AI";

        public float Caliber9x18PM = 125f;
        public float Caliber9x19PARA = 125f;
        public float Caliber46x30 = 135;
        public float Caliber9x21 = 130;
        public float Caliber57x28 = 140;
        public float Caliber762x25TT = 140;
        public float Caliber1143x23ACP = 140;
        public float Caliber9x33R = 130;
        public float Caliber545x39 = 180;
        public float Caliber556x45NATO = 180;
        public float Caliber9x39 = 180;
        public float Caliber762x35 = 180;
        public float Caliber762x39 = 200;
        public float Caliber366TKM = 200;
        public float Caliber762x51 = 225;
        public float Caliber127x55 = 225;
        public float Caliber762x54R = 275;
        public float Caliber86x70 = 300;
        public float Caliber20g = 225;
        public float Caliber12g = 225;
        public float Caliber23x75 = 210;
        public float Caliber26x75 = 50;
        public float Caliber30x29 = 50;
        public float Caliber40x46 = 50;
        public float Caliber40mmRU = 50;
        public float Caliber127x108 = 300;

        public static readonly float Default = 150f;
    }
}