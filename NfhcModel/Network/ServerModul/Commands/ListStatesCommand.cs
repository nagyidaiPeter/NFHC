using NfhcModel.Network.ServerModul.ServerProcessors;
using System.Linq;

namespace NfhcModel.Network.ServerModul.Commands
{
    public class ListStatesCommand : ChatCommand
    {
        public override string Command => "\\states";

        public override void Execute(string[] args, ServerChatProcessor processor)
        {
            if (args.Length < 1)
            {
                processor.SendChatMessage("Usage: \\states EntityName or \\states EntityID");
                return;
            }

            var entityName = args[0];
            var entity = TryGetActor(entityName, processor);
            if (entity == null)
            {
                processor.SendChatMessage($"Entity '{entityName}' not found.");
                return;
            }

            var states = GetAvailableStates(entity);

            //Add ids for states by sorting them by name and then index in the list is the ID
            for (int i = 0; i < states.Count; i++)
            {
                states[i] += $" ({i})";
            }

            processor.SendChatMessage($"Available states for '{entity.gameObject.name}': " + string.Join(", ", states));
        }
    }
}
