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
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class PipelineSession : MySessionComponentBase
    {
        public static PipelineSession Instance;

        public Dictionary<long, Pipeline> Pipelines;
        public List<PipelineSyncPacket> PacketsToLoad;

        public bool draw_cone = false;
        public int masterTimer = 0;
        public bool readyToConnect = false;

        public List<string> allModels = new List<string>();
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            }
            Pipelines = new Dictionary<long, Pipeline>();
            PacketsToLoad = new List<PipelineSyncPacket>();
            Network.Load();
            Network.OnMessageReceived += OnMessageReceived;

            string modpath = ModContext.ModPath;
            allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_125.mwm");
            allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_25.mwm");
            allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_5.mwm");
            allModels.Add(modpath + @"\Models\Cubes\large\Pipeline_1.mwm");
        }

        private void OnMessageReceived(ushort ChannelId, Packet packet, ulong SenderId, bool fromServer)
        {
            if (packet is PipelineSyncPacket)
            {
                var incoming_packet = packet as PipelineSyncPacket;

                Pipeline p, _;
                if (Pipelines.TryGetValue(incoming_packet.incoming_cargo_block_id, out p)
                    && (incoming_packet.incoming_block_state != BlockState.Connected || Pipelines.TryGetValue(incoming_packet.incoming_othercargo_id, out _)))
                {
                    MyLog.Default.WriteLineAndConsole("pipeline sync recieved and succeeded");
                    p.ProcessPacket(incoming_packet);
                }
                else
                {
                    MyLog.Default.WriteLineAndConsole("pipeline sync recieved and failed");
                    bool add = true;
                    for (int i = 0; i < PacketsToLoad.Count; i++)
                    {
                        if (PacketsToLoad[i].incoming_cargo_block_id == incoming_packet.incoming_cargo_block_id)
                        {
                            PacketsToLoad[i] = incoming_packet;
                            add = false;
                            break;
                        }
                    }
                    if (add)
                        PacketsToLoad.Add(incoming_packet);
                }
            }
            else if (packet is SyncRequestPacket)
            {
                var syncReq = packet as SyncRequestPacket;

                int sync = MyAPIGateway.Session.SessionSettings.SyncDistance * MyAPIGateway.Session.SessionSettings.SyncDistance;
                MyLog.Default.WriteLineAndConsole("pipeline sync request recieved");
                Pipeline pipe;
                if (Pipelines.TryGetValue(syncReq.BlockId, out pipe))
                {
                    PipelineSyncPacket packetToSend = new PipelineSyncPacket
                    {
                        incoming_block_state = pipe.server_block_state,
                        incoming_cargo_block_id = pipe.cargo_block.EntityId
                    };

                    if (pipe.server_block_state == BlockState.Connected && pipe.other_cargo_block != null)
                    {
                        packetToSend.incoming_othercargo_id = pipe.other_cargo_block.EntityId;
                    }
                    MyLog.Default.WriteLineAndConsole("pipeline sync request responded to");
                    Network.SendMessageTo(packetToSend, Network.MessageHandlerId, SenderId);
                }
            }
        }

        public void PropagateStateChange(Pipeline pipe)
        {
            PipelineSyncPacket packet = new PipelineSyncPacket
            {
                incoming_block_state = pipe.server_block_state,
                incoming_cargo_block_id = pipe.cargo_block.EntityId
            };

            if (pipe.server_block_state == BlockState.Connected && pipe.other_cargo_block != null)
            {
                packet.incoming_othercargo_id = pipe.other_cargo_block.EntityId;
            }
            MyLog.Default.WriteLineAndConsole("pipeline sync change broadcasted");
            Network.SendMessageToClientsInRange(packet, Network.MessageHandlerId, pipe.cargo_block.GetPosition(), 
                MyAPIGateway.Session.SessionSettings.SyncDistance * MyAPIGateway.Session.SessionSettings.SyncDistance);
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

            masterTimer++;

            if (!MyAPIGateway.Multiplayer.IsServer && masterTimer % 100 == 0 && PacketsToLoad.Count != 0)
            {

                for (int i = PacketsToLoad.Count - 1; i >= 0; i--)
                {
                    var packet = PacketsToLoad[i];
                    Pipeline p, _;
                    if (Pipelines.TryGetValue(packet.incoming_cargo_block_id, out p) 
                        && (packet.incoming_block_state != BlockState.Connected || Pipelines.TryGetValue(packet.incoming_othercargo_id, out _)))
                    {
                        MyLog.Default.WriteLineAndConsole("pipeline sync late success");
                        p.ProcessPacket(packet);
                        PacketsToLoad.RemoveAtFast(i);
                    }
                }
            }
        }

        private void Utilities_MessageEntered(string messageText, ref bool sendToOthers)
        {
            if (messageText.ToLowerInvariant() == "/pipeline toggle")
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
            Network.Unload();
            Instance = null;
            PacketsToLoad = null;
            Pipelines = null;
        }
    }
    public enum BlockState
    {
        Idle,
        Searching,
        Connected
    }
}
