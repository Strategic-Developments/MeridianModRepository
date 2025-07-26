using Math0424.AnimationCore;
using Math0424.Networking;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Localization;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRage.Voxels;
using VRageMath;

namespace ResourceNodes
{
    abstract class DrillBlock : MyAbstractAnimatedBlock
    {

        private static MyDefinitionId EId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

        private static bool InitializedTerminalGroups = false;

        protected IMyFunctionalBlock Blocc;
        protected IMyInventory Inventory;

        protected int tick = -1;
        protected int timesChecked = 0;
        protected int slowdown = 1;
        public bool IsProducing;
        protected bool InvFull, InGround;
        public MyVoxelMaterialDefinition myOre = null;
        public List<MyVoxelMaterialDefinition> options = new List<MyVoxelMaterialDefinition>();
        public List<MyTuple<string, int>> optionsClient = new List<MyTuple<string, int>>();
        protected Action DepositedResources;

        protected float baseSpeed = 10;
        protected int invMultiplier = 8;

        private readonly Dictionary<byte, int> materials = new Dictionary<byte, int>();

        public DrillStateUpdate state;

        public abstract void SetEmissive(Color color);

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            LoadOntoBlock();

            Blocc = (IMyFunctionalBlock)Block;

            try
            {
                if (Blocc.UpgradeValues.ContainsKey("Productivity"))
                    Blocc.UpgradeValues.Remove("Productivity");
                Blocc.UpgradeValues.Add("Productivity", 1f);

                if (Blocc.UpgradeValues.ContainsKey("Effectiveness"))
                    Blocc.UpgradeValues.Remove("Effectiveness");
                Blocc.UpgradeValues.Add("Effectiveness", 1f);
            }
            catch { }

            Block.OnClose += RemoveFromMiners;
            Blocc.AppendingCustomInfo += CustomInfo;

            BlockInit();
        }

        public abstract void BlockInit();

        public override void BeforeFirstUpdate()
        {
            ResourceNode.Instance.Blocks[Blocc.EntityId] = this;

            if (MyAPIGateway.Session.IsServer)
            {
                MyInventory component = new MyInventory(invMultiplier, new Vector3(1), MyInventoryFlags.CanSend);

                foreach (var i in Inv?.GetItems())
                {
                    if (i != null)
                    {
                        component.AddItems(i.Amount, i.Content);
                    }
                }
                Block.Components.Remove(typeof(MyInventoryBase));
                Block.Components.Add<MyInventoryBase>(component);
            }
            ((MyInventory)Inv).Constraint = new MyInventoryConstraint(MySpaceTexts.ToolTipItemFilter_AnyOre, null, true).AddObjectBuilderType(typeof(MyObjectBuilder_Ore));
        }
        public void SetOreFromString(string ore)
        {
            if (ore == "")
                return;

            foreach (var option in options)
            {
                if (option.MinedOre == ore)
                {
                    myOre = option;

                    if (MyAPIGateway.Multiplayer.IsServer)
                    {
                        Blocc.CustomData = myOre.MinedOre;
                    }
                    break;
                }
            }
        }
        public override void MarkForClose()
        {
            ResourceNode.Instance.Blocks.Remove(Blocc.EntityId);
        }
        public static bool IsVisible(IMyTerminalBlock b)
        {
            return ResourceNode.Instance.Blocks.ContainsKey(b.EntityId);
        }
        public static void AddTerminalOptions()
        {
            var list = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyShipDrill>("ResourceNodes_ListBox");
            list.Title = MyStringId.GetOrCompute("Ore to Mine:");
            //c.Tooltip = MyStringId.GetOrCompute("This does some stuff!"); // presenece of this tooltip prevents per-item tooltips
            list.SupportsMultipleBlocks = true;
            list.Visible = IsVisible;

            list.VisibleRowsCount = 3;
            list.Multiselect = false; // wether player can select muliple at once (ctrl+click, click&shift+click, etc)
            list.ListContent = (b, content, preSelect) =>
            {
                DrillBlock block;
                if (ResourceNode.Instance.Blocks.TryGetValue(b.EntityId, out block))
                {
                    if (MyAPIGateway.Multiplayer.IsServer)
                    {
                        if (block?.options == null)
                            return;

                        foreach (var ore in block.options)
                        {
                            double amount = block.baseSpeed * block.Blocc.UpgradeValues["Productivity"] * block.Blocc.UpgradeValues["Effectiveness"] * ore.MinedOreRatio * ore.MinedOreRatio;

                            if (ore.MinedOre == "Stone")
                            {
                                amount *= 25;
                            }

                            var item = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute($"{ore.MinedOre}"),
                                                                        tooltip: MyStringId.GetOrCompute($"{amount} per 100 ticks"),
                                                                        userData: ore.MinedOre); // userData can be whatever you wish and it's retrievable in the ItemSelected call.

                            content.Add(item);

                            if (ore == block.myOre)
                            {
                                preSelect.Add(item);
                            }
                        }
                    }
                    else
                    {
                        if (block?.optionsClient == null)
                            return;

                        foreach (var ore in block.optionsClient)
                        {

                            var item = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute($"{ore.Item1}"),
                                                                        tooltip: MyStringId.GetOrCompute($"{ore.Item2} per 100 ticks"),
                                                                        userData: ore.Item1); // userData can be whatever you wish and it's retrievable in the ItemSelected call.

                            content.Add(item);

                            if (ore.Item1 == block.state?.oreName)
                            {
                                preSelect.Add(item);
                            }
                        }
                    }
                }

                // this is the getter, gets called when the list needs to be shown/refreshed.
                // the 2 lists in the parameters are there for you to fill:
                //   `content` with the options to show
                //   `preSelect` with the options to be already selected (only needed if you want to persist selections).
                // NOTE: `preSelect` requires the same instance(s) as the ones given to `content`, giving it new MyTerminalControlListBoxItem would not work.

                
            };
            list.ItemSelected = (b, selected) =>
            {
                DrillBlock block;
                if (ResourceNode.Instance.Blocks.TryGetValue(b.EntityId, out block) && selected.Count > 0)
                {
                    block.SetOreFromString(selected[0].UserData as string);
                    if (!MyAPIGateway.Multiplayer.IsServer)
                        ResourceNode.Instance.Network.TransmitToServer(new TerminalUpdate() { blockId = b.EntityId, oreName = selected[0].UserData as string }, false);
                }
            };

            MyAPIGateway.TerminalControls.AddControl<IMyShipDrill>(list);
        }
        public override void GameUpdate()
        {
            if (!InitializedTerminalGroups)
            {
                InitializedTerminalGroups = true;
                AddTerminalOptions();
            }

            if (!MyAPIGateway.Session.IsServer)
                return;


            tick++;
            if (!Blocc.CubeGrid.IsStatic)
            {
                Blocc.Enabled = false;
            }

            if (tick % 1000 == 0 && timesChecked < 20 && myOre == null)
            {


                materials.Clear();
                List<MyVoxelBase> detected = new List<MyVoxelBase>();
                Vector3D position = Block.PositionComp.GetPosition() + (Block.PositionComp.WorldMatrixRef.Down * (Block.BlockDefinition.Size.Y + .25));
                BoundingSphereD boundingSphereD = new BoundingSphereD(position, 4);
                MyGamePruningStructure.GetAllVoxelMapsInSphere(ref boundingSphereD, detected);
                foreach (var map in detected)
                {
                    GetResources(position, map);
                }
                if (materials.Count >= 1)
                {
                    InGround = true;
                    AssignNewMaterial();
                }
                timesChecked++;
            }

            

            IsProducing = Blocc.Enabled && Blocc.IsWorking && !Inv.IsFull && InGround;

            if (IsProducing)
            {
                IsProducing = Block.ResourceSink.IsPoweredByType(EId) && Block.ResourceSink.IsPowerAvailable(EId, Block.ResourceSink.MaxRequiredInput);
            }

            if (tick % 10 == 0)
            {
                if (optionsClient == null)
                {
                    optionsClient = new List<MyTuple<string, int>>();
                }
                else
                {
                    optionsClient.Clear();
                }

                foreach (var option in options)
                {
                    optionsClient.Add(new MyTuple<string, int>
                    {
                        Item1 = option.MinedOre,
                        Item2 = (int)(baseSpeed * Blocc.UpgradeValues["Productivity"] * Blocc.UpgradeValues["Effectiveness"] * option.MinedOreRatio * option.MinedOreRatio * (option.MinedOre == "Stone" ? 25 : 1))
                    });
                }

                DrillStateUpdate packet = new DrillStateUpdate
                {
                    blockId = Block.EntityId,
                    isInGround = InGround,
                    invFull = InvFull,
                    oreName = myOre?.MinedOre ?? "nothing",
                    isProducing = IsProducing,
                    oreList = optionsClient
                };

                ResourceNode.Instance.Network.TransmitToPlayersWithinRange(Block.PositionComp.GetPosition(), packet, 1500, false);

                if (Block.IsBuilt)
                {
                    if (!Block.IsFunctional)
                    {
                        SetEmissive(Color.Orange);
                    }
                    else
                    {
                        if (!InGround || !Blocc.Enabled || !Blocc.IsWorking)
                        {
                            SetEmissive(Color.Red);
                        }
                        else if (!IsProducing || myOre == null || Inv.IsFull || InvFull)
                        {
                            SetEmissive(Color.Yellow);
                        }
                    }
                }
                else
                {
                    if (!InGround)
                    {
                        SetEmissive(Color.Red);
                    }
                    else if (myOre != null)
                    {
                        SetEmissive(Color.Aqua);
                    }
                    else
                    {
                        SetEmissive(Color.Yellow);
                    }
                }
            }

            if (IsProducing && myOre != null && tick % 100 == 0)
            {
                float speed = baseSpeed * Block.UpgradeValues["Productivity"];
                float yield = speed * Block.UpgradeValues["Effectiveness"]; // lmao
                MyObjectBuilder_Ore oreObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(myOre.MinedOre);

                double amount = yield * myOre.MinedOreRatio * myOre.MinedOreRatio;

                if (myOre.MinedOre == "Stone")
                {
                    amount *= 25;
                }

                InvFull = !Inv.CanItemsBeAdded((MyFixedPoint)amount, oreObject);

                if (!InvFull)
                {
                    Inv.AddItems((MyFixedPoint)amount, oreObject);
                    DepositedResources?.Invoke();
                }
            }
        }

        private void AssignNewMaterial()
        {
            //get all the materials
            for(int i = 0; i < 50; i++)
            {
                List<MyVoxelBase> detected = new List<MyVoxelBase>();
                Vector3D position = Block.PositionComp.GetPosition() + Block.PositionComp.WorldMatrixRef.Down * i * 3;
                BoundingSphereD boundingSphereD = new BoundingSphereD(position, 10);
                MyGamePruningStructure.GetAllVoxelMapsInSphere(ref boundingSphereD, detected);
                foreach (var map in detected)
                {
                    GetResources(position, map);
                }
            }

            //sort materials and pick ores
            Dictionary<MyVoxelMaterialDefinition, int> optionsDict = new Dictionary<MyVoxelMaterialDefinition, int>();
            foreach (var m in materials.Keys)
            {
                var def = MyDefinitionManager.Static.GetVoxelMaterialDefinition(m);
                if (def != null)
                {
                    if (def.CanBeHarvested && !string.IsNullOrEmpty(def.MinedOre) && !ResourceNode.Instance.MiningBlacklist.Contains(def.MinedOre))
                    {
                        optionsDict.Add(def, materials[m]);
                    }
                }
            }

            if (optionsDict.Count == 0 && materials.Count >= 1)
            {
                var e = materials.Keys.GetEnumerator();
                e.MoveNext();
                var def = MyDefinitionManager.Static.GetVoxelMaterialDefinition(e.Current);
                if (def != null)
                {
                    optionsDict.Add(def, materials[e.Current]);
                }
            }

            //pick top value and add it to the producer
            MyVoxelMaterialDefinition top = null;
            foreach (var m in optionsDict)
            {
                if (top == null)
                {
                    top = m.Key;
                } 
                else if (optionsDict[top] * top.MinedOreRatio < m.Value * m.Key.MinedOreRatio || !top.IsRare)
                {
                    top = m.Key;
                }
                bool found = false;
                for (int i = options.Count - 1; i >= 0; i--)
                {
                    var voxel = options.ElementAt(i);

                    if (voxel.MinedOre == m.Key.MinedOre)
                    {

                        if (voxel.MinedOreRatio < m.Key.MinedOreRatio)
                        {
                            options.RemoveAtFast(i);
                        }
                        else
                        {
                            found = true;
                        }
                        break;
                    }
                }
                if (!found)
                {
                    options.Add(m.Key);
                }
                
            }

            RemoveFromMiners(Block);

            SetOreFromString(Blocc.CustomData);

            if (myOre == null)
            {
                myOre = top;
                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    Blocc.CustomData = myOre.MinedOre;
                }
            }
            AddToMiners();
        }

        private void CustomInfo(IMyTerminalBlock block, StringBuilder builder)
        {
            if (state != null)
            {
                builder.Clear();
                builder.Append($"\nCurrently extracting {state.oreName}");
                builder.Append($"\nIs producing: {state.isProducing}");
                builder.Append($"\nIn ground: {state.isInGround}");
                builder.Append($"\nInventory full: {state.invFull}");
            }
        }

        private void RemoveFromMiners(MyEntity e)
        {
            if (myOre != null && !string.IsNullOrEmpty(myOre.MinedOre))
            {
                if (ResourceNode.Instance.Miners.ContainsKey(myOre.MinedOre))
                {
                    ResourceNode.Instance.Miners[myOre.MinedOre].Remove(e.EntityId);
                    ResourceNode.Instance.Locations.Remove(e.EntityId);
                }
            }
        }

        private void AddToMiners()
        {
            if (myOre != null && !string.IsNullOrEmpty(myOre.MinedOre))
            {
                if (!ResourceNode.Instance.Miners.ContainsKey(myOre.MinedOre))
                {
                    ResourceNode.Instance.Miners.Add(myOre.MinedOre, new HashSet<long>());
                }
                ResourceNode.Instance.Miners[myOre.MinedOre].Remove(Block.EntityId);
                ResourceNode.Instance.Miners[myOre.MinedOre].Add(Block.EntityId);

                ResourceNode.Instance.Locations.Remove(Block.EntityId);
                ResourceNode.Instance.Locations.Add(Block.EntityId, Block.PositionComp.GetPosition());
            }
        }

        private void GetResources(Vector3D pos, MyVoxelBase map)
        {
            MyStorageData cache = new MyStorageData(MyStorageDataTypeFlags.ContentAndMaterial);
            cache.Resize(new Vector3I(1));

            Vector3I voxelPos;
            MyVoxelCoordSystems.WorldPositionToVoxelCoord(map.PositionLeftBottomCorner, ref pos, out voxelPos);
            map.Storage.ReadRange(cache, MyStorageDataTypeFlags.ContentAndMaterial, 0, voxelPos, voxelPos);

            if (cache.Material(0) != 255)
            {
                if (materials.ContainsKey(cache.Material(0)))
                {
                    materials[cache.Material(0)] += cache.Content(0);
                }
                else
                {
                    materials.Add(cache.Material(0), cache.Content(0));
                }
            }
        }

        protected IMyInventory Inv
        {
            get { return Blocc.GetInventory(0); }
        }

    }
}
