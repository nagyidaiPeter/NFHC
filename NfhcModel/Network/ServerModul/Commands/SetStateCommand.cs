using NfhcModel.Network.ServerModul.ServerProcessors;

namespace NfhcModel.Network.ServerModul.Commands
{
    public class SetStateCommand : ChatCommand
    {
        public override string Command => "\\setstate";

        public override void Execute(string[] args, ServerChatProcessor processor)
        {
            if (args.Length < 2)
            {
                processor.SendChatMessage("Usage: \\setstate EntityName State");
                return;
            }

            var entityName = args[0];
            var stateName = args[1];
            var entity = TryGetActor(entityName, processor);
            if (entity == null)
            {
                processor.SendChatMessage($"Entity '{entityName}' not found.");
                return;
            }

            if (SetState(entity, stateName))
            {                
                processor.SendChatMessage($"State of entity '{entityName}' set to '{stateName}'.");
            }
            else
            {
                processor.SendChatMessage($"Failed to set state '{stateName}' for entity '{entityName}'.");
            }
        }
    }
}
