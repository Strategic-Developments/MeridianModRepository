using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using SpaceEngineers.Game.ModAPI;

namespace NoSZInRad
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SafeZone), false)]
    public class Safezone : MyGameLogicComponent
    {
        private static List<BoundingSphereD> NoSafezoneWorkLocales;
        private IMySafeZoneBlock self;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                //NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            self = Entity as IMySafeZoneBlock;
            if (self?.CubeGrid?.Physics == null)
            {
                return;
            }

            if (NoSafezoneWorkLocales == null)
                InitLocales();

            NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            if (NoSafezoneWorkLocales == null)
                InitLocales();

            foreach (var sphere in NoSafezoneWorkLocales)
            {
                if (Vector3.DistanceSquared(self.GetPosition(), sphere.Center) <= sphere.Radius * sphere.Radius)
                {
                    if (self.Enabled)
                        self.Enabled = false;
                    break;
                }
            }
        }

        private static void InitLocales()
        {
            const int REGULAR_REPLOSS_ZONE = 30;
            const int PIRATE_REPLOSS_ZONE = 15;
            NoSafezoneWorkLocales = new List<BoundingSphereD>();
            //AddNew("GPS:CCAS Nairobi SLC:297432:50771:2065996:#FFF19F75:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:CCAS Tripoli SLC:165635:7284:2195716:#FFF19F75:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:CSILLA Regional Landing:2415608:-19793:-17159:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:CSILLA Vekan Gatehold:2247864:33592:6981:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:ENCORP DuPont Transit:-2422618:-20256:598156:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:ENCORP Vanderbilt Station:-2335929:70355:444812:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE);

            //AddNew("GPS:CSILLA Regional Landing:2415608:-19793:-17159:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:CSILLA Vekan Gatehold:2247864:33592:6981:#FFFF6A6A:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:CSILLA Redwatch Outpost:-2588616:57608:-409448:#FFFF0000:NPC Stations:", PIRATE_REPLOSS_ZONE);
            //AddNew("GPS:CSILLA Vault 9 Outpost:992580:69034:2386727:#FFFF0000:NPC Stations:", PIRATE_REPLOSS_ZONE);

            //AddNew("GPS:ENCORP DuPont Transit:-2422618:-20256:598156:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE);
            //AddNew("GPS:ENCORP Vanderbilt Station:-2335929:70355:444812:#FF757FF1:NPC Stations:", REGULAR_REPLOSS_ZONE);
        }   

        private static void AddNew(string stationGPS, float distKm)
        {
            NoSafezoneWorkLocales.Add(new BoundingSphereD(ParseGPS(stationGPS), distKm * 1000));
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
