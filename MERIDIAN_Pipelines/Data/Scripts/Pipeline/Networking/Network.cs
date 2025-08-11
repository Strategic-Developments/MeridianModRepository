using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Multiplayer;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Klime.Pipeline
{
    public static class Network
    {
        public static readonly ushort MessageHandlerId = 19593;
        static Queue<MyTuple<Packet, ushort, ulong, bool>> Buffer;
        private static bool Loaded = false;
        public static ObjectPool<List<IMyPlayer>> PlayerPool;
        public static void Load()
        {
            if (!Loaded)
            {
                Buffer = new Queue<MyTuple<Packet, ushort, ulong, bool>>();

                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(MessageHandlerId, MessageRecieved);

                PlayerPool = new ObjectPool<List<IMyPlayer>>(
                    () => new List<IMyPlayer>(),
                    startSize: 10
                    );

                Loaded = true;
            }
        }

        public static void Unload()
        {
            if (Loaded)
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(MessageHandlerId, MessageRecieved);
                Buffer = null;

                PlayerPool = null;
            }
        }

        public static void SendNextMessage()
        {
            if (Buffer.Count > 0)
            {
                MyTuple<Packet, ushort, ulong, bool> packet = Buffer.Dequeue();

                SendMessageTo(packet.Item1, packet.Item2, packet.Item3, packet.Item4);
            }
        }

        public static void AddMessageToBuffer(Packet packet, ushort channel, ulong RecipientId, bool reliable = true)
        {
            Buffer.Enqueue(new MyTuple<Packet, ushort, ulong, bool>()
            {
                Item1 = packet,
                Item2 = channel,
                Item3 = RecipientId,
                Item4 = reliable
            });
        }

        

        public static void SendMessageTo(Packet packet, ushort channel, ulong RecipientId, bool reliable = true)
        {
            byte[] SerializedMessage = MyAPIGateway.Utilities.SerializeToBinary(packet);
            if (!MyAPIGateway.Multiplayer.SendMessageTo(channel, SerializedMessage, RecipientId, reliable))
            {
                MyLog.Default.Error($"Error: Packet was not sent. Packet: {packet}");
            }
        }

        public static void SendMessageToClients(Packet packet, ushort channel, bool reliable = true, params ulong[] ignoreList)
        {
            byte[] SerializedMessage = MyAPIGateway.Utilities.SerializeToBinary(packet);
            var Players = PlayerPool.Pop();
            MyAPIGateway.Players.GetPlayers(Players);
            foreach (IMyPlayer player in Players)
            {
                if (!ignoreList.Contains(player.SteamUserId) && !player.IsBot)
                {
                    if (!MyAPIGateway.Multiplayer.SendMessageTo(channel, SerializedMessage, player.SteamUserId, reliable))
                    {
                        MyLog.Default.Error($"Error: Packet was not sent. Packet: {packet}");
                    }
                }
            }
            Players.Clear();
            PlayerPool.Push(Players);
        }

        public static void SendMessageToClientsInRange(Packet packet, ushort channel, Vector3D sendPos, float radiusSq, bool reliable = true, params ulong[] ignoreList)
        {
            byte[] SerializedMessage = MyAPIGateway.Utilities.SerializeToBinary(packet);
            var Players = PlayerPool.Pop();
            MyAPIGateway.Players.GetPlayers(Players, (p) =>
            {
                if (p.Character == null)
                    return false;

                return Vector3D.DistanceSquared(p.Character.GetPosition(), sendPos) <= radiusSq;
            });
            foreach (IMyPlayer player in Players)
            {
                if (!ignoreList.Contains(player.SteamUserId) && !player.IsBot)
                {
                    if (!MyAPIGateway.Multiplayer.SendMessageTo(channel, SerializedMessage, player.SteamUserId, reliable))
                    {
                        MyLog.Default.Error($"Error: Packet was not sent. Packet: {packet}");
                    }
                }
            }
            Players.Clear();
            PlayerPool.Push(Players);
        }

        public static void BufferSendMessageToServer(Packet packet, ushort channel, bool reliable = true)
        {
            AddMessageToBuffer(packet, channel, MyAPIGateway.Multiplayer.ServerId, reliable);
        }

        public static void SendMessageToServer(Packet packet, ushort channel, bool reliable = true)
        {
            byte[] SerializedMessage = MyAPIGateway.Utilities.SerializeToBinary(packet);
            if (!MyAPIGateway.Multiplayer.SendMessageToServer(channel, SerializedMessage, reliable))
            {
                MyLog.Default.Error($"Error: Packet was not sent. Packet: {packet}");
            }
        }

        public static void MessageRecieved(ushort ChannelId, byte[] bytes, ulong SenderId, bool fromServer)
        {
            Packet packet = null;
            try
            {
                packet = MyAPIGateway.Utilities.SerializeFromBinary<Packet>(bytes);
            }
            catch { }

            if (packet == null)
            {
                MyLog.Default.WriteLine($"[NETWORK ERROR] Recieved message failed to deserialize. HandlerId: {MessageHandlerId}.Sent from: {SenderId}.");
                return;
            }

            OnMessageReceived?.Invoke(ChannelId, packet, SenderId, fromServer);
        }

        public static event Action<ushort, Packet, ulong, bool> OnMessageReceived;
    }
}
