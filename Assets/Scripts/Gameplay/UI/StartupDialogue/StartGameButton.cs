using ARKitSupportNs;
using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{

    public class StartGameButton : MonoBehaviour
    {
        public Button button;
        // Use this for initialization
        void Start()
        {
            if (button == null)
                button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

        }
        void OnButtonClicked()
        {
            GameController.StartGame();
        }


    }
}
