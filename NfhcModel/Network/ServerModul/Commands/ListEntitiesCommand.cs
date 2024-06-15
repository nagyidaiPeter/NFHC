using NfhcModel.Network.ServerModul.ServerProcessors;

namespace NfhcModel.Network.ServerModul.Commands
{
    public class ListEntitiesCommand : ChatCommand
    {
        public override string Command => "\\actors";

        public override void Execute(string[] args, ServerChatProcessor processor)
        {
            var entities = GetAllEntities();
            processor.SendChatMessage("Available entities: " + string.Join(", ", entities));
        }
    }
}
