using System;
using DetectionEquipment.BaseDefinitions;
using VRageMath;
using static DetectionEquipment.BaseDefinitions.CountermeasureDefinition;
using static DetectionEquipment.BaseDefinitions.CountermeasureEmitterDefinition;
using static DetectionEquipment.BaseDefinitions.SensorDefinition;

namespace DetectionEquipment
{
    internal partial class DetectionDefinitions
    {
        // Hover over field names or look at the base definition for more information.


        private static SensorDefinition CENTURY_RADAR_SENSOR => new SensorDefinition
        {
            Name = "QUINTONSENSOR",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "TorpSensor",


            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "camera",

            MaxAperture = MathHelper.ToRadians(45),
            MinAperture = MathHelper.ToRadians(45),

            Movement = null,

            DetectionThreshold = 1.830299622,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 0.1,

            MaxPowerDraw = 15000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 0.25,
                PowerEfficiencyModifier = 2.5E-16 * 3.0938493E+05,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };



        private static SensorDefinition CENTURY_INFRARED_SENSOR => new SensorDefinition
        {
            Name = "INFRAREDSENSOR_CENTURY",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "TorpIRSensor",


            },

            Type = SensorDefinition.SensorType.Infrared,

            SensorEmpty = "camera",

            MaxAperture = MathHelper.ToRadians(45),
            MinAperture = MathHelper.ToRadians(45),

            Movement = null,

            DetectionThreshold = 4.54E-07,
            BearingErrorModifier = 0.001,
            RangeErrorModifier = 0.001,

            MaxPowerDraw = 15000000,
        };
    }
}