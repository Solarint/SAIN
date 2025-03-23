using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class AimSettings : SAINSettingsBase<AimSettings>, ISAINSettings
    {
        [Category("Aim Target")]
        public HitEffectSettings HitEffects = new HitEffectSettings();

        [Name("Always Aim Center Mass Global")]
        [Description("Force Bots to aim for center of mass. If this is disabled, all bots will have Always Aim Center Mass turned OFF, so their individual settings will be ignored.")]
        [Category("Aim Target")]
        public bool AimCenterMassGlobal = true;

        [Category("Scatter Modifiers")]
        [Name("Enemy Move Scatter Max Buff")]
        [Description("The max buff to bot scatter, so if their enemy is standing still. Scales with velocity. A value of 1 is disabled")]
        [MinMax(1f, 1.5f, 100f)]
        public float EnemyVelocityMaxBuff = 1.2f;

        [Category("Scatter Modifiers")]
        [Name("Enemy Move Scatter Max Debuff")]
        [Description("The minimum debuff to bot scatter, so if their enemy is moving at full speed, but not sprinting. Scales with velocity. A value of 1 is disabled")]
        [MinMax(0.5f, 1f, 100f)]
        public float EnemyVelocityMaxDebuff = 0.8f;

        [Category("Scatter Modifiers")]
        [Name("Enemy Move Scatter Sprint Debuff")]
        [Description("How much to divide bot scatter by if their enemy is sprinting. So the lower the number, the worse their aim will be. A value of 1 is disabled")]
        [MinMax(0.25f, 1f, 100f)]
        public float EnemySprintingScatterMulti = 0.66f;

        [Category("Scatter Modifiers")]
        [Name("Pose Scatter Multiplier")]
        [Description("Lower is more scatter. If a bot is crouching, reduce their scatter up to X. 1.2 would result in 20% less scatter.")]
        [Advanced]
        [MinMax(1f, 2f, 100f)]
        public float ScatterMulti_PoseLevel = 1.2f;

        [Category("Scatter Modifiers")]
        [Name("Prone Scatter Multiplier")]
        [Description("Lower is more scatter. If a bot is prone, reduce their scatter up to X. 1.3 would result in 30% less scatter.")]
        [Advanced]
        [MinMax(1f, 2f, 100f)]
        public float ScatterMulti_Prone = 1.3f;

        [Category("Scatter Modifiers")]
        [Name("Body Part Visibility Scatter Multiplier")]
        [Description("Lower is more scatter. If all body parts on an enemy are visible, reduce their scatter up to X. 1.25 would result in 25% less scatter.")]
        [Advanced]
        [MinMax(1f, 2f, 100f)]
        public float ScatterMulti_PartVis = 1.25f;

        [Category("Scatter Modifiers")]
        [Name("Magnified Optic - Ideal Range - Max Buff")]
        [Description("Lower is more scatter. If a target is further than or equal to Optic Ideal Distance, reduce their scatter by X. 1.2 = 20% less scatter")]
        [Advanced]
        [MinMax(1f, 1.5f, 100f)]
        public float OpticFarMulti = 1.2f;

        [Category("Scatter Modifiers")]
        [Name("Magnified Optic - Ideal Range - Distance")]
        [Description("The distance, in meters, that is considered ideal for a magnified optic, if shooting at a target further than or equal to this, reduce scatter.")]
        [Advanced]
        [MinMax(25f, 150f, 10f)]
        public float OpticFarDistance = 100f;

        [Category("Scatter Modifiers")]
        [Name("Magnified Optic - Too Close - Max Debuff")]
        [Description("Lower is more scatter. If a target is closer than or equal to Optic Too Close Distance, increase their scatter by X. 0.8 = 20% more scatter")]
        [Advanced]
        [MinMax(0.5f, 1f, 100f)]
        public float OpticCloseMulti = 0.8f;

        [Category("Scatter Modifiers")]
        [Name("Magnified Optic - Too Close - Distance")]
        [Description("The distance, in meters, that is considered too close for a magnified optic. " +
            "If shooting at a target closer than or equal to this, increase scatter.")]
        [Advanced]
        [MinMax(25f, 150f, 10f)]
        public float OpticCloseDistance = 75f;

        [Category("Scatter Modifiers")]
        [Name("Red-dot / Holo - Out of Range - Max Debuff")]
        [Description("Lower is more scatter. If a target is further than or equal to Red-dot / Holo - **Out of Range** - Distance, increase their scatter by X. 0.85 = 15% more scatter")]
        [Advanced]
        [MinMax(0.5f, 1f, 100f)]
        public float RedDotFarMulti = 0.85f;

        [Category("Scatter Modifiers")]
        [Name("Red-dot / Holo - Out of Range - Distance")]
        [Description("The distance, in meters, that is considered **Out of Range** for a Red-dot / Holo. " +
            "If shooting at a target further than or equal to this, increase scatter by **Red-dot / Holo - Out of Range - Max Debuff.**")]
        [Advanced]
        [MinMax(25f, 150f, 10f)]
        public float RedDotFarDistance = 100f;

        [Category("Scatter Modifiers")]
        [Name("Red-dot / Holo - Ideal Range - Max Buff")]
        [Description("Lower is more scatter. If a target is further than or equal to Optic Ideal Distance, reduce their scatter by X. 1.15 = 15% less scatter")]
        [Advanced]
        [MinMax(1f, 1.5f, 100f)]
        public float RedDotCloseMulti = 1.15f;

        [Category("Scatter Modifiers")]
        [Name("Red-dot / Holo - Ideal Range - Distance")]
        [Description("The distance, in meters, that is considered **Ideal Range** for a Red-dot / Holo. " +
            "If shooting at a target closer than or equal to this, reduce scatter by **Red-dot / Holo - Ideal Range - Max Buff.**")]
        [Advanced]
        [MinMax(25f, 150f, 10f)]
        public float RedDotCloseDistance = 50f;

        [Category("Scatter Modifiers")]
        [Name("Ironsights - Out of Range - Max Debuff")]
        [Description("Lower is more scatter. If a target is further than or equal to Optic Ideal Distance, reduce their scatter by X. 0.7 = 30% more scatter")]
        [Advanced]
        [MinMax(0.5f, 1f, 100f)]
        public float IronSightFarMulti = 0.7f;

        [Category("Scatter Modifiers")]
        [Name("Ironsights - Out of Range - Distance - Scale Start")]
        [Description("The distance, in meters, that is considered **Out of Range** for a ironsights. " +
            "If shooting at a target further than or equal to this, start increasing scatter linearly up to **Ironsights - Out of Range - Distance - Scale End** by **Ironsights - Out of Range - Max Debuff.**")]
        [Advanced]
        [MinMax(25f, 200f, 10f)]
        public float IronSightScaleDistanceStart = 40f;

        [Category("Scatter Modifiers")]
        [Name("Ironsights - Out of Range - Distance - Scale End")]
        [Description("The distance, in meters, that is considered **Out of Range** for a ironsights. " +
            "If shooting at a target further than or equal to this, increase scatter by **Ironsights - Out of Range - Max Debuff.**")]
        [Advanced]
        [MinMax(25f, 200f, 10f)]
        public float IronSightScaleDistanceEnd = 75f;

        [Name("Center Mass Point")]
        [Description("The maximum height that bots will target if Always Aim Center Mass is on. " +
            "A value of 0 will be directly on the center your head, a value of 1 will be directly at the floor below you at your feet. " +
            "If their aim target is above this, the height will be adjusted to be where this point is.")]
        [Category("Aim Target")]
        [Advanced]
        [MinMax(0f, 1f, 10000f)]
        public float CenterMassVal = 0.3125f;

        [Category("Time To Aim")]
        [Name("Global Faster CQB Reactions")]
        [Description("if this toggle is disabled, all bots will have Faster CQB Reactions turned OFF, so their individual settings will be ignored.")]
        public bool FasterCQBReactionsGlobal = true;

        [Category("Time To Aim")]
        [Name("Aim Down Sight Aim Time Multiplier")]
        [Description("If a bot is aiming down sights, their time to aim will be multiplied by this number")]
        [MinMax(0.01f, 1f, 100f)]
        public float AimDownSightsAimTimeMultiplier = 0.8f;

        [Name("PMCs Can Aim for Headshots")]
        [Category("Aim Target")]
        public bool PMCSAimForHead = false;

        [Category("Aim Target")]
        [Name("PMCs Can Aim for Headshots - Percentage Chance")]
        [Percentage]
        public float PMCAimForHeadChance = 33f;

        public override void Init(List<ISAINSettings> list)
        {
            list.Add(this);
            HitEffects.Init(list);
        }
    }
}