using System;
using NfhcModel.Logger;
using NfhcModel.MonoBehaviours.Gui.MainMenu;
using NfhcModel.MonoBehaviours.Logic;
using UnityEngine;

namespace NfhcModel.MonoBehaviours
{
    public class NfhcBootstrapper : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += HandleLog;

#if DEBUG
            EnableDeveloperFeatures();
#endif

            gameObject.AddComponent<LogicTesting>();
            gameObject.AddComponent<MainMenuMods>();
            gameObject.AddComponent<SceneSyncer>();
            gameObject.AddComponent<Multiplayer>();
            gameObject.AddComponent<BrainInjector>();

        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            Log.Info($"GAME: {type} {logString} {stackTrace}");
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        }

    }
}
