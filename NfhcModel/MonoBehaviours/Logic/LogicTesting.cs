
using NFH.DevTools;
using NFH.Game;
using NFH.Game.Logic;
using NfhcModel.Core;
using NfhcModel.Logger;
using NfhcModel.Network;
using System;
using System.Linq;
using System.Reflection;
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
                neighbour = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor);

                if (neighbour != null)
                {
                    Log.Info("Neighbour change task!");

                    //neighbour.HandleTriggerCommand();

                    var brainScript = neighbour.GetBrainScript();
                    var brainState = brainScript.GetState();
                    var stateInfo = brainScript.GetStateInfo();


                    Log.Info($"BEFORE State: {brainState.Method.Name} StateInfo: {stateInfo}");
                    MethodInfo dynMethod = brainScript.GetType().GetMethod("GoToBeerState", BindingFlags.NonPublic | BindingFlags.Instance);

                    BrainScriptBase.State method = Delegate.CreateDelegate(typeof(BrainScriptBase.State), brainScript, dynMethod) as BrainScriptBase.State;
                    Log.Info($"Created delegate..");
                    brainScript.EnterIdleState();
                    neighbour.StopAllJobs();
                    brainScript.Stop(neighbour);
                    brainScript.SetState(method);
                    brainScript.Run(neighbour);

                    Log.Info($"State: {brainState.ToString()} - {brainState.Method.Name} StateInfo: {stateInfo}");
                }
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                neighbour = GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsNeighbor);

                if (neighbour != null)
                {
                    Log.Info("Neighbour change task!");

                    //neighbour.HandleTriggerCommand();

                    var brainScript = neighbour.GetBrainScript();
                    var brainState = brainScript.GetState();
                    var stateInfo = brainScript.GetStateInfo();

                    Log.Info($"BEFORE State: {brainState.Method.Name} StateInfo: {stateInfo}");
                    MethodInfo dynMethod = brainScript.GetType().GetMethod("GoToSofaState", BindingFlags.NonPublic | BindingFlags.Instance);

                    BrainScriptBase.State method = Delegate.CreateDelegate(typeof(BrainScriptBase.State), brainScript, dynMethod) as BrainScriptBase.State;
                    Log.Info($"Created delegate..");
                    brainScript.EnterIdleState();
                    neighbour.StopAllJobs();
                    brainScript.Stop(neighbour);
                    brainScript.SetState(method);
                    brainScript.Run(neighbour);

                    Log.Info($"State: {brainState.ToString()} - {brainState.Method.Name} StateInfo: {stateInfo}");
                }
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
