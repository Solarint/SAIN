using SAIN.BotSettings.Categories.Util;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.BotSettings.Categories
{
    internal class InitDictionary
    {
        static readonly Type Float = typeof(float);
        static readonly Type Int = typeof(int);
        static readonly Type Bool = typeof(bool);

        public static void Init()
        {
        }


        public static void BotCoreSettingsInit()
        {
            var core = new SAINCoreSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.VisibleAngle),
                    DisplayName = "Vision Cone Angle Max",
                    Description = "The Maximum Vision Cone for a Bot",
                    Type = Float,
                    SAINDefault = core.VisibleAngle,
                    Max = 180,
                    Min = 50
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.VisibleDistance),
                    DisplayName = "Max Visible Distance",
                    Description = "The Maximum Distance a Bot Can See an Enemy.",
                    Type = Float,
                    SAINDefault = core.VisibleDistance,
                    Max = 500,
                    Min = 50
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.GainSightCoef),
                    DisplayName = "Gain Sight Coef",
                    Description = "Controls how quickly bots notice targets, Lower means faster vision speed.",
                    Type = Float,
                    SAINDefault = core.GainSightCoef,
                    Max = 0.5f,
                    Min = 0.05f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.ScatteringPerMeter),
                    DisplayName = "Scattering Per Meter",
                    Description = "How much scatter to add to a bot for every meter between them and their aiming target.",
                    Type = Float,
                    SAINDefault = core.ScatteringPerMeter,
                    Max = 0.5f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.ScatteringClosePerMeter),
                    DisplayName = "Scattering Close Per Meter",
                    Description = "How much scatter to add to a bot for every meter between them and their aiming target.",
                    Type = Float,
                    SAINDefault = core.ScatteringClosePerMeter,
                    Max = 0.5f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.DamageCoeff),
                    DisplayName = "Damage Coeff",
                    Description = "How Much to Increase Scatter when a bot's arms are injured",
                    Type = Float,
                    SAINDefault = core.DamageCoeff,
                    Max = 5f,
                    Min = 1f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.CanGrenade),
                    DisplayName = "Can Use Grenades",
                    Description = "",
                    Type = Bool,
                    SAINDefault = core.CanGrenade,
                });
        }
    }
}
