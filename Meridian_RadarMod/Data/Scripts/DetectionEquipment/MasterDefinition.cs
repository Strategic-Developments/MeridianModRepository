using DetectionEquipment.BaseDefinitions;
using System;
using VRageMath;
using static DetectionEquipment.BaseDefinitions.SensorDefinition;

namespace DetectionEquipment
{
    internal partial class DetectionDefinitions
    {
        /// <summary>
        /// Definition loading priority.
        /// Lower numbers load first; to override another mod, set this to a higher value than the other mod.
        /// DetEq internal definitions have minimum priority, and will always be overriden if possible.
        /// </summary>
        internal const int LoadPriority = int.MinValue + 1;

        internal readonly SensorDefinition[] SensorDefinitions = new SensorDefinition[]
        {
            // Your sensor definitions here.

            //RADARS
            //Tier SG
            ASEL_MKXII_FCR_Def,
            MXM_UNIT_16_Sensor_Def,//UNIT-16 panel sensor
            //Tier I
            MXM_SB_PRR_Series2_Def, //SB-PRR Series2 RWR sensor
            MXM_012_FCR_Def,
            MXM_005_Sensor_Def,
            //Tier II
            MXM_SET_2_Sensor_Def, //SET-2 panel sensor
            MXM_FR_057_Def,
            MXM_008_Sensor_Search_Def, MXM_008_Sensor_Track_Def,
            CSys_NS_RCG_62_Sensor_Def,
            //Tier III
            MXM_009_Sensor_Search_Def, MXM_009_Sensor_Track1_Def, MXM_009_Sensor_Track2_Def,
            MXM_ASTS_5_Def,
            MXM_SAARP_Sensor_Fix_Def, MXM_SAARP_Sensor_Track1_Def, MXM_SAARP_Sensor_Track2_Def, //SAARP panel sensors
            //Tier IV
            //KONIG panels here

            //OPTICAL
            C_Sys_VRE_Optical_Def,
            C_Sys_IRO_DetBlock_Optical_Def,

            //IR
            C_Sys_VRE_IR_Def,
            C_Sys_IRO_DetBlock_IR_Def,


            new SensorDefinition
            {
                Name = "DetEq_VanillaCamera",

                BlockSubtypes = new[]
                {
                    "LargeCameraBlock",
                    "LargeCameraTopMounted",
                    "SmallCameraBlock",
                    "SmallCameraTopMounted",
                },
                Type = SensorType.Infrared,
                MaxAperture = Math.PI/4,
                MinAperture = Math.PI/16,
                DetectionThreshold = 1.45E-07,
                BearingErrorModifier = 0.001,
                RangeErrorModifier = 15,
                MaxPowerDraw = -1,
                Movement = null,
            },

            new SensorDefinition
            {
                Name = "DetEq_GimbalCamera",

                BlockSubtypes = new[]
                {
                    "GimbalCamera",
                },
                Type = SensorType.Optical,
                SensorEmpty = "GimbalCameraEmpty",
                MaxAperture = MathHelper.ToRadians(20),
                MinAperture = MathHelper.ToRadians(20),
                DetectionThreshold = 7.43E-09,
                BearingErrorModifier = 0.01,
                RangeErrorModifier = 15,
                MaxPowerDraw = -1,
                Movement = new SensorMovementDefinition
                {
                    AzimuthPart = "gimbalcam_azimuth",
                    AzimuthRate = 2 * Math.PI,
                    MaxAzimuth = Math.PI,
                    MinAzimuth = -Math.PI,

                    ElevationPart = "gimbalcam_elevation",
                    ElevationRate = 1 * Math.PI,
                    MaxElevation = Math.PI/2,
                    MinElevation = -Math.PI/8,
                },
            },

            new SensorDefinition
            {
                Name = "DetEq_SimpleActiveRadar",

                BlockSubtypes = new[]
                {
                    "ActiveRadar_Simple",
                },
                Type = SensorType.Radar,
                MaxAperture = MathHelper.ToRadians(ApMax_T_I),
                MinAperture = MathHelper.ToRadians(ApMin_T_I),

                Movement = new SensorMovementDefinition
                {
                    AzimuthPart = "smallfixedradar_azimuth",
                    AzimuthRate = 4 * Math.PI / 6,
                    MaxAzimuth = Math.PI,
                    MinAzimuth = -Math.PI,

                    ElevationPart = "smallfixedradar_elevation",
                    ElevationRate = 2 * Math.PI,
                    MaxElevation = Math.PI/2,
                    MinElevation = -Math.PI/8,
                },
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
                    AccountForRadarAngle = false,
                }
            },

            new SensorDefinition
            {
                Name = "DetEq_SmallFixedRadar",

                BlockSubtypes = new[]
                {
                    "SmallFixedRadar",
                },
                Type = SensorType.Radar,
                MaxAperture = MathHelper.ToRadians(ApMax_T_SG),
                MinAperture = MathHelper.ToRadians(ApMin_T_SG),

                Movement = new SensorMovementDefinition
                {
                    AzimuthPart = "smallfixedradar_azimuth",
                    AzimuthRate = 4 * Math.PI / 6,
                    MaxAzimuth = MathHelper.ToRadians(35),
                    MinAzimuth = -MathHelper.ToRadians(35),

                    ElevationPart = "smallfixedradar_elevation",
                    ElevationRate = 2 * Math.PI,
                    MaxElevation = MathHelper.ToRadians(35),
                    MinElevation = -MathHelper.ToRadians(35),
                },
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
                    AccountForRadarAngle = false,
                }
            },

            new SensorDefinition
            {
                Name = "DetEq_SimplePassiveRadar",

                BlockSubtypes = new[]
                {
                    "PassiveRadar_Simple",
                },
                Type = SensorType.PassiveRadar,
                MaxAperture = Math.PI,
                MinAperture = Math.PI,
                DetectionThreshold = 1000,
                MaxPowerDraw = -1,
                BearingErrorModifier = 0.00001,
                RangeErrorModifier = 1,
                Movement = null,
                RadarProperties = new RadarPropertiesDefinition
                {
                    ReceiverArea = 10 * 5,
                }
            },
        };

        internal readonly CountermeasureDefinition[] CountermeasureDefinitions = new CountermeasureDefinition[]
        {
            // Your countermeasure definitions here.
            // aristeas if you see this, trolled - Nerd
            new CountermeasureDefinition()
            {
                Name = "DetEq_SimpleFlare",

                CountermeasureType = CountermeasureDefinition.CountermeasureTypeEnum.Infrared,
                MaxRange = 300000,
                FalloffScalar = 0.00001f,
                MinNoise = 0.01f,
                FalloffType = CountermeasureDefinition.FalloffTypeEnum.Linear,
                MinEffectAperture = (float) Math.PI,
                MaxEffectAperture = (float) Math.PI,
                MaxLifetime = 300,
                HasPhysics = true,
                DragMultiplier = 0.001f,
                ParticleEffect = "Smoke_Firework"
            },
            new CountermeasureDefinition()
            {
                Name = "DetEq_SimpleChaff",

                CountermeasureType = CountermeasureDefinition.CountermeasureTypeEnum.Radar,
                MaxRange = 300000,
                FalloffScalar = 1.0E10f,
                MinNoise = 0f,
                FalloffType = CountermeasureDefinition.FalloffTypeEnum.Quadratic,
                MinEffectAperture = (float) Math.PI,
                MaxEffectAperture = (float) Math.PI,
                MaxLifetime = 240,
                HasPhysics = true,
                DragMultiplier = 0.001f,
                ParticleEffect = "SimpleChaffParticle"
            },
            new CountermeasureDefinition()
            {
                Name = "DetEq_SimpleAreaJammer",

                CountermeasureType = CountermeasureDefinition.CountermeasureTypeEnum.Radar,
                MaxRange = 300000,
                FalloffScalar = 1.0E12f,
                MinNoise = 0f,
                FalloffType = CountermeasureDefinition.FalloffTypeEnum.Quadratic,
                MinEffectAperture = (float) Math.PI,
                MaxEffectAperture = (float) Math.PI,
                MaxLifetime = uint.MaxValue,
                HasPhysics = false,
                DragMultiplier = 0f,
                ParticleEffect = ""
            }
        };

        internal readonly CountermeasureEmitterDefinition[] CountermeasureEmitterDefinitions = new CountermeasureEmitterDefinition[]
        {
            // Your countermeasure emitter definitions here.
            new CountermeasureEmitterDefinition
            {
                Name = "DetEq_SimpleFlareEmitter",

                BlockSubtypes = new[]
                {
                    "FlareLauncher"
                },
                Muzzles = new[]
                {
                    "muzzle_01",
                    "muzzle_02",
                    "muzzle_03",
                    "muzzle_04",
                    "muzzle_05",
                    "muzzle_06",
                    "muzzle_07",
                    "muzzle_08",
                    "muzzle_09",
                    "muzzle_10",
                    "muzzle_11",
                    "muzzle_12",
                    "muzzle_13",
                    "muzzle_14",
                    "muzzle_15",
                    "muzzle_16",
                    "muzzle_17",
                    "muzzle_18",
                    "muzzle_19",
                    "muzzle_20",
                    "muzzle_21",
                    "muzzle_22",
                    "muzzle_23",
                    "muzzle_24",
                    "muzzle_25",
                },
                CountermeasureIds = new[]
                {
                    "DetEq_SimpleFlare"
                },
                IsCountermeasureAttached = false,
                ShotsPerSecond = 15,
                EjectionVelocity = 50,
                FireParticle = "Muzzle_Flash_Autocannon",
                ActivePowerDraw = 0,
                MagazineSize = 25,
                MagazineItem = "DetEq_FlareMagazine",
                ReloadTime = 12,
                InventorySize = 0.240f,
            },
            new CountermeasureEmitterDefinition
            {
                Name = "DetEq_SimpleChaffEmitter",

                BlockSubtypes = new[]
                {
                    "ChaffLauncher"
                },
                Muzzles = new[]
                {
                    "muzzle_01",
                    "muzzle_02",
                    "muzzle_03",
                    "muzzle_04",
                    "muzzle_05",
                    "muzzle_06",
                    "muzzle_07",
                    "muzzle_08",
                    "muzzle_09",
                    "muzzle_10",
                    "muzzle_11",
                    "muzzle_12",
                    "muzzle_13",
                    "muzzle_14",
                    "muzzle_15",
                    "muzzle_16",
                    "muzzle_17",
                    "muzzle_18",
                    "muzzle_19",
                    "muzzle_20",
                    "muzzle_21",
                    "muzzle_22",
                    "muzzle_23",
                    "muzzle_24",
                    "muzzle_25",
                },
                CountermeasureIds = new[]
                {
                    "DetEq_SimpleChaff"
                },
                IsCountermeasureAttached = false,
                ShotsPerSecond = 15,
                EjectionVelocity = 100,
                FireParticle = "Muzzle_Flash_Autocannon",
                ActivePowerDraw = 0,
                MagazineSize = 25,
                MagazineItem = "DetEq_ChaffMagazine",
                ReloadTime = 12,
                InventorySize = 0.240f,
            },
            new CountermeasureEmitterDefinition
            {
                Name = "DetEq_SimpleJammer",

                BlockSubtypes = new[]
                {
                    "SimpleJammer"
                },
                Muzzles = new[]
                {
                    "muzzle",
                },
                CountermeasureIds = new[]
                {
                    "DetEq_SimpleAreaJammer"
                },
                IsCountermeasureAttached = true,
                ShotsPerSecond = 60,
                EjectionVelocity = 0,
                FireParticle = "",
                ActivePowerDraw = 50,
                InventorySize = 0,
            },
        };
    }
}
