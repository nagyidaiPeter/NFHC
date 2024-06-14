using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NfhcModel.Core;
using NfhcModel.Logger;
using NfhcModel.Network;
using NfhcModel.Network.ClientModul;
using NfhcModel.Network.ServerModul;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NfhcModel.MonoBehaviours.Gui.MainMenu
{
    public class MainMenuMods : MonoBehaviour
    {
        public Server server { get; set; }

        public Client client { get; set; }

        public ChatHandler chatHandler;

        private Button join, host, disconnect;
        private TMP_InputField address, nickname;

        void Start()
        {

        }


        void Update()
        {

        }

        public void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            try
            {
                Log.Info("Creating new lifetime scope..");
                NfhcServiceLocator.BeginNewLifetimeScope();
                server = NfhcServiceLocator.LocateService<Server>();
                client = NfhcServiceLocator.LocateService<Client>();
            }
            catch (Exception ex)
            {
                Log.Info(ex);
            }

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            AssetBundle chatBundle = AssetBundle.LoadFromFile(Path.GetDirectoryName(path) + "/StreamingAssets/serverbrowser");

            string sceneName = chatBundle.GetAllScenePaths().First();
            Log.Debug($"Trying to load scene: {sceneName}");
            var sceneLoading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            sceneLoading.completed += SceneLoading_completed;
        }

        private void SceneLoading_completed(AsyncOperation obj)
        {            
            if (GameObject.Find("host") is GameObject hostButtonGO && hostButtonGO.GetComponent<Button>() is Button hostButton)
            {
                Debug.Log("Found host button!");
                
                DontDestroyOnLoad(hostButtonGO.transform.root);

                host = hostButton;
                hostButton.onClick.AddListener(delegate
                {
                    Log.Info($"Starting server..");
                    client.clientConfig.IP = "localhost";
                    client.clientConfig.Port = 29050;
                    client.IsServer = true;
                    StartCoroutine(RunServer());
                    StartCoroutine(RunClient());

                    GameObject.Find("Modal").GetComponent<RectTransform>().sizeDelta = new Vector2(200, 100);
                    GameObject.Find("Modal").GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-205, 223.7f, 0);
                    foreach (Transform child in GameObject.Find("ConnectionRoot").transform)
                    {
                        child.gameObject.SetActive(false);
                    }
                    GameObject.Find("ConnectionRoot").gameObject.SetActive(true);
                    foreach (Transform child in GameObject.Find("ConnectedRoot").transform)
                    {
                        child.gameObject.SetActive(true);
                    }
                });
            }
            else
            {
                Debug.Log("Failed to find host button!");
            }

            if (GameObject.Find("join") is GameObject joinButtonGO && joinButtonGO.GetComponent<Button>() is Button joinButton)
            {
                Debug.Log("Found join button!");
                join = joinButton;
                joinButton.onClick.AddListener(delegate
                {
                    Log.Info($"Joining server..");
                    StartCoroutine(RunClient());
                    GameObject.Find("ConnectedRoot")?.SetActive(true);
                });
            }
            else
            {
                Debug.Log("Failed to find join button!");
            }


            if (GameObject.Find("disconnect") is GameObject disconnectButtonGO && disconnectButtonGO.GetComponent<Button>() is Button dcButton)            {
                Debug.Log("Found disconnect button!");
                disconnect = dcButton;
                disconnect.gameObject.SetActive(false);
                dcButton.onClick.AddListener(delegate
                {
                    Log.Info($"Disconnecting from server..");
                    
                    foreach (Transform child in GameObject.Find("ConnectionRoot").transform)
                    {
                        child.gameObject.SetActive(true);
                    }
                    GameObject.Find("Modal").GetComponent<RectTransform>().sizeDelta = new Vector2(450, 220);
                    GameObject.Find("Modal").GetComponent<RectTransform>().anchoredPosition3D = new Vector3(-455, 223.7f, 0);
                    foreach (Transform child in GameObject.Find("ConnectedRoot").transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    if (client.IsClientRunning)
                    {
                        client.StopClient();
                    }

                    if (server.IsServerRunning)
                    {
                        server.StopServer();
                    }

                    client.IsServer = false;

                    StopAllCoroutines();
                });
            }
            else
            {
                Debug.Log("Failed to find disconnect button!");
            }

            if (GameObject.Find("address") is GameObject addressFieldGO && addressFieldGO.GetComponent<TMP_InputField>() is TMP_InputField addressField)
            {
                Debug.Log("Found address field!");
                address = addressField;
                addressField.onEndEdit.AddListener(delegate
                {
                    var splitted = addressField.text.Split(':');
                    client.clientConfig.IP = splitted[0];                    
                    Int32.TryParse(splitted[1], out client.clientConfig.Port);
                    Debug.Log($"New client conf: {client.clientConfig.IP}:{client.clientConfig.Port}");
                });
            }
            else
            {
                Debug.Log("Failed to find address field!");
            }

            if (GameObject.Find("username") is GameObject usernameFieldGO && usernameFieldGO.GetComponent<TMP_InputField>() is TMP_InputField usernameField)
            {
                Debug.Log("Found nickname field!");
                nickname = usernameField;
                nickname.onEndEdit.AddListener(delegate
                {
                    var playerManager = NfhcServiceLocator.LocateService<PlayerManager>();
                    playerManager.LocalPlayer.Name = nickname.text;
                });
            }
            else
            {
                Debug.Log("Failed to find nickname field!");
            }



            //Setup chat
            if (GameObject.Find("Chat") is GameObject chatGo)
            {
                GameObject messageObj = GameObject.Find("Message").gameObject;
                
                messageObj.SetActive(false);

                DontDestroyOnLoad(chatGo);
                DontDestroyOnLoad(messageObj);

                chatHandler = chatGo.AddComponent<ChatHandler>();

                chatHandler.chatPanel = chatGo.transform.Find(@"Modal/messageView/Viewport/Content").gameObject;

                chatHandler.text = messageObj;

                var chatInput = GameObject.Find("chatInput").GetComponent<TMP_InputField>();

                chatInput.onValueChanged.AddListener(text =>
                {
                    chatHandler.Activate();
                });

                chatInput.onEndEdit.AddListener(text =>
                {
                    client.SendChatMessage(text);
                    chatInput.text = "";
                    chatHandler.StopWritingChat();
                });

                client.chatHandler = chatHandler;

                chatHandler.currentInactiveTime = chatHandler.TimeToActive;
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void OnGUI()
        {

        }

        private IEnumerator RunServer()
        {
            StartCoroutine(server.RunServer());
            yield return null;
        }

        private IEnumerator RunClient()
        {
            StartCoroutine(client.RunClient());
            yield return null;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            Log.Debug($"Scene loaded: {scene.name}");
        }
    }
}
