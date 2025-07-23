using System;
using System.Collections.Generic;
using System.Linq;
using ObjectBuilders.SafeZone;
using ProtoBuf;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Engine.Physics;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.Entities.Blocks.SafeZone;
using SpaceEngineers.Game.Entities.Weapons;
using SpaceEngineers.Game.ModAPI;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Klime.Pipeline
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CargoContainer), false, "Pipeline_Cargo")]
    public class Pipeline : MyGameLogicComponent
    {
        //public static bool initControl = false;

        IMyCargoContainer cargo_block;
        IMyCargoContainer other_cargo_block;
        MyInventory cargo_inventory;
        MyInventory other_cargo_inventory;
        public enum BlockState
        {
            Idle,
            Searching,
            Connected
        }

        public enum OverallAnimationState
        {
            Idle,
            AnimStart,
            AnimContinue,
            AnimEnd
        }
        
        BlockState server_block_state = BlockState.Idle;
        BlockState client_block_state = BlockState.Idle;
        List<IHitInfo> all_hits = new List<IHitInfo>();
        
        int frame = 0;
        int frameOffset = 0;
        int updateInventoryPeriodFrames = 600; // 10 seconds
        int failedSearches = 0;
        int searchCooldownFrames = 0;
        int searchCooldownFailMultiple = 200;
        int searchCooldownMax = 1200; // 20 seconds

        double search_radius = 2000;
        double search_angle_tolerence = 0.2; //in radians, not degrees
        List<MyEntity> search_ents = new List<MyEntity>();
        List<IMyCargoContainer> search_onlycargo = new List<IMyCargoContainer>();
        Dictionary<IMyCargoContainer, Vector3D> search_Positions = new Dictionary<IMyCargoContainer, Vector3D>();
        MyStringId pipeline_mat;
        Color search_col = Color.LightGreen;
        Vector4 for_col = Color.LightGreen;
        List<IMyPlayer> all_players = new List<IMyPlayer>();
        public ushort netId = 19593;
        List<VisualChunk> allChunks = new List<VisualChunk>();

        List<string> allModels = new List<string>();
        MatrixD cone_mat = MatrixD.Identity;

        public class VisualChunk
        {
            public MyEntity chunk;
            public int current_lifetime;
            public Vector3D current_velocity;
            public double chunkLength;
            public VisualChunk(MyEntity chunk, double chunkLength)
            {
                this.chunk = chunk;
                current_lifetime = 0;
                current_velocity = Vector3D.Zero;
                this.chunkLength = chunkLength;
            }

        }


        [ProtoContract]
        public class Packet
        {
            [ProtoMember(50)]
            public BlockState incoming_block_state;

            [ProtoMember(51)]
            public long incoming_cargo_block_id;

            [ProtoMember(52)]
            public long incoming_othercargo_id;

            public Packet()
            {

            }

            public Packet(BlockState incoming_block_state, long incoming_cargo_block_id, long incoming_othercargo_id)
            {
                this.incoming_block_state = incoming_block_state;
                this.incoming_cargo_block_id = incoming_cargo_block_id;
                this.incoming_othercargo_id = incoming_othercargo_id;
            }


        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            cargo_block = Entity as IMyCargoContainer;
            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (cargo_block != null)
            {
                if (cargo_block.CubeGrid.Physics != null)
                {
                    //Add rest of pipeline mats
                    pipeline_mat = MyStringId.GetOrCompute("Square");
                    search_col.A = (byte)50;
                    var cb = cargo_block as MyCubeBlock;
                    if (cb != null)
                    {
                        string modpath = cb.BlockDefinition.Context.ModPath;
                        allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_125.mwm");
                        allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_25.mwm");
                        allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_5.mwm");
                        allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_1.mwm");

                        //ModelPath = cb.BlockDefinition.Context.ModPath + @"\Models\pipeline.mwm";
                        //ModelPath = @"C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Models\Cubes\large\ConveyorTube.mwm";
                        //MyVisualScriptLogicProvider.SendChatMessage(ModelPath);
                    }


                    if (MyAPIGateway.Session.IsServer)
                    {
                        frameOffset = MyUtils.GetRandomInt(0, 59);
                    }

                    //if (!initControl)
                    //{
                    //    List<IMyTerminalControl> controls = new List<IMyTerminalControl>();
                    //    MyAPIGateway.TerminalControls.GetControls<IMyCargoContainer>(out controls);
                    //    foreach (var control in controls)
                    //    {
                    //        if (control.Id == "ShowInTerminal")
                    //        {
                    //            var onOff = control as IMyTerminalControlOnOffSwitch;
                    //            if (onOff != null)
                    //            {
                    //                onOff.Title = CheckTitle();
                    //                onOff.vi
                    //            }
                    //        }
                    //    }

                    //    initControl = true;
                    //}

                    NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(netId, pipeline_net_handler);
                    //MyVisualScriptLogicProvider.AddGPSForAll("", "", cargo_block.WorldMatrix.Translation, Color.Orange);
                }
            }
        }

        private void pipeline_net_handler(ushort arg1, byte[] arg2, ulong arg3, bool arg4)
        {
            Packet incoming_packet = MyAPIGateway.Utilities.SerializeFromBinary<Packet>(arg2);
            if (incoming_packet != null && incoming_packet.incoming_cargo_block_id == cargo_block.EntityId)
            {
                BlockState previous_block_state = client_block_state;
                client_block_state = incoming_packet.incoming_block_state;

                if (client_block_state == BlockState.Connected)
                {
                    other_cargo_block = MyAPIGateway.Entities.GetEntityById(incoming_packet.incoming_othercargo_id) as IMyCargoContainer;
                }
                else
                {
                    other_cargo_block = null;
                }

                if (other_cargo_block == null || other_cargo_block.MarkedForClose)
                {
                    client_block_state = BlockState.Idle;
                }

                if (client_block_state == BlockState.Connected && previous_block_state != BlockState.Connected)
                {
                    //MyVisualScriptLogicProvider.SendChatMessage("Trig");
                    //MyVisualScriptLogicProvider.SendChatMessage(previous_block_state.ToString());
                    NewChain();
                }

                if (client_block_state != BlockState.Connected && previous_block_state == BlockState.Connected)
                {
                    CleanupChain();
                }
            }
        }

        private void NewChain()
        {
            try
            {
                if (cargo_block != null && other_cargo_block != null)
                {
                    var cargoPos = cargo_block.WorldMatrix.Translation + cargo_block.WorldMatrix.Forward * 0f;
                    var otherPos = other_cargo_block.WorldMatrix.Translation + other_cargo_block.WorldMatrix.Backward * 0f;
                    var vector_between = otherPos - cargoPos;
                    //var vector_between = (other_cargo_block.WorldMatrix.Translation + other_cargo_block.WorldMatrix.Backward*1.6f) 
                    //    - (cargo_block.WorldMatrix.Translation + cargo_block.WorldMatrix.Forward*1.6f);
                    var distance = vector_between.Length();

                    double remaining_distance = distance;

                    int number_of_125 = (int)(remaining_distance / (125 * 2.5));
                    remaining_distance -= number_of_125 * (125 * 2.5);

                    int number_of_25 = (int)(remaining_distance / (25 * 2.5));
                    remaining_distance -= number_of_25 * (25 * 2.5);

                    int number_of_5 = (int)(remaining_distance / (5 * 2.5));
                    remaining_distance -= number_of_5 * (5 * 2.5);

                    int number_of_1 = (int)(remaining_distance / (1 * 2.5));
                    remaining_distance -= number_of_1 * (1 * 2.5);
                    if (distance < 2.5)
                    {
                        number_of_1 = 1;
                    }

                    //MyVisualScriptLogicProvider.SendChatMessage("Distance: " + distance);
                    //MyVisualScriptLogicProvider.SendChatMessage("Number of 312.5: " + number_of_125);
                    //MyVisualScriptLogicProvider.SendChatMessage("Number of 62.5: " + number_of_25);
                    //MyVisualScriptLogicProvider.SendChatMessage("Number of 12.5: " + number_of_5);
                    //MyVisualScriptLogicProvider.SendChatMessage("Number of 2.5: " + number_of_1);

                    var up_of_first = Vector3D.Normalize(vector_between);
                    var forward_of_first = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                    var position_of_first = cargoPos + (up_of_first * 1.25) + (up_of_first * remaining_distance);
                    var ent_of_first = PrimeEntityActivator(allModels[3]);
                    ent_of_first.WorldMatrix = MatrixD.CreateWorld(position_of_first, forward_of_first, up_of_first);

                    var newChunk = new VisualChunk(ent_of_first, 2.5);
                    allChunks.Add(newChunk);

                    var current_offset = 2.5;
                    for (int i = 1; i < number_of_1; i++)
                    {
                        MyEntity ent = PrimeEntityActivator(allModels[3]);
                        var position_of_chunk = allChunks[i - 1].chunk.WorldMatrix.Translation + up_of_first * 2.5;
                        var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                        ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                        var innerChunk = new VisualChunk(ent, 2.5);
                        allChunks.Add(innerChunk);
                        current_offset += (1 * 2.5);
                    }

                    for (int i = 0; i < number_of_5; i++)
                    {
                        if (i == 0)
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[2]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (5 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 12.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[2]);
                            var position_of_chunk = allChunks[allChunks.Count - 1].chunk.WorldMatrix.Translation + up_of_first * (5 * 2.5);
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 12.5);
                            allChunks.Add(innerChunk);
                        }
                        current_offset += (5 * 2.5);
                        //MyVisualScriptLogicProvider.SendChatMessage(current_offset);
                    }

                    for (int i = 0; i < number_of_25; i++)
                    {
                        if (i == 0)
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[1]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (25 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 62.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[1]);
                            var position_of_chunk = allChunks[allChunks.Count - 1].chunk.WorldMatrix.Translation + up_of_first * (25 * 2.5);
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 62.5);
                            allChunks.Add(innerChunk);
                        }
                        current_offset += (25 * 2.5);
                    }

                    for (int i = 0; i < number_of_125; i++)
                    {
                        if (i == 0)
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[0]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (125 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 312.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(allModels[0]);
                            var position_of_chunk = allChunks[allChunks.Count - 1].chunk.WorldMatrix.Translation + up_of_first * (125 * 2.5);
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 312.5);
                            allChunks.Add(innerChunk);
                        }
                        current_offset += (125 * 2.5);
                    }

                    //int number_of_chunks = (int)(vector_between.Length() / 2.5);
                    //var remainder = vector_between.Length() - (2.5 * number_of_chunks);
                    //var forward_of_first = Vector3D.Normalize(vector_between);
                    //var position_of_first = cargo_block.WorldMatrix.Translation + (forward_of_first * remainder);
                    //var up_of_first = MyUtils.GetRandomPerpendicularVector(ref forward_of_first);
                    //var ent_of_first = PrimeEntityActivator();
                    //ent_of_first.WorldMatrix = MatrixD.CreateWorld(position_of_first, forward_of_first, up_of_first);

                    //var newChunk = new VisualChunk(ent_of_first);
                    //allChunks.Add(newChunk);

                    //RigidBodyFlag rbf = RigidBodyFlag.RBF_STATIC;
                    //var test_chunk = allChunks[0];
                    //var end_chunk = allChunks[allChunks.Count - 1];
                    //MyPhysicsHelper.InitBoxPhysics(test_chunk.chunk, MyStringHash.GetOrCompute("Ammo"), new Vector3(0, 5, 0), new Vector3(5, 5, 0), 0f, 0f, 0f, 15, rbf);
                    //test_chunk.chunk.Physics.Enabled = true;
                    //test_chunk.chunk.Physics.Activate();
                    // foreach (var ent in allChunks)
                    // {
                    //     MyPhysicsHelper.InitModelPhysics(ent.chunk, RigidBodyFlag.RBF_STATIC, 15);
                    //     ent.chunk.Physics.Enabled = true;
                    //     ent.chunk.Physics.Activate();
                    // }
                    //////newChunk.chunk.Physics.Enabled = true;
                    //////newChunk.chunk.GetPhysicsBody().Activate();
                    ////MyPhysicsHelper.InitBoxPhysics(newChunk.chunk, MyStringHash.GetOrCompute("Ammo"), new Vector3(5, 0, 0), new Vector3(1, 1, 1), 100, 0f, 0f, 9, RigidBodyFlag.RBF_DEFAULT);
                    ////newChunk.chunk.Physics.Enabled = true;
                    ////newChunk.chunk.Physics.Activate();

                    //for (int i = 1; i < number_of_chunks; i++)
                    //{
                    //    MyEntity ent = PrimeEntityActivator();
                    //    var position_of_chunk = allChunks[i - 1].chunk.WorldMatrix.Translation + forward_of_first * 2.5;
                    //    var up_of_chunk = MyUtils.GetRandomPerpendicularVector(ref forward_of_first);
                    //    ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_first, up_of_chunk);
                    //    var innerChunk = new VisualChunk(ent);

                    //    allChunks.Add(innerChunk);
                    //}
                }
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("", e.Message);
            }
        }
        private void CleanupChain()
        {
            foreach (var ent in allChunks)
            {
                if (ent.chunk != null && !ent.chunk.MarkedForClose)
                {
                    ent.chunk.Close();
                }
            }

            allChunks.Clear();
        }

        private MyEntity PrimeEntityActivator(string path)
        {
            var ent = new MyEntity();
            ent.Init(null, path, null, null, null);
            ent.Render.CastShadows = true; //Maybe true?
            ent.IsPreview = true;
            ent.Save = false;
            ent.SyncFlag = false;
            ent.NeedsWorldMatrix = false;
            ent.Flags |= EntityFlags.IsNotGamePrunningStructureObject;
            MyEntities.Add(ent, true);
            return ent;
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                frame++;
                if (frame <= 0) frame = 0;

                if (MyAPIGateway.Session.IsServer)
                {
                    if (PipelineSession.Instance.readyToConnect)
                    {
                        NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
                    }

                    if ( ((frame + frameOffset) % updateInventoryPeriodFrames ) == 0)
                    {
                        //Inventory logic
                        if (server_block_state == BlockState.Connected)
                        {
                            if (cargo_inventory == null || other_cargo_inventory == null)
                            {
                                cargo_inventory = cargo_block.GetInventory() as MyInventory;
                                other_cargo_inventory = other_cargo_block.GetInventory() as MyInventory;
                            }

                            if (cargo_inventory != null && other_cargo_inventory != null && !cargo_inventory.Empty())
                            {
                                for (int i = 0; i < cargo_inventory.ItemCount; i++)
                                {
                                    other_cargo_inventory.TransferItemFrom(cargo_inventory, i);
                                }
                            }
                        }

                        //Networking
                        all_players.Clear();
                        MyAPIGateway.Multiplayer.Players.GetPlayers(all_players);
                        Packet packet = new Packet();
                        packet.incoming_block_state = server_block_state;
                        packet.incoming_cargo_block_id = cargo_block.EntityId;

                        if (server_block_state == BlockState.Connected && other_cargo_block != null)
                        {
                            packet.incoming_othercargo_id = other_cargo_block.EntityId;
                        }
                        var binary_packet = MyAPIGateway.Utilities.SerializeToBinary<Packet>(packet);
                        foreach (var p in all_players)
                        {
                            MyAPIGateway.Multiplayer.SendMessageTo(netId, binary_packet, p.SteamUserId);
                        }

                        if (MyAPIGateway.Utilities.IsDedicated)
                        {
                            MyAPIGateway.Multiplayer.SendMessageTo(netId, binary_packet, MyAPIGateway.Multiplayer.MyId);
                        }

                    }

                }

                if (!MyAPIGateway.Utilities.IsDedicated)
                {
                    if (frame % 120 == 0)
                    {
                        cone_mat = cargo_block.WorldMatrix;

                        if (client_block_state == BlockState.Connected)
                        {
                            if (cargo_block != null && !cargo_block.MarkedForClose && cargo_block.CubeGrid.Physics != null)
                            {
                                foreach (var ent in allChunks)
                                {
                                    ent.chunk.Render.EnableColorMaskHsv = true;
                                    ent.chunk.Render.TextureChanges = cargo_block.Render.TextureChanges;
                                    ent.chunk.Render.MetalnessColorable = cargo_block.Render.MetalnessColorable;
                                    ent.chunk.Render.ColorMaskHsv = cargo_block.Render.ColorMaskHsv;
                                }
                            }
                        }
                        else
                        {
                            CleanupChain();
                        }
                    }

                    if (client_block_state == BlockState.Idle || client_block_state == BlockState.Searching)
                    {
                        if (PipelineSession.Instance.draw_cone)
                        {
                            MySimpleObjectDraw.DrawLine(cargo_block.WorldMatrix.Translation, cargo_block.WorldMatrix.Translation + cargo_block.WorldMatrix.Forward * 10, pipeline_mat,
                                ref for_col, 0.1f, BlendTypeEnum.PostPP);
                            MySimpleObjectDraw.DrawTransparentCone(ref cone_mat, 405.4f, 2000, ref search_col, 8, pipeline_mat); //for 0.2 radians at 2km
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                MyLog.Default.WriteLine("KLIME PIPELINE: " + e);
            }
        }

        public override void UpdateAfterSimulation100()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                if (server_block_state == BlockState.Idle)
                {
                    if (isOk(cargo_block))
                    {
                        server_block_state = BlockState.Searching;
                    }
                }

                if (server_block_state == BlockState.Searching)
                {
                    if (isOk(cargo_block))
                    {
                        searchCooldownFrames -= 100;
                        if (searchCooldownFrames <= 0 )
                        {
                            bool established_connection = DoSearch();
                            if (established_connection)
                            {
                                server_block_state = BlockState.Connected;
                                failedSearches = 0;
                            }
                            else
                            {
                                failedSearches++;
                                searchCooldownFrames = Math.Min(failedSearches * searchCooldownFailMultiple, searchCooldownMax);
                            }
                        }
                    }
                    else
                    {
                        server_block_state = BlockState.Idle;
                    }
                }

                if (server_block_state == BlockState.Connected)
                {
                    if (!ValidateOtherCargo(other_cargo_block))
                    {
                        server_block_state = BlockState.Idle;
                    }
                }
            }
        }

        private bool DoSearch()
        {
            bool connected_ok = false;
            search_ents.Clear();
            search_onlycargo.Clear();

            BoundingSphereD sphere = new BoundingSphereD(cargo_block.WorldMatrix.Translation, search_radius);
            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, search_ents);

            foreach (var ent in search_ents)
            {
                if (ent is IMyCargoContainer)
                {
                    IMyCargoContainer test_cargo = ent as IMyCargoContainer;
                    if (test_cargo != null && test_cargo.BlockDefinition.SubtypeName == "Pipeline_Cargo" && !test_cargo.IsSameConstructAs(cargo_block) && test_cargo.CubeGrid.IsStatic)
                    {
                        if (AngleCheck(ref cargo_block, ref test_cargo))
                        {
                            search_onlycargo.Add(test_cargo);
                        }
                    }
                }
            }

            //Vector3D startLine = cargo_block.WorldMatrix.Translation;
            //Vector3D endLine = startLine + (cargo_block.WorldMatrix.Forward * search_radius);
            //double currentClosest = 0;

            //for (int i = 0; i < search_onlycargo.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        Vector3D checkPos = search_onlycargo[i].WorldMatrix.Translation;
            //        Vector3D closePoint = MyUtils.GetClosestPointOnLine(ref startLine, ref endLine, ref checkPos);
            //        currentClosest = (closePoint - startLine).LengthSquared();

            //        if (cargo_block.CustomName.Contains("@"))
            //        {
            //            MyVisualScriptLogicProvider.AddGPSForAll(currentClosest.ToString() + " INDEX: " + i, "", closePoint, Color.Red);
            //        }

            //        if (ValidateOtherCargo(search_onlycargo[i]))
            //        {
            //            other_cargo_block = search_onlycargo[i];
            //            connected_ok = true;
            //        }
            //    }
            //    else
            //    {
            //        Vector3D checkPos = search_onlycargo[i].WorldMatrix.Translation;
            //        Vector3D closePoint = MyUtils.GetClosestPointOnLine(ref startLine, ref endLine, ref checkPos);
            //        double testClosestDistance = (closePoint - startLine).LengthSquared();

            //        if (cargo_block.CustomName.Contains("@"))
            //        {
            //            MyVisualScriptLogicProvider.AddGPSForAll(testClosestDistance.ToString() + " INDEX: " + i, "", closePoint, Color.Red);
            //        }

            //        if (testClosestDistance < currentClosest)
            //        {
            //            if (ValidateOtherCargo(search_onlycargo[i]))
            //            {
            //                other_cargo_block = search_onlycargo[i];
            //                currentClosest = testClosestDistance;
            //                connected_ok = true;
            //            }
            //        }
            //    }
            //}

            search_onlycargo = search_onlycargo.OrderBy(o => Vector3D.Distance(cargo_block.WorldMatrix.Translation, o.WorldMatrix.Translation)).ToList();

            foreach (var cargo in search_onlycargo)
            {
                if (ValidateOtherCargo(cargo))
                {
                    other_cargo_block = cargo;
                    connected_ok = true;
                    break;
                }
            }

            return connected_ok;
        }

        private bool AngleCheck(ref IMyCargoContainer mainCargo, ref IMyCargoContainer bCargo)
        {
            bool withinAngle = false;
            var vector_between = Vector3D.Normalize(bCargo.WorldMatrix.Translation - mainCargo.WorldMatrix.Translation);
            var angle = MyUtils.GetAngleBetweenVectorsAndNormalise(mainCargo.WorldMatrix.Forward, vector_between);

            if (angle <= search_angle_tolerence)
            {
                withinAngle = true;
            }

            return withinAngle;
        }

        private bool ValidateOtherCargo(IMyCargoContainer test_cargo)
        {
            bool is_valid = false;
            if (isOk(test_cargo) && isOk(cargo_block))
            {
                if (AngleCheck(ref cargo_block, ref test_cargo))
                {
                    is_valid = true;
                    all_hits.Clear();
                    MyAPIGateway.Physics.CastRay(cargo_block.WorldMatrix.Translation, test_cargo.WorldMatrix.Translation, all_hits);
                    foreach (var hit in all_hits)
                    {
                        if (hit.HitEntity != null && hit.HitEntity is MyVoxelBase)
                        {
                            is_valid = false;
                        }
                    }
                }
            }
            return is_valid;
        }

        public bool isOk(IMyCargoContainer cargo_to_check)
        {
            bool ok_to_proceed = false;
            if (cargo_to_check != null && cargo_to_check.IsFunctional && cargo_to_check.IsWorking && cargo_to_check.CubeGrid.Physics != null && cargo_to_check.CubeGrid.IsStatic
                && !cargo_to_check.MarkedForClose && !cargo_to_check.CubeGrid.MarkedForClose && cargo_block.ShowInTerminal)
            {
                ok_to_proceed = true;
            }

            if (!ok_to_proceed && other_cargo_block != null && cargo_to_check.EntityId == other_cargo_block.EntityId)
            {
                other_cargo_block = null;
                other_cargo_inventory = null;
            }
            return ok_to_proceed;
        }

        public float AngleBetweenVectors(Vector3D v1, Vector3D v2, Vector3D up)
        {
            var cross = Vector3D.Cross(v1, v2);
            var dot = Vector3D.Dot(v1, v2);

            var angle = Math.Atan2(cross.Length(), dot);

            var test = Vector3D.Dot(up, cross);
            if (test > 0.0) angle = -angle;
            return (float)angle;
        }

        public override void Close()
        {
            try
            {
                if (allChunks != null)
                {
                    foreach (var ent in allChunks)
                    {
                        ent.chunk.Close();
                    }
                    allChunks.Clear();
                }
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(netId, pipeline_net_handler);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("KLIME PIPELINE: " + e);
            }
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PipelineSession : MySessionComponentBase
    {
        public static PipelineSession Instance;
        public bool draw_cone = false;
        public int masterTimer = 0;
        public bool readyToConnect = false;
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            }
        }

        public override void LoadData()
        {
            Instance = this;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session.IsServer)
            {
                if (masterTimer == 300)
                {
                    readyToConnect = true;
                }
            }
            masterTimer += 1;
        }

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText == "/pipeline toggle")
            {
                draw_cone = !draw_cone;

                sendToOthers = false;
            }
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;
            }
            Instance = null;
        }
    }
}