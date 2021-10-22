using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NfhcModel.MonoBehaviours.Gui
{
    public class UiTimer : MonoBehaviour
    {
        public bool UiEnabled = true;


        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UiEnabled = !UiEnabled;
                gameObject.GetComponent<Canvas>().enabled = UiEnabled;
            }
        }
    }
}
