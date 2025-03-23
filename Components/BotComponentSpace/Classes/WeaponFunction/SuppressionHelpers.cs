using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;

namespace SAIN.SAINComponent.Classes.WeaponFunction
{
    public static class SuppressionHelpers
    {
        public static ESuppressionState FindActiveState(float suppNum, out SuppressionConfig suppressionConfig)
        {
            Dictionary<ESuppressionState, SuppressionConfig> configDict = GlobalSettingsClass.Instance.Mind.SUPPRESSION_STATES;

            ESuppressionState state = ESuppressionState.Extreme;
            if (configDict.TryGetValue(state, out suppressionConfig) &&
                suppressionConfig.IsActive(suppNum)) {
                return state;
            }

            state = ESuppressionState.Heavy;
            if (configDict.TryGetValue(state, out suppressionConfig) &&
                suppressionConfig.IsActive(suppNum)) {
                return state;
            }

            state = ESuppressionState.Medium;
            if (configDict.TryGetValue(state, out suppressionConfig) &&
                suppressionConfig.IsActive(suppNum)) {
                return state;
            }

            state = ESuppressionState.Light;
            if (configDict.TryGetValue(state, out suppressionConfig) &&
                suppressionConfig.IsActive(suppNum)) {
                return state;
            }

            return ESuppressionState.None;
        }
    }
}