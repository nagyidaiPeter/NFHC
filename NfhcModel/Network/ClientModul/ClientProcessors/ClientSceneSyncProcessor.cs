using LiteNetLib;
using LiteNetLib.Utils;
using NFH.Game;
using NFH.Game.Localization;
using NFH.Game.UI;
using NfhcModel.Logger;
using NfhcModel.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NfhcModel.Network.ClientModul.ClientProcessors
{
    public class ClientSceneSyncProcessor : BaseProcessor
    {
        public new Queue<SceneLoadingSync> IncomingMessages { get; set; } = new Queue<SceneLoadingSync>();

        public new Queue<SceneLoadingSync> OutgoingMessages { get; set; } = new Queue<SceneLoadingSync>();

        public ClientSceneSyncProcessor()
        {

        }

        public override bool AddMessage(byte[] message, DataStructures.PlayerData player)
        {
            var SceneLoadingSync = BaseMessageType.Deserialize<SceneLoadingSync>(message);
            IncomingMessages.Enqueue(SceneLoadingSync);
            return true;
        }

        public override bool SendMessage(object message, DataStructures.PlayerData player)
        {
            if (message is SceneLoadingSync dataMessage)
            {
                OutgoingMessages.Enqueue(dataMessage);
                return true;
            }
            return false;
        }

        public override void Process()
        {
            while (IncomingMessages.Any())
            {
                SceneLoadingSync sceneLoadingSync = IncomingMessages.Dequeue();

                if (!GetClient.IsServer)
                {
                    //SceneManager.LoadScene(sceneLoadingSync.SceneName, (LoadSceneMode)sceneLoadingSync.LoadingMode);

                    if (SceneController.HasInstance)
                    {
                        string text = SceneController.FindLevelScenePath(sceneLoadingSync.SceneName);
                        
                        //SceneController.Instance.ReloadCurrentLevelScene();

                        //DescriptionRecord levelDescription = null;
                        //if (UserDataController.HasInstance)
                        //{
                        //    var levelSequence = UserDataController.Instance.LevelSetSequence;
                        //    levelDescription = LanguageResolverBase.LocalizeLevelDescription(levelSequence.GetDescriptionByName(sceneLoadingSync.SceneName), sceneLoadingSync.SceneName, true);
                        //}
                        if (GameUI.HasInstance)
                        {
                            if (sceneLoadingSync.SceneName == "Level__Master")
                            {
                                GameUI.Instance.ShowLevelSelectScreen("");
                            }
                            else
                            {
                                GameUI.Instance.ShowLevelSelectScreen(sceneLoadingSync.SceneName);
                                SceneController.Instance.LoadLevelScene(text);
                                GameUI.Instance.HideLevelSelectScreen();                                
                            }
                        }
                    }
                }
            }

            while (OutgoingMessages.Any())
            {
                SceneLoadingSync SceneLoadingSync = OutgoingMessages.Dequeue();

            }
        }
    }
}
