using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using SpaceEngineers.Game.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game;
using VRage;

namespace NoSZInRad
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SafezoneMain : MySessionComponentBase
    {
        private List<IMySafeZoneBlock> Safezones;
        public static List<MyTuple<BoundingSphereD, bool>> NoSafezoneWorkLocales;
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            const int REGULAR_REPLOSS_ZONE = 30;
            const int PIRATE_REPLOSS_ZONE = 15;
            NoSafezoneWorkLocales = new List<MyTuple<BoundingSphereD, bool>>();
            Safezones = new List<IMySafeZoneBlock>();

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
            }

            AddNew("GPS:CCAS Nairobi SLC:297432:50771:2065996:#FFF19F75:NPC Stations:", REGULAR_REPLOSS_ZONE, true);
            AddNew("GPS:CCAS Tripoli SLC:165635:7284:2195716:#FFF19F75:NPC Stations:", REGULAR_REPLOSS_ZONE, true);
            AddNew("GPS:CSILLA Regional Landing:2415608:-19793:-17159:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE, true);
            AddNew("GPS:CSILLA Vekan Gatehold:2247864:33592:6981:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE, true);
            AddNew("GPS:ENCORP DuPont Transit:-2422618:-20256:598156:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE, true);
            AddNew("GPS:ENCORP Vanderbilt Station:-2335929:70355:444812:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE, true);

            AddNew("GPS:CSILLA Redwatch Outpost:-2588616:57608:-409448:#FFFF0000:NPC Stations:", PIRATE_REPLOSS_ZONE, true);
            AddNew("GPS:CSILLA Vault 9 Outpost:992580:69034:2386727:#FFFF0000:NPC Stations:", PIRATE_REPLOSS_ZONE, true);

            //AddNew("GPS:Kimi:2702071:-297911:-948008:#FFF9F9F9:Planets:", 5, false);
            //AddNew("GPS:Caerus:-2634367:65652:-434416:#FFFF0000:Planets:", 100, false);
            //AddNew("GPS:Deimos:965725:65306:2415429:#FFFF0000:Planets:", 100, false);
            //AddNew("GPS:Thanatos:331166:130897:-6369487:#FFF9F9F9:Planets:", 150, false);
            
        }

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session.GameplayFrameCounter % 100 == 69 && MyAPIGateway.Multiplayer.IsServer)
            {
                foreach (var self in Safezones)
                {
                    foreach (var sphere in NoSafezoneWorkLocales)
                    {
                        if (Vector3.DistanceSquared(self.GetPosition(), sphere.Item1.Center) <= sphere.Item1.Radius * sphere.Item1.Radius)
                        {
                            if (self.Enabled)
                                self.Enabled = false;
                            break;
                        }
                    }
                }
            }

            if (!MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Session?.Player?.Character != null)
            {
                foreach (var sphere in NoSafezoneWorkLocales)
                {
                    if (Vector3D.DistanceSquared(MyAPIGateway.Session.Player.Character.GetPosition(), sphere.Item1.Center) <= sphere.Item1.Radius * sphere.Item1.Radius)
                    {
                        MyAPIGateway.Utilities.ShowNotification("Warning: You are in an economic zone. Remember to disable your weapons!", 1, "Red");
                    }
                    else if (Vector3D.DistanceSquared(MyAPIGateway.Session.Player.Character.GetPosition(), sphere.Item1.Center) <= sphere.Item1.Radius * sphere.Item1.Radius * 2.25 /* 1.5^2*/)
                    {
                        MyAPIGateway.Utilities.ShowNotification("Warning: You are near an economic zone. Ensure any weapons fire does not go into it!", 1, "Red");
                    }
                }
                
            }
        }
        private void OnEntityAdd(IMyEntity obj)
        {
            if (obj is IMyCubeGrid)
            {
                if (obj.Physics == null)
                {
                    return;
                }
                IMyCubeGrid grid = obj as IMyCubeGrid;
                grid.OnBlockAdded += CubeGrid_OnBlockAdded;
                grid.OnBlockRemoved += Grid_OnBlockRemoved;
                grid.OnClose += CubeGrid_OnClose;

                foreach (var block in ((MyCubeGrid)grid).GetFatBlocks())
                {
                    OnBlockAddedInit(block);
                }
            }
        }

        private void Grid_OnBlockRemoved(IMySlimBlock obj)
        {
            if (obj.FatBlock != null && obj.FatBlock is IMySafeZoneBlock
                && obj.BlockDefinition.Id.SubtypeName == "meridian_safezone_base")
            {
                Safezones.Remove(obj.FatBlock as IMySafeZoneBlock);
            }
        }

        private void CubeGrid_OnClose(IMyEntity obj)
        {
            ((IMyCubeGrid)obj).OnBlockAdded -= CubeGrid_OnBlockAdded;
            ((IMyCubeGrid)obj).OnBlockRemoved -= Grid_OnBlockRemoved;
            ((IMyCubeGrid)obj).OnClose -= CubeGrid_OnClose;
        }
        private void OnBlockAddedInit(IMyCubeBlock obj)
        {
            if (obj != null && obj is IMySafeZoneBlock 
                && obj.SlimBlock.BlockDefinition.Id.SubtypeName == "meridian_safezone_base")
            {
                Safezones.Add(obj as IMySafeZoneBlock);
            }
        }
        private void CubeGrid_OnBlockAdded(IMySlimBlock obj)
        {
            if (obj.FatBlock != null && obj.FatBlock is IMySafeZoneBlock
                && obj.BlockDefinition.Id.SubtypeName == "meridian_safezone_base")
            {
                Safezones.Add(obj.FatBlock as IMySafeZoneBlock);
            }
        }
        protected override void UnloadData()
        {
            NoSafezoneWorkLocales = null;
            Safezones = null;
            if (MyAPIGateway.Multiplayer.IsServer)
                MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdd;
            
        }

        private static void AddNew(string stationGPS, float distKm, bool isStation)
        {
            NoSafezoneWorkLocales.Add(new MyTuple<BoundingSphereD, bool>(new BoundingSphereD(ParseGPS(stationGPS), distKm * 1000), isStation));
        }

        private static Vector3D ParseGPS(string gpsString)
        {
            var gpsStringSplit = gpsString.Split(':');

            double x, y, z;

            if (gpsStringSplit.Length <= 6)
                return Vector3D.Zero;

            bool passX = double.TryParse(gpsStringSplit[2], out x);
            bool passY = double.TryParse(gpsStringSplit[3], out y);
            bool passZ = double.TryParse(gpsStringSplit[4], out z);

            if (passX && passY && passZ)
            {
                return new Vector3D(x, y, z);
            }
            else
                return Vector3D.Zero;
        }
    }
}

