using NFH.Game.Logic;
using NfhcModel.Core;
using NfhcModel.Logger;
using NfhcModel.Network;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.ClientModul.ClientProcessors;
using NfhcModel.Network.Messages;
using NfhcModel.Network.ServerModul;
using NfhcModel.Network.ServerModul.ServerProcessors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NfhcModel.MonoBehaviours
{
    public class SceneSyncer : MonoBehaviour
    {
        public Server server { get; set; }
        public Client client { get; set; }

        private ServerSceneSyncProcessor sceneSyncProcessor;

        private PlayerManager playerManager;


        void Awake()
        {
            try
            {
                server = NfhcServiceLocator.LocateService<Server>();
                client = NfhcServiceLocator.LocateService<Client>();
                sceneSyncProcessor = NfhcServiceLocator.LocateService<ServerSceneSyncProcessor>();
                playerManager = NfhcServiceLocator.LocateService<PlayerManager>();

            }
            catch (Exception ex)
            {
                Log.Info(ex);
            }

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (client.IsServer)
            {
                Log.Info($"{scene.name} loaded with {loadMode} mode. Sending to clients!");
                SceneLoadingSync sceneLoadingSync = new SceneLoadingSync();
                sceneLoadingSync.SenderID = -1;
                sceneLoadingSync.SceneName = scene.name;
                sceneLoadingSync.LoadingMode = (int)loadMode;
                sceneSyncProcessor.SendMessage(sceneLoadingSync, null);
            }

            StartCoroutine(FindWoody());
        }


        private IEnumerator FindWoody()
        {
            while (playerManager.LocalPlayer.Woody == null)
            {
                if (GameObject.FindObjectsOfType<ActorBrain>().FirstOrDefault(x => x.IsWoody) is ActorBrain woody)
                {
                    Log.Info($"Success finding woody..");
                    playerManager.LocalPlayer.Woody = woody.gameObject;
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }
}
