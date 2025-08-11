using ProtoBuf;
using System.Collections.Generic;
using VRageMath;
using VRage.Serialization;

namespace Klime.Pipeline
{

    [ProtoInclude(1000, typeof(PipelineSyncPacket))]
    [ProtoInclude(1001, typeof(SyncRequestPacket))]
    [ProtoContract]
    public abstract class Packet
    {

        public Packet() { }
    }

    [ProtoContract]
    public class PipelineSyncPacket : Packet
    {
        [ProtoMember(1)]
        public BlockState incoming_block_state;

        [ProtoMember(2)]
        public long incoming_cargo_block_id;

        [ProtoMember(3)]
        public long incoming_othercargo_id;

        public PipelineSyncPacket()
        {

        }

        public PipelineSyncPacket(BlockState incoming_block_state, long incoming_cargo_block_id, long incoming_othercargo_id)
        {
            this.incoming_block_state = incoming_block_state;
            this.incoming_cargo_block_id = incoming_cargo_block_id;
            this.incoming_othercargo_id = incoming_othercargo_id;
        }
    }

    [ProtoContract]
    public class SyncRequestPacket : Packet
    {
        // DJ if you're reading this ik theres a mild amount of position exploitation here, I do not care
        [ProtoMember(1)] public Vector3D PlayerPos;
        public SyncRequestPacket() { }

        public SyncRequestPacket(Vector3D pos)
        {
            PlayerPos = pos;
        }
    }
}
