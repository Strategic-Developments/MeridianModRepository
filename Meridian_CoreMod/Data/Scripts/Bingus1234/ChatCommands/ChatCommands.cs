using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRage;

namespace ChatCommandsN
{

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class ChatCommands : MySessionComponentBase
    {
        public static string ModName = "MeridianCoreMod";

        public static MyTuple<ulong, string[]> lastCommandSent = new MyTuple<ulong, string[]>();

        private static Dictionary<string, MyTuple<Action<ulong, string[]>, string>> _chatCommands = new Dictionary<string, MyTuple<Action<ulong, string[]>, string>>();

        public static void OnChatMessageRecieved(ulong sender, string messageText, ref bool sendToOthers)
        {
            string[] messageTextSplit = messageText.Split(' ');

            MyTuple<Action<ulong, string[]>, string> Command;

            if (_chatCommands.TryGetValue(messageTextSplit[0].ToLowerInvariant(), out Command))
            {
                messageTextSplit[0] = null;

                sendToOthers = false;
                string[] copy = new string[messageTextSplit.Length];
                messageTextSplit.CopyTo(copy, 0);
                lastCommandSent = new MyTuple<ulong, string[]>(sender, copy);
                
                Command.Item1.Invoke(sender, messageTextSplit);
            }
        }

        

        public static void AddChatCommand(string CommandText, Action<ulong, string[]> Command, string description = null)
        {
            if (!_chatCommands.ContainsKey(CommandText.ToLowerInvariant()))
                _chatCommands.Add(CommandText.ToLowerInvariant(), new MyTuple<Action<ulong, string[]>, string>(Command, description));
            else
            {
                MyLog.Default.Warning("Chat command already exists.");
            }
        }
        public static void ChatCommand_GetAllCommands(ulong SenderId, string[] message)
        {
            foreach (KeyValuePair<string, MyTuple<Action<ulong, string[]>, string>> Command in _chatCommands)
            {
                if (!(Command.Key == "/showallcommands"))
                {
                    ShowMessage(Command.Key.ToString());
                    if (Command.Value.Item2 != null)
                        MyAPIGateway.Utilities.ShowMessage("", Command.Value.Item2);
                }
            }

            return;
        }
        public override void BeforeStart()
        {
            MyAPIUtilities.Static.MessageEnteredSender += OnChatMessageRecieved;
            AddChatCommand("/ShowAllCommands", ChatCommand_GetAllCommands);
        }

        protected override void UnloadData()
        {
            MyAPIUtilities.Static.MessageEnteredSender -= OnChatMessageRecieved;
            _chatCommands.Clear();
        }

        public static void ShowMessage(string message)
        {
            MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }


        public static bool IsOwner(ulong PlayerId)
        {
            return MyAPIGateway.Session.GetUserPromoteLevel(PlayerId) >= MyPromoteLevel.Owner;
        }
        public static bool IsAdmin(ulong PlayerId)
        {
            return MyAPIGateway.Session.GetUserPromoteLevel(PlayerId) >= MyPromoteLevel.Admin;
        }

        public static bool IsSpaceMaster(ulong PlayerId)
        {
            return MyAPIGateway.Session.GetUserPromoteLevel(PlayerId) >= MyPromoteLevel.SpaceMaster;
        }

        public static bool IsModerator(ulong PlayerId)
        {
            return MyAPIGateway.Session.GetUserPromoteLevel(PlayerId) >= MyPromoteLevel.Moderator;
        }

        public static bool IsOnlyPlayer(ulong PlayerId)
        {
            return MyAPIGateway.Session.GetUserPromoteLevel(PlayerId) == MyPromoteLevel.None;
        }
    }
}
