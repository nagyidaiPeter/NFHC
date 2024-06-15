using UnityEngine;
using NfhcModel.TestBrains;
using NFH.Game.Logic;
using System.Linq;

namespace NfhcModel.MonoBehaviours.Logic
{


    public class BrainInjector : MonoBehaviour
    {

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                InjectCustomBrain();
            }
        }

        public void InjectCustomBrain()
        {
            // Create a new instance of the custom brain script
            var customBrain = ScriptableObject.CreateInstance<NeighborBrain>();

            if (customBrain == null)
            {
                Debug.LogError("Failed to create custom brain script.");
                return;
            }

            // Find the neighbor object (assuming it has a specific tag or name)
            ActorBrain neighbor = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor);

            if (neighbor != null)
            {
                // Get the ActorBrain component
                var actorBrain = neighbor.GetComponent<ActorBrain>();

                if (actorBrain != null)
                {
                    // Unload the current brain script
                    actorBrain.UnloadBrainScript(true);
                    actorBrain.StopAllJobs();
                    //actorBrain.AddAndRunJob(customBrain);
                    actorBrain.AddJob(customBrain);
                    actorBrain.StartBrainScript();
                    

                    Debug.Log("Custom brain injected successfully!");
                }
                else
                {
                    Debug.LogError("Neighbor does not have an ActorBrain component.");
                }
            }
            else
            {
                Debug.LogError("Neighbor object not found.");
            }
        }
    }

}
