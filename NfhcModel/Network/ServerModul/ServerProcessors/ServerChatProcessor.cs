using LiteNetLib.Utils;
using LiteNetLib;
using NFH.Game.Logic;
using NfhcModel.Network.Messages;
using NfhcModel.Network.ServerModul.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace NfhcModel.Network.ServerModul.ServerProcessors
{
    public class ServerChatProcessor : BaseProcessor
    {
        public new Queue<ChatMessage> IncomingMessages { get; set; } = new Queue<ChatMessage>();
        public new Queue<ChatMessage> OutgoingMessages { get; set; } = new Queue<ChatMessage>();

        private readonly Dictionary<string, ChatCommand> _commands;

        public override MessageTypes MessageType { get { return MessageTypes.Chat; } }

        public ServerChatProcessor()
        {
            _commands = new Dictionary<string, ChatCommand>(StringComparer.OrdinalIgnoreCase);

            // Automatically register all commands
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ChatCommand)) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var command = (ChatCommand)Activator.CreateInstance(type);
                _commands.Add(command.Command, command);
            }
        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var chatMessage = BaseMessageType.Deserialize<ChatMessage>(message);
            chatMessage.PlayerName = player.Name;
            chatMessage.MessageText = chatMessage.MessageText;
            IncomingMessages.Enqueue(chatMessage);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is ChatMessage dataMessage)
            {
                OutgoingMessages.Enqueue(dataMessage);
                return true;
            }
            return false;
        }

        public override void Process()
        {
            while (IncomingMessages.Any())
            {
                ChatMessage chatMessage = IncomingMessages.Dequeue();

                //Message starts with player name, so to check for a command that must be removed

                Debug.Log($"Received chat message from {chatMessage.PlayerName}: {chatMessage.MessageText}");
                if (chatMessage.MessageText.StartsWith("\\"))
                {
                    ProcessCommand(chatMessage);
                }
                else
                {
                    var msg = BaseMessageType.Serialize(chatMessage);
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(msg);
                    GetServer.server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
                }
            }

            while (OutgoingMessages.Any())
            {
                ChatMessage chatMessage = OutgoingMessages.Dequeue();
                var msg = BaseMessageType.Serialize(chatMessage);
                NetDataWriter writer = new NetDataWriter();
                writer.Put(msg);
                GetServer.server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            }
        }

        private void ProcessCommand(ChatMessage chatMessage)
        {
            var parts = chatMessage.MessageText.Split(' ');
            if (parts.Length == 0)
            {
                SendChatMessage("Invalid command format.");
                return;
            }

            var commandName = parts[0];
            var args = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    command.Execute(args, this);
                } catch (Exception ex)
                {
                    SendChatMessage($"Error executing command '{commandName}': {ex.Message}");
                }
            }
            else
            {
                SendChatMessage("Unknown command.");
            }
        }

        public void SendChatMessage(string message)
        {
            var chatMessage = new ChatMessage(message);
            OutgoingMessages.Enqueue(chatMessage);
        }
    }
}
