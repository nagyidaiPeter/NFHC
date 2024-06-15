using NFH.Game.Logic;
using NfhcModel.Network.ServerModul.ServerProcessors;
using UnityEngine;

namespace NfhcModel.Network.ServerModul.Commands
{
    public class GetState : ChatCommand
    {
        public override string Command => "\\getstate";

        public override void Execute(string[] args, ServerChatProcessor processor)
        {
            if (args.Length < 1)
            {
                processor.SendChatMessage("Usage: \\getstate EntityName");
                return;
            }

            var entityName = args[0];

            GameObject entity = TryGetActor(entityName, processor);

            if (entity == null)
            {
                return;
            }

            var actor = entity.GetComponent<ActorBrain>();

            if (actor == null)
            {
                processor.SendChatMessage($"Entity '{entityName}' has no brain.");
                return;
            }

            var brainScript = actor.GetBrainScript();
            if (brainScript == null)
            {
                processor.SendChatMessage($"Entity '{entityName}' has no brain script.");
                return;
            }

            var state = brainScript.GetState();
            if (state == null)
            {
                processor.SendChatMessage($"Entity '{entityName}' has no state.");
                return;
            }

            processor.SendChatMessage($"State of entity {brainScript.GetStateInfo()}");
        }
    }
}
