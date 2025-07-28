using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;


namespace AutoCleanup
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class AutoCleanupMeridian : MySessionComponentBase
    {
        private readonly List<string> _requiredSubtypeIds = new List<string>(){
            "IffReflector",
            "TorpIFF",
            "IffReflector_Small",
        };

        private static int Ticks => MyAPIGateway.Session.GameplayFrameCounter;
        
        public override void BeforeStart()
        {
            //Cleanup();
        }

        private void Cleanup()
        {
            if (!MyAPIGateway.Utilities.IsDedicated) return;
            var gridsToDelete = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(gridsToDelete, IsDeletable);
            foreach (var ent in gridsToDelete)
            {
                ent.Close();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Utilities.IsDedicated || Ticks % 100 != 0) return;
            var m = MyAPIGateway.Session?.Player?.Character?.WorldMatrix;

            if (m == null) return;
            var hits = new List<IHitInfo>();
            MyAPIGateway.Physics.CastRay(m.Value.Translation, m.Value.Translation + m.Value.Forward * 200, hits);

            if (hits.Count == 0) return;
            var grid = hits[0].HitEntity as IMyCubeGrid;
            if (grid != null && IsDeletable(grid))
            {
                MyAPIGateway.Utilities.ShowNotification(
                    "Warning! Grid will be deleted due to either no IFF, no power, or its name.",
                    1000 * 100 / 60, "Red");
            }
        }
        private bool IsDeletable(IMyEntity ent)
        {
            var g = ent as IMyCubeGrid;
            if (g == null)
            {
                return false;
            }
            var gridDumpList = new List<IMyCubeGrid>();
            g.GetGridGroup(GridLinkTypeEnum.Logical).GetGrids(gridDumpList);

            if (gridDumpList.Count == 1 && ((MyCubeGrid)g).BlocksCount == 1)
            {
                var wheels = g.GetFatBlocks<IMyWheel>();

                foreach (var wheel in wheels)
                {
                    if (wheel.IsAttached)
                        return false;
                }
            }
            
            foreach (var grid in gridDumpList)
            {
                var blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks, CheckGrid);

                if (blocks.Count > 0 &&
                    !grid.CustomName.StartsWith("Static Grid") && 
                    !grid.CustomName.StartsWith("Small Grid") && 
                    !grid.CustomName.StartsWith("Large Grid")) 
                    return false; 
            }
            return true;
        }

        private bool CheckGrid(IMySlimBlock block)
        {
            return _requiredSubtypeIds.Contains(block.BlockDefinition.Id.SubtypeName);
        }
    }
}
