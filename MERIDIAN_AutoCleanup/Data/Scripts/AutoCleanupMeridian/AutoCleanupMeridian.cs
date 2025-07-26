using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Sandbox.Game.EntityComponents;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using VRage.Game.Models;
using VRage.Render.Particles;
using System.Linq.Expressions;
using System.IO;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.Weapons;
using VRage;
using VRage.Collections;
using VRage.Voxels;
using ProtoBuf;
using System.Collections.Concurrent;
using VRage.Serialization;
using Sandbox.Engine.Physics;
using Sandbox.Game.GameSystems;
using System.Data;

namespace AutoCleanup
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class AutoCleanupMeridian : MySessionComponentBase
    {
        private List<string> RequiredSubtypeIds;
        public static int Ticks => MyAPIGateway.Session.GameplayFrameCounter;
        public override void LoadData()
        {
            RequiredSubtypeIds = new List<string>()
            {
                "IffReflector",
                "TorpIFF",
                "IffReflector_Small",
            };
        }
        private bool IsRequiredBlockPresent = false;
        readonly List<IMyCubeGrid> gridDumpList = new List<IMyCubeGrid>();
        readonly List<IHitInfo> hits = new List<IHitInfo>();
        public override void BeforeStart()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
            {
                HashSet<IMyEntity> gridsToDelete = new HashSet<IMyEntity>();


                MyAPIGateway.Entities.GetEntities(gridsToDelete, IsDeletable);

                foreach (var ent in gridsToDelete)
                {
                    ent.Close();
                }
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (!MyAPIGateway.Utilities.IsDedicated && Ticks % 100 == 0)
            {
                MatrixD? m = MyAPIGateway.Session?.Player?.Character?.WorldMatrix;

                if (m != null)
                {
                    MyAPIGateway.Physics.CastRay(m.Value.Translation, m.Value.Translation + m.Value.Forward * 200, hits);

                    if (hits.Count > 0)
                    {
                        IHitInfo info = hits[0];

                        IMyCubeGrid grid = info.HitEntity as IMyCubeGrid;

                        if (grid != null)
                        {
                            if (IsDeletable(grid))
                            {
                                MyAPIGateway.Utilities.ShowNotification("Warning! Grid will be deleted due to either no IFF, no power, or its name.", 1000 * 100 / 60, "Red");
                            }
                        }
                    }

                    hits.Clear();
                }
            }
        }
        private bool IsDeletable(IMyEntity ent)
        {
            if (!(ent is IMyCubeGrid))
            {
                return false;
            }

            IMyCubeGrid g = ent as IMyCubeGrid;
            gridDumpList.Clear();
            g.GetGridGroup(GridLinkTypeEnum.Mechanical).GetGrids(gridDumpList);

            bool IsNamed = false;
            foreach (var grid in gridDumpList)
            {
                if (!grid.CustomName.StartsWith("Static Grid") && !grid.CustomName.StartsWith("Small Grid") && !grid.CustomName.StartsWith("Large Grid"))
                {
                    IsNamed = true;
                    break;
                }
            }

            if (!IsNamed)
            {
                return true;
            }

            bool BlockPresentAndPowered = false;
            foreach (var grid in gridDumpList)
            {
                IsRequiredBlockPresent = false;
                grid.GetBlocks(null, CheckGrid);

                if (IsRequiredBlockPresent)
                {
                    BlockPresentAndPowered = true;
                    break;
                }
            }

            return !BlockPresentAndPowered;
        }

        public bool CheckGrid(IMySlimBlock block)
        {
            if (RequiredSubtypeIds.Contains(block.BlockDefinition.Id.SubtypeName))
            {
                IsRequiredBlockPresent = true;
            }

            return false;
        }
    }
}
