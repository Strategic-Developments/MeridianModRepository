using VRage.Game.Components;
using VRage.Utils;

namespace DetectionEquipment.Communication
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, DetectionDefinitions.LoadPriority)]
    // ReSharper disable once UnusedType.Global
    internal class DefinitionCollector : MySessionComponentBase
    {
        internal DefinitionApi DefinitionApi = new DefinitionApi();

        public override void LoadData()
        {
            MyLog.Default.WriteLineAndConsole($"{ModContext.ModName} - Preparing DefinitionApi.");
            DefinitionApi.Init(ModContext, OnDefApiReady);
        }

        protected override void UnloadData()
        {
            DefinitionApi.UnloadData();
            MyLog.Default.WriteLineAndConsole($"{ModContext.ModName} - DefinitionApi unloaded.");
        }

        private void OnDefApiReady()
        {
            var allDefinitions = new DetectionDefinitions();
            foreach (var sensorDef in allDefinitions.SensorDefinitions)
                DefinitionApi.RegisterDefinition(sensorDef.Name, sensorDef);
            foreach (var counterDef in allDefinitions.CountermeasureDefinitions)
                DefinitionApi.RegisterDefinition(counterDef.Name, counterDef);
            foreach (var counterEDef in allDefinitions.CountermeasureEmitterDefinitions)
                DefinitionApi.RegisterDefinition(counterEDef.Name, counterEDef);
            DefinitionApi.LogInfo($"{ModContext.ModName} - Registered all definitions.");
        }
    }
}
