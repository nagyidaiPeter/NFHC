using NFH.Game.Logic;
using NfhcModel.Network.ServerModul.ServerProcessors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NfhcModel.Network.ServerModul.Commands
{
    public abstract class ChatCommand
    {
        public abstract string Command { get; }
        public abstract void Execute(string[] args, ServerChatProcessor processor);

        protected GameObject FindEntityByName(string entityName)
        {
            var entities = GameObject.FindObjectsOfType<ActorBrain>();
            return entities.FirstOrDefault(e => e.name == entityName)?.gameObject;
        }

        protected GameObject FindEntityByID(int id)
        {
            var entities = GameObject.FindObjectsOfType<ActorBrain>();

            entities = entities.OrderBy(e => e.gameObject.name).ToArray();

            // ID is sort by name and then index in the list is the ID
            if (id < 0 || id >= entities.Length)
                return null;

            return entities[id].gameObject;
        }

        protected List<string> GetAllEntities()
        {
            var entities = new List<string>();

            //Add all entities to the list and attach id next to them, id is index in the list of entities sorted by name

            var allEntities = GameObject.FindObjectsOfType<ActorBrain>();
            allEntities = allEntities.OrderBy(e => e.gameObject.name).ToArray();

            foreach (var entity in allEntities)
            {
                entities.Add(entity.gameObject.name + " (" + Array.IndexOf(allEntities, entity) + ")");
            }

            return entities;
        }

        protected bool SetState(GameObject entity, string stateName)
        {
            var actorBrain = entity.GetComponent<ActorBrain>();
            if (actorBrain == null)
                return false;

            var brainScript = actorBrain.GetBrainScript();

            if (brainScript == null)
            {
                UnityEngine.Debug.Log("Brain script is null");
                return false;
            }

            UnityEngine.Debug.Log("Brain script found");

            //If stateName is id, get state by id
            if (int.TryParse(stateName, out var id))
            {
                var states = GetAvailableStates(entity);
                if (id < 0 || id >= states.Count)
                {
                    UnityEngine.Debug.Log("State id out of range");
                    return false;
                }

                stateName = states[id];
            }


            var method = brainScript.GetType().GetMethod(stateName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                UnityEngine.Debug.Log("State could not be found");
                return false;
            }

            UnityEngine.Debug.Log("State found");

            var state = Delegate.CreateDelegate(typeof(BrainScriptBase.State), brainScript, method) as BrainScriptBase.State;

            UnityEngine.Debug.Log("State created");
            actorBrain.StopAllJobs();
            brainScript.SetState(state);
            actorBrain.AddAndRunJob(brainScript);

            UnityEngine.Debug.Log($"{entity.gameObject.name} state set to {stateName}");
            return true;
        }

        protected List<string> GetAvailableStates(GameObject entity)
        {
            var brainScript = entity.GetComponent<ActorBrain>()?.GetBrainScript();
            if (brainScript == null)
                return new List<string>();

            var states = brainScript.GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(ActorBrain))
                .Select(m => m.Name)
                .ToList();

            states = states.OrderBy(s => s).ToList();

            return states;
        }

        protected GameObject TryGetActor(string entityIdentifier, ServerChatProcessor processor)
        {
            GameObject entity = null;
            //check if entityName is id or name
            if (int.TryParse(entityIdentifier, out var id))
            {
                entity = FindEntityByID(id);
                if (entity != null)
                {
                    return entity;
                }
                processor.SendChatMessage($"Entity with ID '{id}' not found.");
            }

            entity = FindEntityByName(entityIdentifier);
            if (entity == null)
            {
                processor.SendChatMessage($"Entity '{entityIdentifier}' not found.");
            }

            return entity;
        }
    }
}
