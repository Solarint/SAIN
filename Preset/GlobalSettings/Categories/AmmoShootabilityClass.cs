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
            Values = GetValuesFromClass.UpdateValues(ICaliber.Default, Default, this, Values);
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

        public static Dictionary<object, object> Values = new Dictionary<object, object>();

        private const string Description = "Lower is BETTER. How Shootable this ammo type is, affects semi auto firerate and full auto burst length";

        public float Caliber9x18PM = 0.2f;
        public float Caliber9x19PARA = 0.25f;
        public float Caliber46x30 = 0.3f;
        public float Caliber9x21 = 0.3f;
        public float Caliber57x28 = 0.35f;
        public float Caliber762x25TT = 0.4f;
        public float Caliber1143x23ACP = 0.4f;
        public float Caliber9x33R = 0.65f;
        public float Caliber545x39 = 0.5f;
        public float Caliber556x45NATO = 0.5f;
        public float Caliber9x39 = 0.55f;
        public float Caliber762x35 = 0.55f;
        public float Caliber762x39 = 0.65f;
        public float Caliber366TKM = 0.65f;
        public float Caliber762x51 = 0.7f;
        public float Caliber127x55 = 0.75f;
        public float Caliber762x54R = 0.8f;
        public float Caliber86x70 = 1.0f;
        public float Caliber20g = 0.65f;
        public float Caliber12g = 0.7f;
        public float Caliber23x75 = 0.75f;
        public float Caliber26x75 = 1f;
        public float Caliber30x29 = 1f;
        public float Caliber40x46 = 1f;
        public float Caliber40mmRU = 1f;

        [DefaultValue(0.25f)]
        [AmmoCaliber(ICaliber.Caliber127x108)]
        [NameAndDescription(nameof(Caliber127x108), Description)]
        [MinMax(0.01f, 1f, 100f)]
        public float Caliber127x108 = 0.25f;

        public static readonly float Default = 0.5f;
    }
}