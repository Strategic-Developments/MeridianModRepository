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
        //Common
        private static double Bandwidth_General = 1670000;
        private static double Freq_General = 2800000000;
        private static double PowerEfficiency_General = 2.50E-16;

        //Tier SG
        private static int PowerDraw_SG = 5000000;

        private static double DetThreshold_SG_S = 8.873949985;
        private static double DetThreshold_SG_P = 8.873949985;
        private static double DetThreshold_SG_T = 7.043650362;

        private static double BearingError_Search_SG = 0.4;
        private static double RangeError_Search_SG = 0.4;
        private static double BearingError_Panel_SG = 0.4;
        private static double RangeError_Panel_SG = 0.4;
        private static double BearingError_Track_SG = 0.4;
        private static double RangeError_Track_SG = 0.4;

        private static double ReceiverArea_SG = 10;

        private static double ApMin_S_SG = 15;
        private static double ApMin_P_SG = 65;
        private static double ApMin_T_SG = 1;

        private static double ApMax_S_SG = 25;
        private static double ApMax_P_SG = 75;
        private static double ApMax_T_SG = 2;

        private static double PowerEffMod_SG_S = 4.3921686E+04;
        private static double PowerEffMod_SG_P = 6.4347690E+08;
        private static double PowerEffMod_SG_T = 1.1736116E-03;

        //Tier I
        private static int PowerDraw_I = 15000000;

        private static double DetThreshold_I_S = 14.5271161;
        private static double DetThreshold_I_P = 7.483465734;
        private static double DetThreshold_I_T = 4.997549464;

        private static double BearingError_Search_I = 0.4;
        private static double RangeError_Search_I = 0.4;
        private static double BearingError_Panel_I = 0.4;
        private static double RangeError_Panel_I = 0.4;
        private static double BearingError_Track_I = 0.4;
        private static double RangeError_Track_I = 0.4;

        private static double ReceiverArea_I = 20;

        private static double ApMin_S_I = 15;
        private static double ApMin_P_I = 55;
        private static double ApMin_T_I = 1;

        private static double ApMax_S_I = 25;
        private static double ApMax_P_I = 65;
        private static double ApMax_T_I = 2;

        private static double PowerEffMod_I_S = 7.4117846E+04;
        private static double PowerEffMod_I_P = 6.0545738E+07;
        private static double PowerEffMod_I_T = 1.2363974E-03;

        //Tier II
        private static int PowerDraw_II = 30000000;

        private static double DetThreshold_II_S = 18.23727823;
        private static double DetThreshold_II_P = 13.23972876;
        private static double DetThreshold_II_T = 3.87640052;

        private static double BearingError_Search_II = 0.2;
        private static double RangeError_Search_II = 0.2;
        private static double BearingError_Panel_II = 0.2;
        private static double RangeError_Panel_II = 0.2;
        private static double BearingError_Track_II = 0.2;
        private static double RangeError_Track_II = 0.2;

        private static double ReceiverArea_II = 40;

        private static double ApMin_S_II = 25;
        private static double ApMin_P_II = 60;
        private static double ApMin_T_II = 1;

        private static double ApMax_S_II = 35;
        private static double ApMax_P_II = 70;
        private static double ApMax_T_II = 2;

        private static double PowerEffMod_II_S = 2.6930985E+06;
        private static double PowerEffMod_II_P = 2.9203212E+08;
        private static double PowerEffMod_II_T = 1.5092741E-03;

        //Tier III
        private static int PowerDraw_III = 60000000;

        private static double DetThreshold_III_S = 18.74084332;
        private static double DetThreshold_III_P = 14.8644428;
        private static double DetThreshold_III_T = 3.167249842;

        private static double BearingError_Search_III = 0.1;
        private static double RangeError_Search_III = 0.1;
        private static double BearingError_Panel_III = 0.1;
        private static double RangeError_Panel_III = 0.1;
        private static double BearingError_Track_III = 0.1;
        private static double RangeError_Track_III = 0.1;

        private static double ReceiverArea_III = 80;

        private static double ApMin_S_III = 35;
        private static double ApMin_P_III = 65;
        private static double ApMin_T_III = 1;

        private static double ApMax_S_III = 45;
        private static double ApMax_P_III = 75;
        private static double ApMax_T_III = 2;

        private static double PowerEffMod_III_S = 2.7593369E+07;
        private static double PowerEffMod_III_P = 8.5796920E+08;
        private static double PowerEffMod_III_T = 1.5648154E-03;

        //Tier IV
        private static int PowerDraw_IV = 120000000;
        private static int PowerDraw_IV_Track = 85000000;

        private static double DetThreshold_IV_S = 21.76272177;
        private static double DetThreshold_IV_P = 19.08485019;
        private static double DetThreshold_IV_T = 2.677871585;

        private static double BearingError_Search_IV = 0.05;
        private static double RangeError_Search_IV = 0.05;
        private static double BearingError_Panel_IV = 0.05;
        private static double RangeError_Panel_IV = 0.05;
        private static double BearingError_Track_IV = 0.05;
        private static double RangeError_Track_IV = 0.05;

        private static double ReceiverArea_IV = 160;

        private static double ApMin_S_IV = 65;
        private static double ApMin_P_IV = 89;
        private static double ApMin_T_IV = 1;

        private static double ApMax_S_IV = 85;
        private static double ApMax_P_IV = 90;
        private static double ApMax_T_IV = 2;

        private static double PowerEffMod_IV_S = 4.0234064E+09;
        private static double PowerEffMod_IV_P = 1.9910596E+12;
        private static double PowerEffMod_IV_T = 2.0463627E-03;

        // Hover over field names or look at the base definition for more information.
        private static SensorDefinition MXM_012_FCR_Def => new SensorDefinition
        {
            Name = "MXM_012_FCR_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_Radar_1"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_T_I),
            MinAperture = MathHelper.ToRadians(ApMin_T_I),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "R1_Azi",
                AzimuthRate = 4 * Math.PI / 6,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "R1_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "MXM-012 Track Sensor",

            DetectionThreshold = DetThreshold_I_T,
            BearingErrorModifier = BearingError_Track_I,
            RangeErrorModifier = RangeError_Track_I,
            
            MaxPowerDraw = PowerDraw_I,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_I,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_I_T,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_ASTS_5_Def => new SensorDefinition
        {
            Name = "MXM_ASTS_5_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_ASTS_5"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_S_III),
            MinAperture = MathHelper.ToRadians(ApMin_S_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "ASTS5_Azi",
                AzimuthRate = Math.PI /8 ,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "ASTS5_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "ASTS-5 Search Sensor",

            DetectionThreshold = DetThreshold_III_S,
            BearingErrorModifier = BearingError_Search_III,
            RangeErrorModifier = RangeError_Search_III,

            MaxPowerDraw = PowerDraw_III,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_S,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_FR_057_Def => new SensorDefinition
        {
            Name = "MXM_FR_057_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_FR_057"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_T_II),
            MinAperture = MathHelper.ToRadians(ApMin_T_II),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "FR_057_Azi",
                AzimuthRate = Math.PI / 6,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "FR_057_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "FR-057 Track Sensor",

            DetectionThreshold = DetThreshold_II_T,
            BearingErrorModifier = BearingError_Track_II,
            RangeErrorModifier = RangeError_Track_II,

            MaxPowerDraw = PowerDraw_II,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_II,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_II_T,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition CSys_NS_RCG_62_Sensor_Def => new SensorDefinition
        {
            Name = "CSys_NS_RCG_62_Sensor_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_CSys_NS_RCG_62"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "CSys_NS_RCG_62_Sensor",

            MaxAperture = MathHelper.ToRadians(ApMax_T_II),
            MinAperture = MathHelper.ToRadians(ApMin_T_II),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "CSys_NS_RCG_62_Azi",
                AzimuthRate = Math.PI / 2,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,

                ElevationPart = "CSys_NS_RCG_62_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI / 9,
                MinElevation = - Math.PI /2,
            },

            TerminalName = "NS/RCG-62 Track Sensor",

            DetectionThreshold = DetThreshold_II_T,
            BearingErrorModifier = BearingError_Track_II,
            RangeErrorModifier = RangeError_Track_II,

            MaxPowerDraw = PowerDraw_II,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_II,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_II_T,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_008_Sensor_Search_Def => new SensorDefinition
        {
            Name = "MXM_008_Sensor_Search_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_008",
                "SOFL_MXM_008_Centered",
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "SOFL_MXM_012_Sensor",

            MaxAperture = MathHelper.ToRadians(ApMax_S_II),
            MinAperture = MathHelper.ToRadians(ApMin_S_II),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "MXM_012_Azi",
                AzimuthRate =  2 * Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "MXM_012_Elev",
                ElevationRate = 4 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "MXM-008 Search Sensor",

            DetectionThreshold = DetThreshold_II_S,
            BearingErrorModifier = BearingError_Search_II,
            RangeErrorModifier = RangeError_Search_II,

            MaxPowerDraw = PowerDraw_II / 2,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_II,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_II_S * 2,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        private static SensorDefinition MXM_008_Sensor_Track_Def => new SensorDefinition
        {
            Name = "MXM_008_Sensor_Track_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_008",
                "SOFL_MXM_008_Centered",
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_T_II),
            MinAperture = MathHelper.ToRadians(ApMin_T_II),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate =  2 * Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "",
                ElevationRate = 4 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "MXM-008 Track Sensor",

            DetectionThreshold = DetThreshold_II_T,
            BearingErrorModifier = BearingError_Track_II,
            RangeErrorModifier = RangeError_Track_II,

            MaxPowerDraw = PowerDraw_II / 2,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_II,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_II_T * 2,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_009_Sensor_Search_Def => new SensorDefinition
        {
            Name = "MXM_009_Sensor_1_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_009"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_S_III),
            MinAperture = MathHelper.ToRadians(ApMin_S_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate =  Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "MXM-009 Search Sensor",

            DetectionThreshold = DetThreshold_III_S,
            BearingErrorModifier = BearingError_Search_III,
            RangeErrorModifier = RangeError_Track_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_S * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        private static SensorDefinition MXM_009_Sensor_Track1_Def => new SensorDefinition
        {
            Name = "MXM_009_Sensor_Track1_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_009"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_T_III),
            MinAperture = MathHelper.ToRadians(ApMin_T_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate = Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,

                ElevationPart = "",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI / 2,
                MinElevation = -Math.PI / 8,
            },

            TerminalName = "MXM-009 Track Sensor 1",

            DetectionThreshold = DetThreshold_III_T,
            BearingErrorModifier = BearingError_Track_III,
            RangeErrorModifier = RangeError_Track_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_T * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        private static SensorDefinition MXM_009_Sensor_Track2_Def => new SensorDefinition
        {
            Name = "MXM_009_Sensor_Track2_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_009"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_T_III),
            MinAperture = MathHelper.ToRadians(ApMin_T_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate = Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,

                ElevationPart = "",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI / 2,
                MinElevation = -Math.PI / 8,
            },

            TerminalName = "MXM-009 Track Sensor 2",

            DetectionThreshold = DetThreshold_III_T,
            BearingErrorModifier = BearingError_Track_III,
            RangeErrorModifier = RangeError_Track_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_T * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_005_Sensor_Def => new SensorDefinition
        {
            Name = "MXM_005_Sensor_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_MXM_005"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "",

            MaxAperture = MathHelper.ToRadians(ApMax_S_I),
            MinAperture = MathHelper.ToRadians(ApMin_S_I),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate =  Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/16,
            },

            TerminalName = "MXM-005 Search Sensor",

            DetectionThreshold = DetThreshold_I_S,
            BearingErrorModifier = BearingError_Track_I,
            RangeErrorModifier = RangeError_Search_I,

            MaxPowerDraw = PowerDraw_I,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_I,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_I_S,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        
        private static SensorDefinition ASEL_MKXII_FCR_Def => new SensorDefinition
        {
            Name = "ASEL_MKXII_FCR_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_ASEL_MKXII_FCR"
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "ASEL_MKXII_Radar_Sensor",

            MaxAperture = MathHelper.ToRadians(ApMax_T_SG),
            MinAperture = MathHelper.ToRadians(ApMin_T_SG),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "ASEL_MKXII_Azi",
                AzimuthRate = Math.PI ,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,
            
                ElevationPart = "ASEL_MKXII_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI/2,
                MinElevation = -Math.PI/8,
            },

            TerminalName = "ASEL MKXII Track Sensor",

            DetectionThreshold = DetThreshold_SG_T,
            BearingErrorModifier = BearingError_Track_SG,
            RangeErrorModifier = RangeError_Track_SG,

            MaxPowerDraw = PowerDraw_SG,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_SG,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_SG_T,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_SAARP_Sensor_Fix_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor_Fix_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat",
                "SOFL_SAARP_1x1",
                "SOFL_SAARP_1x1_Corner",
                "SOFL_SAARP_1x2",
                "SOFL_SAARP_1x2_Corner",
                "SOFL_SAARP_1x1_Offset",
                "SOFL_SAARP_1x2_Corner_Offset",
                "SOFL_SAARP_1x2_Offset",
                "SOFL_SAARP_Half_Flat",
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SAARP_Sensor_Search",

            MaxAperture = MathHelper.ToRadians(ApMax_P_III),
            MinAperture = MathHelper.ToRadians(ApMin_P_III),

            Movement = null,

            TerminalName = "SAARP Search Sensor",

            DetectionThreshold = DetThreshold_III_P,
            BearingErrorModifier = BearingError_Panel_III,
            RangeErrorModifier = RangeError_Panel_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_P * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        private static SensorDefinition MXM_SAARP_Sensor_Track1_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor_Track1_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat",
                "SOFL_SAARP_1x1",
                "SOFL_SAARP_1x1_Corner",
                "SOFL_SAARP_1x2",
                "SOFL_SAARP_1x2_Corner",
                "SOFL_SAARP_1x1_Offset",
                "SOFL_SAARP_1x2_Corner_Offset",
                "SOFL_SAARP_1x2_Offset",
                "SOFL_SAARP_Half_Flat",
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "SAARP_Sensor_Track1_Def",

            MaxAperture = MathHelper.ToRadians(ApMax_T_III),
            MinAperture = MathHelper.ToRadians(ApMin_T_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate =  4 * Math.PI,
                MaxAzimuth = 0.5 * Math.PI,
                MinAzimuth = -0.5 * Math.PI,
            
                ElevationPart = "",
                ElevationRate = 4 * Math.PI,
                MaxElevation = 0.5 * Math.PI,
                MinElevation = -0.5 * Math.PI,
            },

            TerminalName = "SAARP Track Sensor 1",

            DetectionThreshold = DetThreshold_III_T,
            BearingErrorModifier = BearingError_Track_III,
            RangeErrorModifier = RangeError_Track_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_T * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };
        private static SensorDefinition MXM_SAARP_Sensor_Track2_Def => new SensorDefinition
        {
            Name = "MXM_SAARP_Sensor_Track2_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SAARP_Flat",
                "SOFL_SAARP_1x1",
                "SOFL_SAARP_1x1_Corner",
                "SOFL_SAARP_1x2",
                "SOFL_SAARP_1x2_Corner",
                "SOFL_SAARP_1x1_Offset",
                "SOFL_SAARP_1x2_Corner_Offset",
                "SOFL_SAARP_1x2_Offset",
                "SOFL_SAARP_Half_Flat",
            },

            Type = SensorDefinition.SensorType.Radar,

            SensorEmpty = "SAARP_Sensor_Track2_Def",

            MaxAperture = MathHelper.ToRadians(ApMax_T_III),
            MinAperture = MathHelper.ToRadians(ApMin_T_III),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate =  4 * Math.PI,
                MaxAzimuth = 0.5 * Math.PI,
                MinAzimuth = -0.5 * Math.PI,
            
                ElevationPart = "",
                ElevationRate = 4 * Math.PI,
                MaxElevation = 0.5 * Math.PI,
                MinElevation = -0.5 * Math.PI,
            },

            TerminalName = "SAARP Track Sensor 2",

            DetectionThreshold = DetThreshold_III_T,
            BearingErrorModifier = BearingError_Track_III,
            RangeErrorModifier = RangeError_Track_III,

            MaxPowerDraw = PowerDraw_III / 3,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_III,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_III_T * 3,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_UNIT_16_Sensor_Def => new SensorDefinition
        {
            Name = "MXM_UNIT_16_Sensor_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_UNIT_16_Flat",
                "SOFL_UNIT_16_1x2_Upright_Offset",
                "SOFL_UNIT_16_1x2_Upright",
                "SOFL_UNIT_16_1x2_Side",
                "SOFL_UNIT_16_1x1_Upright",
                "SOFL_UNIT_16_1x1_Side",
                "SOFL_UNIT_16_1x1_Offset",
                "SOFL_UNIT_16_1x1_Corner_Upright",
                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "UNIT_16_Sensor",

            MaxAperture = MathHelper.ToRadians(ApMax_P_SG),
            MinAperture = MathHelper.ToRadians(ApMin_P_SG),

            Movement = null,

            TerminalName = "UNIT-16 Search Radar",

            DetectionThreshold = DetThreshold_SG_P,
            BearingErrorModifier = BearingError_Panel_SG,
            RangeErrorModifier = RangeError_Panel_SG,

            MaxPowerDraw = PowerDraw_SG,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_SG,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_SG_P,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General,
            }
        };

        private static SensorDefinition MXM_SET_2_Sensor_Def => new SensorDefinition
        {
            Name = "MXM_SET_2_Sensor_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SET2_Flat",
                "SOFL_SET2_Flat_Half",
                "SOFL_SET2_1x1",
                "SOFL_SET2_1x1_Offset",
                "SOFL_SET2_1x1_Corner",
                "SOFL_SET2_1x1_Corner_Offset",
                "SOFL_SET2_1x2",
                "SOFL_SET2_1x2_Offset",
                

                
            },

            Type = SensorDefinition.SensorType.Radar,
            
            SensorEmpty = "SET_2_Sensor",

            MaxAperture = MathHelper.ToRadians(ApMax_P_II),
            MinAperture = MathHelper.ToRadians(ApMin_P_II),

            Movement = null,

            TerminalName = "SET-2 Search Radar",

            DetectionThreshold = DetThreshold_II_P,
            BearingErrorModifier = BearingError_Panel_II,
            RangeErrorModifier = RangeError_Panel_II,

            MaxPowerDraw = PowerDraw_II,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = ReceiverArea_II,
                PowerEfficiencyModifier = PowerEfficiency_General * PowerEffMod_II_P,
                Bandwidth = Bandwidth_General,
                Frequency = Freq_General
            }
        };

        private static SensorDefinition MXM_SB_PRR_Series2_Def => new SensorDefinition
        {
            Name = "MXM_SB_PRR_Series2_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_SB_PRR_Series2_Flat",

            },

            Type = SensorDefinition.SensorType.PassiveRadar,

            SensorEmpty = "SB_PRR_Series2_Sensor",

            MaxAperture = MathHelper.ToRadians(45),
            MinAperture = MathHelper.ToRadians(40),

            Movement = null,

            TerminalName = "SB-PRR RWR sensor",

            DetectionThreshold = 30,
            BearingErrorModifier = 0.5,
            RangeErrorModifier = 0.01,

            MaxPowerDraw = -1,

            RadarProperties = new SensorDefinition.RadarPropertiesDefinition
            {
                ReceiverArea = 10 * 5,
            }

        };

        private static SensorDefinition C_Sys_VRE_Optical_Def => new SensorDefinition
        {
            Name = "C_Sys_VRE_Optical_Def",

            BlockSubtypes = new[]
            {
            // These HAVE to be camera blocks for now.
            "SOFL_C-SYS_VRE",
            "SOFL_C-SYS_VRE_Inset",

            },

            Type = SensorDefinition.SensorType.Optical,

            SensorEmpty = "C-Sys_VRE_Opt_Sensor",

            MaxAperture = MathHelper.ToRadians(55),
            MinAperture = MathHelper.ToRadians(35),

            Movement = null,

            TerminalName = "C-Sys VRE Optic sensor",

            DetectionThreshold = 30,
            BearingErrorModifier = 0.9,
            RangeErrorModifier = 0.6,

            MaxPowerDraw = 500000,
        };
        private static SensorDefinition C_Sys_VRE_IR_Def => new SensorDefinition
        {
            Name = "C_Sys_VRE_IR_Def",

            BlockSubtypes = new[]
            {
            // These HAVE to be camera blocks for now.
            "SOFL_C-SYS_VRE",
            "SOFL_C-SYS_VRE_Inset",

            },

            Type = SensorDefinition.SensorType.Infrared,

            SensorEmpty = "C-Sys_VRE_IR_Sensor",

            MaxAperture = MathHelper.ToRadians(55),
            MinAperture = MathHelper.ToRadians(35),

            Movement = null,

            TerminalName = "C-Sys VRE IR sensor",

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 3,

            MaxPowerDraw = 500000,
        };

        private static SensorDefinition C_Sys_IRO_DetBlock_Optical_Def => new SensorDefinition
        {
            Name = "C_Sys_IRO_DetBlock_Optical_Def",

            BlockSubtypes = new[]
            {
                // These HAVE to be camera blocks for now.
                "SOFL_CSys_IRO_DetBlock_MkX"
            },

            Type = SensorDefinition.SensorType.Optical,

            SensorEmpty = "CSys_IRO_DetBlock_Sensor_Opt",

            MaxAperture = MathHelper.ToRadians(20),
            MinAperture = MathHelper.ToRadians(0.5),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "SOFL_CSys_IRO_DetBlock_Azi",
                AzimuthRate = Math.PI,
                MaxAzimuth = Math.PI,
                MinAzimuth = -Math.PI,

                ElevationPart = "SOFL_CSys_IRO_DetBlock_Elev",
                ElevationRate = 2 * Math.PI,
                MaxElevation = Math.PI,
                MinElevation = -Math.PI / 8,
            },

            TerminalName = "C-Sys IR/O DetBlock Optical Sensor",

            DetectionThreshold = 30,
            BearingErrorModifier = 0.01,
            RangeErrorModifier = 1,

            MaxPowerDraw = 500000,

        };
        private static SensorDefinition C_Sys_IRO_DetBlock_IR_Def => new SensorDefinition
        {
            Name = "C_Sys_IRO_DetBlock_IR_Def",

            BlockSubtypes = new[]
                    {
                // These HAVE to be camera blocks for now.
                "SOFL_CSys_IRO_DetBlock_MkX"
            },

            Type = SensorDefinition.SensorType.Infrared,

            SensorEmpty = "CSys_IRO_DetBlock_Sensor_IR",

            MaxAperture = MathHelper.ToRadians(20),
            MinAperture = MathHelper.ToRadians(0.5),

            Movement = new SensorDefinition.SensorMovementDefinition
            {
                AzimuthPart = "",
                AzimuthRate = Math.PI,
                MaxAzimuth = 0.01,
                MinAzimuth = 0,

                ElevationPart = "",
                ElevationRate = 2 * Math.PI,
                MaxElevation = 0.01,
                MinElevation = 0,
            },

            TerminalName = "C-Sys IR/O DetBlock IR Sensor",

            DetectionThreshold = 30,
            BearingErrorModifier = 0.1,
            RangeErrorModifier = 1,

            MaxPowerDraw = 500000,

        };

    }
}