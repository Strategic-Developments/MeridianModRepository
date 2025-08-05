using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox.Engine.Utils;
using VRage;

namespace ChatCommandsN
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ChatCommandsMain : MySessionComponentBase
    {
        public override void BeforeStart()
        {
            foreach (var mod in MyAPIGateway.Session.Mods)
            {
                if (mod.PublishedFileId == 3168614142) // GridEdit, the commands are from
                    return;
            }

            ChatCommands.AddChatCommand("/GetSubtypeID", ChatCommand_GetSubtypeID, "Gets the subtypeID of the block you're looking at.");
            ChatCommands.AddChatCommand("/GetTypeID", ChatCommand_GetTypeID, "Gets the typeID of the block you're looking at.");
            ChatCommands.AddChatCommand("/GetSkinID", ChatCommand_GetSkinID, "Gets the subtype ID of the skin of the block you're looking at.");
        }
        public static IMyCubeGrid GetRaycastedGrid(ref LineD rayLine)
        {
            List<IHitInfo> HitEntities = new List<IHitInfo>();
            MyAPIGateway.Physics.CastRay(rayLine.To, rayLine.From, HitEntities);

            IMyCubeGrid closestGrid = null;
            double closestFraction = double.MaxValue;
            foreach (var hitEntity in HitEntities)
            {
                if (hitEntity.HitEntity is IMyCubeGrid)
                {
                    IMyCubeGrid g = hitEntity.HitEntity as IMyCubeGrid;
                    double d = 0;
                    Vector3I pos = new Vector3I();
                    g.GetLineIntersectionExactGrid(ref rayLine, ref pos, ref d);

                    if (d < closestFraction)
                    {
                        closestFraction = d;
                        closestGrid = g;
                    }
                }
            }
            return closestGrid;
        }

        public static LineD GetRaycastLine()
        {
            if (MyAPIGateway.Session.CameraController is MySpectatorCameraController)
            {
                MatrixD matrix = MySpectator.Static.Orientation;

                return new LineD(MySpectator.Static.Position, MySpectator.Static.Position + matrix.Forward * 300);
            }
            Vector3D forward = MyAPIGateway.Session.Camera.WorldMatrix.Forward;
            Vector3D eyePosition = MyAPIGateway.Session.Player.Character.PositionComp.GetPosition() + MyAPIGateway.Session.Player.Character.WorldMatrix.Up * 1.8;
            return new LineD(eyePosition, eyePosition + forward * 300);
        }

        public static void ChatCommand_GetSubtypeID(ulong SenderId, string[] message)
        {
            LineD rayLine = GetRaycastLine();
            IMyCubeGrid closestGrid = GetRaycastedGrid(ref rayLine);



            if (closestGrid == null)
            {
                ChatCommands.ShowMessage("Error: No grid found");
                return;
            }
            double dist;
            IMySlimBlock block;
            closestGrid.GetLineIntersectionExactAll(ref rayLine, out dist, out block);

            if (block == null)
            {
                ChatCommands.ShowMessage("Error: No block found");
                return;
            }
            if (block.BlockDefinition.Id.SubtypeName == "")
            {
                ChatCommands.ShowMessage($"Block Subtype Name: {block.BlockDefinition.Id.TypeId.ToString().Remove(0, 16)}");
            }
            else
            {
                ChatCommands.ShowMessage($"Block Subtype Name: {block.BlockDefinition.Id.SubtypeName}");
            }

            return;
        }

        public static void ChatCommand_GetTypeID(ulong SenderId, string[] message)
        {
            LineD rayLine = GetRaycastLine();
            IMyCubeGrid closestGrid = GetRaycastedGrid(ref rayLine);

            double dist;
            IMySlimBlock block;
            closestGrid.GetLineIntersectionExactAll(ref rayLine, out dist, out block);

            if (block == null)
            {
                ChatCommands.ShowMessage("Error: No block found");
                return;
            }
            ChatCommands.ShowMessage($"Block Type Name: {block.BlockDefinition.Id.TypeId.ToString().Remove(0, 16)}");
        }

        public static void ChatCommand_GetSkinID(ulong SenderId, string[] message)
        {
            LineD rayLine = GetRaycastLine();
            IMyCubeGrid closestGrid = GetRaycastedGrid(ref rayLine);

            double dist;
            IMySlimBlock block;
            closestGrid.GetLineIntersectionExactAll(ref rayLine, out dist, out block);

            if (block == null)
            {
                ChatCommands.ShowMessage("Error: No block found");
                return;
            }

            ChatCommands.ShowMessage($"Block Skin Id: '{block.SkinSubtypeId}'");

            return;
        }
    }
}
