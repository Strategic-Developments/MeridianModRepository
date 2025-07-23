using DetectionEquipment.BaseDefinitions;

namespace DetectionEquipment
{
    internal partial class DetectionDefinitions
    {
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

        };

        internal readonly CountermeasureDefinition[] CountermeasureDefinitions = new CountermeasureDefinition[]
        {
            // Your countermeasure definitions here.
        };

        internal readonly CountermeasureEmitterDefinition[] CountermeasureEmitterDefinitions = new CountermeasureEmitterDefinition[]
        {
            // Your countermeasure emitter definitions here.
        };
    }
}
