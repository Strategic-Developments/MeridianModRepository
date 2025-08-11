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

        public IMyCargoContainer cargo_block;
        public IMyCargoContainer other_cargo_block;
        public MyInventory cargo_inventory;
        public MyInventory other_cargo_inventory;
        
        public BlockState server_block_state = BlockState.Idle;
        public BlockState client_block_state = BlockState.Idle;
        
        int frame = 0;
        int frameOffset = 0;
        const int updateInventoryPeriodFrames = 600; // 10 seconds
        int failedSearches = 0;
        int searchCooldownFrames = 0;
        const int searchCooldownFailMultiple = 200;
        const int searchCooldownMax = 1200; // 20 seconds

        const double search_radius = 3000;
        const double search_angle_tolerence = 0.2; //in radians, not degrees
        readonly List<MyEntity> search_ents = new List<MyEntity>();
        List<IMyCargoContainer> search_onlycargo = new List<IMyCargoContainer>();
        MyStringId pipeline_mat;
        Color search_col = Color.LightGreen;
        Vector4 for_col = Color.LightGreen;
        readonly List<VisualChunk> allChunks = new List<VisualChunk>();

        
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

                    if (MyAPIGateway.Session.IsServer)
                    {
                        frameOffset = MyUtils.GetRandomInt(0, 59);
                    }

                    NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                    PipelineSession.Instance.Pipelines.Add(cargo_block.EntityId, this);

                    if (!MyAPIGateway.Multiplayer.IsServer)
                    {
                        Network.SendMessageToServer(new SyncRequestPacket(cargo_block.EntityId), Network.MessageHandlerId);
                    }

                    //MyVisualScriptLogicProvider.AddGPSForAll("", "", cargo_block.WorldMatrix.Translation, Color.Orange);
                }
            }
        }


        public void ProcessPacket(PipelineSyncPacket incoming_packet)
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

        private void NewChain()
        {
            try
            {
                if (cargo_block != null && other_cargo_block != null)
                {
                    var cargoPos = cargo_block.WorldMatrix.Translation + cargo_block.WorldMatrix.Forward * 0f;
                    var otherPos = other_cargo_block.WorldMatrix.Translation + other_cargo_block.WorldMatrix.Backward * 0f;
                    var vector_between = otherPos - cargoPos;
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
                    var ent_of_first = PrimeEntityActivator(PipelineSession.Instance.allModels[3]);
                    ent_of_first.WorldMatrix = MatrixD.CreateWorld(position_of_first, forward_of_first, up_of_first);

                    var newChunk = new VisualChunk(ent_of_first, 2.5);
                    allChunks.Add(newChunk);

                    var current_offset = 2.5;
                    for (int i = 1; i < number_of_1; i++)
                    {
                        MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[3]);
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
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[2]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (5 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 12.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[2]);
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
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[1]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (25 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 62.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[1]);
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
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[0]);
                            var position_of_chunk = allChunks[0].chunk.WorldMatrix.Translation - (allChunks[0].chunkLength * 0.5 * up_of_first) + (up_of_first * current_offset) + (up_of_first * (125 * 1.25));
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 312.5);
                            allChunks.Add(innerChunk);
                        }
                        else
                        {
                            MyEntity ent = PrimeEntityActivator(PipelineSession.Instance.allModels[0]);
                            var position_of_chunk = allChunks[allChunks.Count - 1].chunk.WorldMatrix.Translation + up_of_first * (125 * 2.5);
                            var forward_of_chunk = MyUtils.GetRandomPerpendicularVector(ref up_of_first);
                            ent.WorldMatrix = MatrixD.CreateWorld(position_of_chunk, forward_of_chunk, up_of_first);
                            var innerChunk = new VisualChunk(ent, 312.5);
                            allChunks.Add(innerChunk);
                        }
                        current_offset += (125 * 2.5);
                    }
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
                            MySimpleObjectDraw.DrawTransparentCone(ref cone_mat, 608.130106526f, 3000, ref search_col, 8, pipeline_mat); //for 0.2 radians at 2km
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
                                PipelineSession.Instance.PropagateStateChange(this);
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
                        PipelineSession.Instance.PropagateStateChange(this);
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
                    if (test_cargo != null && test_cargo.BlockDefinition.SubtypeName == "Pipeline_Cargo" && test_cargo.CubeGrid.IsStatic)
                    {
                        if (AngleCheck(ref cargo_block, ref test_cargo))
                        {
                            search_onlycargo.Add(test_cargo);
                        }
                    }
                }
            }
            
            search_onlycargo = search_onlycargo.OrderBy(o => Math.Floor(Vector3.Distance(cargo_block.WorldMatrix.Translation, o.WorldMatrix.Translation)/100)*100
            + MyUtils.GetAngleBetweenVectors(cargo_block.WorldMatrix.Forward, o.WorldMatrix.Forward)).ToList();

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

        public override void MarkForClose()
        {
            PipelineSession.Instance.Pipelines.Remove(cargo_block.EntityId);
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
                
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("KLIME PIPELINE: " + e);
            }
        }
    }

    
}