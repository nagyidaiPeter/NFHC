using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NfhcModel.MonoBehaviours.Gui
{


    public class ChatHandler : MonoBehaviour
    {

        private List<MessageObject> Messages = new List<MessageObject>();

        private int MaxMessages = 100;

        public GameObject chatPanel, text;

        public bool WritingChatActive = false;

        public bool ReceivedChatActive = false;

        public float TimeToActive = 8;

        public float currentInactiveTime = 0;

        public bool EnableChat = false;

        public void Activate()
        {
            currentInactiveTime = 0;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        public void StopWritingChat()
        {
            WritingChatActive = false;
        }


        public void AddMessage(string msg, int sender)
        {
            var newMsg = new MessageObject();
            newMsg.Text = msg;
            newMsg.SenderID = sender;

            var newTextObj = Instantiate(text, chatPanel.transform);
            newTextObj.SetActive(true);
            newMsg.TextObject = newTextObj.GetComponent<TMP_Text>();
            newMsg.TextObject.text = newMsg.Text;

            newMsg.TextObject.color = sender < 0 ? Color.red : Color.white;

            Messages.Add(newMsg);

            while (Messages.Count > MaxMessages)
            {
                Destroy(Messages[0].TextObject.gameObject);
                Messages.RemoveAt(0);
            }

            Activate();
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            currentInactiveTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.T) && !WritingChatActive && EnableChat)
            {
                currentInactiveTime = 0;
                WritingChatActive = true;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);

                    var chatInput = GameObject.Find("chatInput").GetComponent<TMP_InputField>();
                    chatInput.ActivateInputField();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) && WritingChatActive)
            {
                WritingChatActive = false;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }

            if (currentInactiveTime > TimeToActive)
            {
                WritingChatActive = false;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }

        }
    }

    [System.Serializable]
    public class MessageObject
    {
        public string Text;
        public TMP_Text TextObject;
        public int SenderID;
    }

}
