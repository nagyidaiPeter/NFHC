using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.MonoBehaviours
{
    public class LogicTesting : MonoBehaviour
    {
        private ActorBrain woody, neighbour;


        private void GetAllChild(Transform parent)
        {
            foreach (Transform child in parent)
            {
                Log.Info($"Child: {child.name}");
                foreach (var comp in child.gameObject.GetComponents<Component>())
                {
                    Log.Info($"\t{child.name} Comp: {comp.GetType()}");
                }
                GetAllChild(child);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                woody = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsWoody);

                if (woody != null)
                {
                    Log.Info("Woody comps:");
                    foreach (var comp in woody.gameObject.GetComponents<Component>())
                    {
                        Log.Info($"\tComp: {comp.GetType()}");
                    }

                    Log.Info("Woody childs:");
                    GetAllChild(woody.transform);



                    neighbour = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor);

                    if (neighbour != null)
                    {
                        Log.Info("Neighbour comps:");
                        foreach (var comp in neighbour.gameObject.GetComponents<Component>())
                        {
                            Log.Info($"Comp: {comp.GetType()}");
                        }

                        Log.Info("Neighbour childs:");
                        GetAllChild(woody.transform);
                    }
                }
            }


            if (Input.GetKeyDown(KeyCode.J))
            {
                foreach (var handler in GameObject.FindObjectsOfType<EntityActionHandler>())
                {
                    Log.Info($"\n{handler.gameObject.name}:");
                    foreach (var action in handler.Actions)
                    {
                        Log.Info($"Action: {action.ActionName}");
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                foreach (var entity in GameObject.FindObjectsOfType<GameEntity>())
                {
                    Log.Info($"\nEnity: {entity.WorldObjectName}");

                    Log.Info($"Comps:");
                    foreach (var comp in entity.gameObject.GetComponents<Component>())
                    {
                        Log.Info($"\tComp: {comp.GetType()}");
                    }

                    Log.Info($"Childs:");
                    foreach (Transform child in entity.transform)
                    {
                        Log.Info($"\tChild: {child.name}");
                        Log.Info($"\t\tChild Comps:");
                        foreach (var comp in child.gameObject.GetComponents<Component>())
                        {
                            Log.Info($"\t\t\tComp: {comp.GetType()}");
                        }
                    }

                    Log.Info($"Inventory:");
                    if (entity.Inventory != null && !entity.Inventory.IsEmpty)
                    {
                        foreach (var item in entity.Inventory.ItemSlots)
                        {
                            Log.Info($"\tInv: {item._item.name}");
                        }
                    }
                }

            }
        }
    }
}
