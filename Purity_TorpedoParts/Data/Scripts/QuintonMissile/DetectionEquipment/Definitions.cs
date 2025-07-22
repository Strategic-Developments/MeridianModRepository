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

            MaxAperture = MathHelper.ToRadians(90),
            MinAperture = MathHelper.ToRadians(5),

            Movement = null,

            DetectionThreshold = 30,
            BearingErrorModifier = 0.05,
            RangeErrorModifier = 0.0001,

            MaxPowerDraw = 5000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.0000000000000025,
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

            MaxAperture = MathHelper.ToRadians(90),
            MinAperture = MathHelper.ToRadians(5),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate = 10 * Math.PI / 6,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "",
                ElevationRate = 8 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            DetectionThreshold = 30,
            BearingErrorModifier = 0.05,
            RangeErrorModifier = 0.0001,
            
            MaxPowerDraw = 5000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.0000000000000025,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };

        

        /*private static SensorDefinition MXM_SAARP_Sensor1_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor1_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat"
                //"SOFL_SAARP_1x1"
                //"SOFL_SAARP_1x1_corner"
                //"SOFL_SAARP_1x2"
                //"SOFL_SAARP_1x2_corner"
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SAARP_Sensor_1",

            MaxAperture = MathHelper.ToRadians(60),
            MinAperture = MathHelper.ToRadians(10),

            Movement = null,

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 0.0001,
            
            MaxPowerDraw = 14000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.00000000000000025,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };

        private static SensorDefinition MXM_SAARP_Sensor2_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor2_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat"
                //"SOFL_SAARP_1x1"
                //"SOFL_SAARP_1x1_corner"
                //"SOFL_SAARP_1x2"
                //"SOFL_SAARP_1x2_corner"
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SAARP_Sensor_2",

            MaxAperture = MathHelper.ToRadians(60),
            MinAperture = MathHelper.ToRadians(10),

            Movement = null,

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 0.0001,
            
            MaxPowerDraw = 14000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.00000000000000025,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };

        private static SensorDefinition MXM_SAARP_Sensor3_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor3_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat"
                //"SOFL_SAARP_1x1"
                //"SOFL_SAARP_1x1_corner"
                //"SOFL_SAARP_1x2"
                //"SOFL_SAARP_1x2_corner"
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SAARP_Sensor_3",

            MaxAperture = MathHelper.ToRadians(60),
            MinAperture = MathHelper.ToRadians(10),

            Movement = null,

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 0.0001,
            
            MaxPowerDraw = 14000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.00000000000000025,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };

        private static SensorDefinition MXM_SAARP_Sensor4_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor4_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat"
                //"SOFL_SAARP_1x1"
                //"SOFL_SAARP_1x1_corner"
                //"SOFL_SAARP_1x2"
                //"SOFL_SAARP_1x2_corner"
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SAARP_Sensor_4",

            MaxAperture = MathHelper.ToRadians(60),
            MinAperture = MathHelper.ToRadians(10),

            Movement = null,

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 0.0001,
            
            MaxPowerDraw = 14000000,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 4.9 * 2.7,
                PowerEfficiencyModifier = 0.00000000000000025,
                Bandwidth = 1.67E6,
                Frequency = 2800E6,
            }
        };*/
    }
}