using ProtoBuf;
using ResourceNodes;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using VRage;
using static Math0424.Networking.EasyNetworker;

namespace Math0424.Networking
{

    [ProtoContract]
    class DrillStateUpdate : IPacket
    {

        [ProtoMember(1)] public long blockId;
        [ProtoMember(2)] public bool isInGround;
        [ProtoMember(3)] public bool isProducing;
        [ProtoMember(4)] public bool invFull;
        [ProtoMember(5)] public string oreName;
        [ProtoMember(6)] public List<MyTuple<string, int>> oreList;

        public int GetId()
        {
            return 2;
        }

        public void Execute()
        {
            var e = MyEntities.GetEntityById(blockId);
            if (e != null && e is MyCubeBlock)
            {
                var b = e as MyCubeBlock;
                if (!b.MarkedForClose)
                {
                    AdvancedStaticDrill d = b.GameLogic.GetAs<AdvancedStaticDrill>();
                    MediumStaticDrill d1 = b.GameLogic.GetAs<MediumStaticDrill>();
                    BasicStaticDrill d2 = b.GameLogic.GetAs<BasicStaticDrill>();
                    if (d != null)
                    {
                        d.IsProducing = isProducing;
                        d.state = this;
                        d.optionsClient = oreList;
                    }
                    else if (d1 != null)
                    {
                        d1.IsProducing = isProducing;
                        d1.state = this;
                        d.optionsClient = oreList;
                    }
                    else if (d2 != null)
                    {
                        d2.IsProducing = isProducing;
                        d2.state = this;
                        d2.optionsClient = oreList;
                    }
                }
            }
        }

    }

    [ProtoContract]
    class TerminalUpdate : IPacket
    {

        [ProtoMember(1)] public long blockId;
        [ProtoMember(2)] public string oreName;


        public int GetId()
        {
            return 3;
        }

        public void Execute()
        {
            var e = MyEntities.GetEntityById(blockId);
            if (e != null && e is MyCubeBlock)
            {
                var b = e as MyCubeBlock;
                if (!b.MarkedForClose)
                {
                    AdvancedStaticDrill d = b.GameLogic.GetAs<AdvancedStaticDrill>();
                    MediumStaticDrill d1 = b.GameLogic.GetAs<MediumStaticDrill>();
                    BasicStaticDrill d2 = b.GameLogic.GetAs<BasicStaticDrill>();
                    if (d != null)
                    {
                        d.SetOreFromString(oreName);
                    }
                    else if (d1 != null)
                    {
                        d1.SetOreFromString(oreName);
                    }
                    else if (d2 != null)
                    {
                        d2.SetOreFromString(oreName);
                    }
                }
            }
        }

    }
}
