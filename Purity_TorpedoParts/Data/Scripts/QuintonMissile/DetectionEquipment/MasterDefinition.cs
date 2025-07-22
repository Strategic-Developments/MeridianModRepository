using DetectionEquipment.BaseDefinitions;

namespace DetectionEquipment
{
    internal partial class DetectionDefinitions
    {
        internal readonly SensorDefinition[] SensorDefinitions = new SensorDefinition[]
        {
            // Your sensor definitions here.
            CENTURY_RADAR_SENSOR,
            CENTURY_INFRARED_SENSOR,
           


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
