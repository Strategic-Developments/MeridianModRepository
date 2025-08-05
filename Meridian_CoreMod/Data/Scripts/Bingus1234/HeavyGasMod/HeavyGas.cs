using System;
using System.IO;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Nevcairiel.HeavyGas
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class HeavyGasSession : MySessionComponentBase
    {
        HeavyGasSettings Settings = new HeavyGasSettings();

        public static bool EnableNPCs = false;

        public override void LoadData()
        {
            Settings.Load();
            EnableNPCs = Settings.EnableNPCs;
        }

        class HeavyGasSettings
        {
            const string VariableId = nameof(HeavyGasSettings); // IMPORTANT: must be unique as it gets written in a shared space (sandbox.sbc)
            const string FileName = "HeavyGas.ini"; // the file that gets saved to world storage under your mod's folder
            const string IniSection = "Config";

            public bool EnableNPCs = false;

            public HeavyGasSettings()
            {

            }

            void LoadConfig(MyIni iniParser)
            {
                EnableNPCs = iniParser.Get(IniSection, nameof(EnableNPCs)).ToBoolean(EnableNPCs);
            }

            void SaveConfig(MyIni iniParser)
            {
                iniParser.Set(IniSection, nameof(EnableNPCs), EnableNPCs);
            }

            public void Load()
            {
                if (MyAPIGateway.Session.IsServer)
                    LoadOnHost();
                else
                    LoadOnClient();
            }

            void LoadOnHost()
            {
                MyIni iniParser = new MyIni();

                // load file if exists then save it regardless so that it can be sanitized and updated
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(FileName, typeof(HeavyGasSettings)))
                {
                    using (TextReader file = MyAPIGateway.Utilities.ReadFileInWorldStorage(FileName, typeof(HeavyGasSettings)))
                    {
                        string text = file.ReadToEnd();

                        MyIniParseResult result;
                        if (!iniParser.TryParse(text, out result))
                            throw new Exception($"Config error: {result.ToString()}");

                        LoadConfig(iniParser);
                    }
                }

                iniParser.Clear(); // remove any existing settings that might no longer exist
                SaveConfig(iniParser);

                string saveText = iniParser.ToString();
                MyAPIGateway.Utilities.SetVariable<string>(VariableId, saveText);

                using (TextWriter file = MyAPIGateway.Utilities.WriteFileInWorldStorage(FileName, typeof(HeavyGasSettings)))
                {
                    file.Write(saveText);
                }
            }

            void LoadOnClient()
            {
                string text;
                if (!MyAPIGateway.Utilities.GetVariable<string>(VariableId, out text))
                    throw new Exception("No config found in sandbox.sbc!");

                MyIni iniParser = new MyIni();
                MyIniParseResult result;
                if (!iniParser.TryParse(text, out result))
                    throw new Exception($"Config error: {result.ToString()}");

                LoadConfig(iniParser);
            }
        }
    }

    // This object gets attached to entities depending on their type and optionally subtype aswell.
    // The 2nd arg, "false", is for entity-attached update if set to true which is not recommended, see for more info: https://forum.keenswh.com/threads/modapi-changes-jan-26.7392280/
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenTank), false)]
    public class HeavyGas : MyGameLogicComponent
    {
        // A molecule of water weights 18 atomic mass units
        // .. 2 of which are hydrogen
        // .. and 16 are oxygen
        // in SE, turning 1kg of Ice into gas results in 10L of Hydrogen, and 5L of Oxygen, scale the values accordingly
        public static double GAS_L_KG_CONVERSION_H2 = 10 * (2.0 / 18.0) / 10.0;
        public static double GAS_L_KG_CONVERSION_O2 = 10 * (16.0 / 18.0) / 5.0;

        private IMyGasTank tank;

        bool SetupComplete = false;
        double massMultiplier = 0;

        bool NPCOwned = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            // this method is called async! always do stuff in the first update unless you're sure it must be in this one.
            // NOTE the objectBuilder arg is not the Entity's but the component's, and since the component wasn't loaded from an OB that means it's always null, which it is (AFAIK).
            base.Init(objectBuilder);
            tank = (IMyGasTank)Entity;

            MyGasTankDefinition tankDef = (MyGasTankDefinition)tank.SlimBlock.BlockDefinition;

            if (tankDef != null && tankDef.StoredGasId == MyResourceDistributorComponent.HydrogenId)
                massMultiplier = GAS_L_KG_CONVERSION_H2;
            else if (tankDef != null && tankDef.StoredGasId == MyResourceDistributorComponent.OxygenId)
                massMultiplier = GAS_L_KG_CONVERSION_O2;

            NeedsUpdate = massMultiplier > 0f ? MyEntityUpdateEnum.EACH_FRAME : MyEntityUpdateEnum.NONE;
        }

        private void CheckIfNPCOwned(IMyCubeGrid grid)
        {
            NPCOwned = true;
            foreach (var owner in grid.BigOwners)
            {
                if (owner == 0)
                    continue;

                if (MyAPIGateway.Players.TryGetSteamId(owner) > 0)
                    NPCOwned = false;
            }
        }

        private void OnGridSplit(IMyCubeGrid arg1, IMyCubeGrid arg2)
        {
            // stop listening for events on split grids
            arg1.OnBlockOwnershipChanged -= CheckIfNPCOwned;
            arg1.OnGridSplit -= OnGridSplit;
            arg2.OnBlockOwnershipChanged -= CheckIfNPCOwned;
            arg2.OnGridSplit -= OnGridSplit;

            // and continue listening on the tanks grid
            tank.CubeGrid.OnBlockOwnershipChanged += CheckIfNPCOwned;
            tank.CubeGrid.OnGridSplit += OnGridSplit;

            // .. and update ownership now
            CheckIfNPCOwned(tank.CubeGrid);
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (SetupComplete == false)
            {
                tank.CubeGrid.OnBlockOwnershipChanged += CheckIfNPCOwned;
                tank.CubeGrid.OnGridSplit += OnGridSplit;
                CheckIfNPCOwned(tank.CubeGrid);

                // Update every 100 frames only from now on
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
                SetupComplete = true;
            }
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            MyInventory inv = (MyInventory)tank.GetInventory();
            MyFixedPoint newExternalMass = (MyFixedPoint)((tank.FilledRatio * tank.Capacity) * massMultiplier);

            // disable extra mass for NPC grids, if needed
            if (HeavyGasSession.EnableNPCs == false && NPCOwned == true)
            {
                newExternalMass = 0;
            }

            // update external mass
            if (inv != null && newExternalMass != inv.ExternalMass)
            {
                inv.ExternalMass = newExternalMass;
                inv.Refresh();
            }
        }
    }
}
