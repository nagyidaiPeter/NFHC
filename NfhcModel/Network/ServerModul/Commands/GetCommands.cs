using NfhcModel.Network.ServerModul.ServerProcessors;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace NfhcModel.Network.ServerModul.Commands
{
    public class GetCommands : ChatCommand
    {
        public override string Command => "\\cmd";

        public override void Execute(string[] args, ServerChatProcessor processor)
        {
            //Display all available commands

            //Collect all commands
            var _commands = new Dictionary<string, ChatCommand>(StringComparer.OrdinalIgnoreCase);

            // Automatically register all commands
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ChatCommand)) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var command = (ChatCommand)Activator.CreateInstance(type);
                _commands.Add(command.Command, command);
            }

            //Display all commands
            processor.SendChatMessage("Available commands: " + string.Join(", ", _commands.Keys));
        }
    }
}
