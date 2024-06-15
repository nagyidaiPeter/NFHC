
using NFH.DevTools;
using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Core;
using NfhcModel.Logger;
using NfhcModel.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NfhcModel.MonoBehaviours
{

    public class LogicTesting : MonoBehaviour
    {
        private ActorBrain woody, neighbour;

        GameObject woodyInstance;

        bool triggered = false;

        private PlayerManager _playerManager;
        internal PlayerManager GetPlayerManager
        {
            get
            {
                _playerManager = _playerManager ?? NfhcServiceLocator.LocateService<PlayerManager>();
                return _playerManager;
            }
        }

        void Start()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                Log.Info($"Scene loaded instance: {LogicController.HasInstance}");
                triggered = false;
                if (LogicController.HasInstance)
                {
                    LogicController.Instance.OnLevelWasInitialized += (level) =>
                    {
                        Log.Info($"Set lives to 1");
                        LogicController.Instance.TheGameScore.LivesCount = 1;
                        LogicController.Instance.TheGameScore.UnPauseNotifications(true);
                    };
                }
            };
        }

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
            if (Input.GetKeyDown(KeyCode.Z))
            {
                string sceneName = SceneManager.GetActiveScene().name;
                string filePath = Application.dataPath + "/" + sceneName + "_Hierarchy.xml";

                using (XmlWriter writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Scene");
                    writer.WriteAttributeString("name", sceneName);

                    foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
                    {
                        WriteGameObjectHierarchy(writer, rootGameObject);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                Debug.Log("Hierarchy saved to " + filePath);
            }


            if (Input.GetKeyDown(KeyCode.J))
            {
                CollectAndWriteEntityActionHandlers();
            }

            foreach (var player in GetPlayerManager.Players.Values)
            {
                if (player.Woody != null && !triggered)
                {
                    if (player.Woody.GetComponent<ActorBrain>().CurrentRoom == neighbour.CurrentRoom)
                    {
                        SetupDieCommands(player.Woody.GetComponent<ActorBrain>(), neighbour);
                        triggered = true;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ActorBrain originalWoody = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsWoody);

                woodyInstance = Instantiate(originalWoody.gameObject, originalWoody.transform.parent);//new GameObject("woody");
                woodyInstance.transform.parent = originalWoody.transform.parent;
                woodyInstance.name = "Woody-Clone";

                var oldActionHandler = originalWoody.GetComponent<EntityActionHandler>();

                var actor = woodyInstance.GetComponent<Actor>();
                var actorBrain = woodyInstance.GetComponent<ActorBrain>();
                var actionHandler = woodyInstance.GetComponent<EntityActionHandler>();
                var entity = woodyInstance.GetComponent<GameEntity>();

                var brainProp = actor.GetType().GetField("_brain", BindingFlags.NonPublic | BindingFlags.Instance);
                brainProp.SetValue(actor, actorBrain);

                entity.WorldObjectName = woodyInstance.name;

                Log.Info($"Old ID: {originalWoody.gameObject.GetInstanceID()} newID: {actorBrain.gameObject.GetInstanceID()}");

                var prop = actionHandler.GetType().GetField("_entity", BindingFlags.NonPublic | BindingFlags.Instance);
                prop.SetValue(actionHandler, entity);

                foreach (var action in oldActionHandler.Actions)
                {
                    Log.Info($"Action: {action?.ActionName} Actor: {action?.ActorName} ActorBrain: {action?.ActorBrain?.name} BehavActor: {action.BehaviorActorBrain}");
                }

                actionHandler.Actions.Clear();
                Log.Info($"Actions on new woody: {actionHandler.Actions.Count} Actions on old woody: {oldActionHandler.Actions.Count}");

                Log.Info($"----------------***Adding actions***----------------");

                foreach (var action in oldActionHandler.Actions)
                {
                    EntityAction newAction = new EntityAction(action.ActionName);

                    action.CopyFieldsTo(newAction);

                    Log.Info($"Action: {newAction?.ActionName} Actor: {newAction?.ActorName} ActorBrain: {newAction?.ActorBrain?.name} BehavActor: {newAction.BehaviorActorBrain}");

                    if (newAction.ActorName == "woody")
                    {
                        var nameProp = newAction.GetType().GetField("_actorName", BindingFlags.NonPublic | BindingFlags.Instance);
                        nameProp.SetValue(newAction, woodyInstance.name);
                    }

                    if (newAction.ActorBrain == originalWoody.gameObject.GetComponent<ActorBrain>())
                    {
                        newAction.ActorBrain = actorBrain;
                    }

                    var handlerProp = newAction.GetType().GetField("_handler", BindingFlags.NonPublic | BindingFlags.Instance);
                    handlerProp.SetValue(newAction, actionHandler);

                    actionHandler.Actions.Add(newAction);
                }
                Log.Info($"----------------***Done***----------------");

                actor.SetCurrentRoom(originalWoody.CurrentRoom);

                LevelRoom room = originalWoody.CurrentRoom;
                if (woodyInstance.gameObject.GetComponent<GameEntity>() is GameEntity gameEntity && room != null)
                {
                    gameEntity.SetCurrentRoom(room);
                    woodyInstance.gameObject.transform.localPosition = new Vector3(originalWoody.transform.localPosition.x, originalWoody.transform.localPosition.y, 0);

                    if (LogicController.Instance.TheTriggerHandler is TriggerHandler triggerHandler)
                    {
                        LogicController.Instance.InitializeLevel(LogicController.Instance.CurrentLevel);
                        triggerHandler.gameObject.SetActive(false);
                        triggerHandler.gameObject.SetActive(true);
                    }
                    else
                    {
                        Log.Info($"Failed to get trigger handler..");
                    }

                    LogicController.Instance.AddEntityToWorld(gameEntity, room);

                    LogicController.Instance.GetAllPortals(true);
                    LogicController.Instance.GetAllItems(true);
                    LogicController.Instance.GetAllContainers(true);
                    LogicController.Instance.GetAllActors(true);
                    LogicController.Instance.GetAllActorBrains(true);
                }
                else
                {
                    Log.Info($"Failed to get Room: {room == null} or Entity: {woodyInstance.GetComponent<GameEntity>() == null}");
                }
                Log.Info($"Successfully added new test woody!");
            }
        }

      private void CollectAndWriteEntityActionHandlers()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("EntityActionHandler Report:");
        sb.AppendLine();

        // Find all EntityActionHandler components in the scene
        EntityActionHandler[] handlers = FindObjectsOfType<EntityActionHandler>();

        foreach (EntityActionHandler handler in handlers)
        {
            GameObject obj = handler.gameObject;
            sb.AppendLine($"GameObject: {obj.name}");
            sb.AppendLine($"EntityActionHandler for {obj.name}:");
            sb.AppendLine($"Number of Actions: {handler.Count}");
            sb.AppendLine();

            foreach (EntityAction action in handler.Actions)
            {
                sb.AppendLine($"  Action Name: {action.ActionName}");
                sb.AppendLine($"    Noise Level: {action.NoiseLevel}");
                sb.AppendLine($"    Duration: {action.Duration}");
                sb.AppendLine($"    Show Progress: {action.ShowProgress}");
                sb.AppendLine($"    Object Animation Name: {action.ObjectAnimName}");
                sb.AppendLine($"    Object Next Animation Name: {action.ObjectNextAnimName}");
                sb.AppendLine($"    Actor Name: {action.ActorName}");
                sb.AppendLine($"    Actor Animation Name: {action.ActorAnimName}");
                sb.AppendLine($"    Actor Next Animation Name: {action.ActorNextAnimName}");
                sb.AppendLine($"    Behavior Name: {action.BehaviorName}");
                sb.AppendLine($"    Behavior Actor Name: {action.BehaviorActorName}");
                sb.AppendLine($"    Trigger Always: {action.TriggerAlways}");
                sb.AppendLine($"    Number of Tricks: {action.Tricks.Count}");
                sb.AppendLine($"    Number of Translations: {action.Translations?.Count ?? 0}");
                sb.AppendLine();
            }

            // If jobs are accessible, include their details as well using reflection
            ActorBrain actorBrain = handler.GetComponent<ActorBrain>();
            if (actorBrain != null)
            {
                sb.AppendLine($"ActorBrain for {obj.name}:");

                // Use reflection to get the private jobStack field
                FieldInfo jobStackField = typeof(ActorBrain).GetField("jobStack", BindingFlags.NonPublic | BindingFlags.Instance);
                if (jobStackField != null)
                {
                    object jobStackValue = jobStackField.GetValue(actorBrain);
                    if (jobStackValue != null)
                    {
                        Type jobStackType = jobStackValue.GetType();
                        PropertyInfo countProperty = jobStackType.GetProperty("Count");
                        if (countProperty != null)
                        {
                            int jobCount = (int)countProperty.GetValue(jobStackValue);
                            sb.AppendLine($"Number of Jobs: {jobCount}");
                            sb.AppendLine();

                            MethodInfo toArrayMethod = jobStackType.GetMethod("ToArray");
                            if (toArrayMethod != null)
                            {
                                Array jobArray = (Array)toArrayMethod.Invoke(jobStackValue, null);
                                foreach (var job in jobArray)
                                {
                                    Type jobType = job.GetType();
                                    PropertyInfo nameProperty = jobType.GetProperty("Name");
                                    PropertyInfo isStoppableProperty = jobType.GetProperty("IsStoppable");
                                    MethodInfo getStateInfoMethod = jobType.GetMethod("GetStateInfo");

                                    string jobName = nameProperty != null ? (string)nameProperty.GetValue(job) : "Unknown";
                                    bool isStoppable = isStoppableProperty != null && (bool)isStoppableProperty.GetValue(job);
                                    string stateInfo = getStateInfoMethod != null ? (string)getStateInfoMethod.Invoke(job, new object[] { actorBrain }) : "No State Info";

                                    sb.AppendLine($"  Job Name: {jobName}");
                                    sb.AppendLine($"    Is Stoppable: {isStoppable}");
                                    sb.AppendLine($"    Details: {stateInfo}");
                                    sb.AppendLine();
                                }
                            }
                        }
                    }
                }
                sb.AppendLine();
            }
        }

        // Define the path for the output file
        string path = Path.Combine(Application.dataPath, "EntityActionHandlerReport.txt");

        // Write the collected information to the file
        File.WriteAllText(path, sb.ToString());

        Debug.Log($"EntityActionHandler report written to {path}");
    }

        void WriteGameObjectHierarchy(XmlWriter writer, GameObject gameObject)
        {
            writer.WriteStartElement("GameObject");
            writer.WriteAttributeString("name", gameObject.name);

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                writer.WriteStartElement("Component");
                writer.WriteAttributeString("type", component.GetType().Name);
                writer.WriteEndElement();
            }

            foreach (Transform child in gameObject.transform)
            {
                WriteGameObjectHierarchy(writer, child.gameObject);
            }

            writer.WriteEndElement();
        }

        private void SetupDieCommands(ActorBrain woody, ActorBrain neighbour)
        {
            FightCommand cmd = FightCommand.Create("fight", woody.GetComponent<Actor>(), TriggerFlag.InSameRoom);
            neighbour.HandleTriggerCommand(cmd);

            DieCommand dieCmd = DieCommand.Create("die", neighbour.GetComponent<Actor>(), TriggerFlag.InSameRoom);
            woody.HandleTriggerCommand(dieCmd);

            woody.GetComponent<EntityActionHandler>().SetRuntimeReferencesForActions();
            neighbour.GetComponent<EntityActionHandler>().SetRuntimeReferencesForActions();

            Log.Info("Fight-Die commands issued!");
        }
    }
}
