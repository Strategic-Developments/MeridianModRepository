using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;


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
        bool doFirstCleanup = true;
        public override void UpdateBeforeSimulation()
        {
            if (doFirstCleanup)
            {
                Cleanup();
                doFirstCleanup = false;
            }

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
            var grids = new List<IMyCubeGrid>();
            g.GetGridGroup(GridLinkTypeEnum.Logical).GetGrids(grids);
            //MyLog.Default.WriteLineAndConsole($"Grid {g.CustomName} with {grids.Count} grid tested to cleanup.");
            if (grids.Count == 1 && ((MyCubeGrid)g).BlocksCount == 1)
            {
                foreach (var wheel in g.GetFatBlocks<IMyWheel>())
                {
                    if (wheel.IsAttached)
                    {
                        //MyLog.Default.WriteLineAndConsole($"Grid {g.CustomName} is an attached wheel, returning false!");
                        return false;
                    }
                }
            }
            bool hasIff = false, hasName = false;
            foreach (var grid in grids)
            {
                var blocks = new List<IMySlimBlock>();
                grid.GetBlocks(blocks, CheckGrid);

                // the two don't have to be on the same grid
                if (blocks.Count > 0)
                {
                    //MyLog.Default.WriteLineAndConsole($"Grid {grid.CustomName}, part of {g.CustomName}'s grid group has IFF.");
                    hasIff = true;
                }
                if (!grid.CustomName.StartsWith("Static Grid") &&
                    !grid.CustomName.StartsWith("Small Grid") &&
                    !grid.CustomName.StartsWith("Large Grid"))
                {
                    //MyLog.Default.WriteLineAndConsole($"Grid {grid.CustomName}, part of {g.CustomName}'s grid group is named.");
                    hasName = true;
                }

                if (hasIff && hasName)
                {
                    return false;
                }
            }
            //MyLog.Default.WriteLineAndConsole($"Grid {g.CustomName} is {(hasName ? "named" : "unnamed")} and {(hasIff ? "has an IFF" : "has no IFF")}, returning true!");
            return true;
        }

        private bool CheckGrid(IMySlimBlock block)
        {
            return _requiredSubtypeIds.Contains(block.BlockDefinition.Id.SubtypeName);
        }
    }
}
