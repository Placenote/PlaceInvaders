using PunServerNs;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NetworkUiNs
{


    [System.Serializable]
    public class ConnectionStateColors
    {
        public Color
        Disconnected = Color.grey,
        Connecting = Color.white,
        Connected = Color.green,
        Failed = Color.red;
    }

    public class ConnectionButton : MonoBehaviour
    {
        public GameObject ConnectionIndicator;
        public Text StateText;
        public Button ButtonObject;
        public NetGameStateId StateId;
        public ConnectionStateColors StateColors;


        void ShowFinalState(NetGameStateId stateId, Color colorId)
        {

            ColorBlock colorsBlock = ButtonObject.colors;
            ConnectionIndicator.SetActive(false);
            StateText.text = stateId.ToString();
            StateText.gameObject.SetActive(true);

            StateText.color = colorsBlock.normalColor = colorId;
            ButtonObject.colors = colorsBlock;
            
        }


        public void AddOnClickListener(Action onClickListener)
        {
            ButtonObject.onClick.AddListener(new UnityAction(onClickListener));
        }

        public void SwitchState(NetGameStateId newId)
        {
          

            switch (newId)
            {
                case NetGameStateId.Disconnected:
                    ShowFinalState(newId, StateColors.Disconnected);
                    break;

                case NetGameStateId.Connecting:
                    {
                        ConnectionIndicator.SetActive(true);
                        StateText.text = newId.ToString();
                        StateText.gameObject.SetActive(false);

                        ColorBlock colorsBlock = ButtonObject.colors;
                        StateText.color = colorsBlock.normalColor = colorsBlock.normalColor = StateColors.Connecting;
                        ButtonObject.colors = colorsBlock;
                    }
                    break;

                case NetGameStateId.Connected:
                    ShowFinalState(newId, StateColors.Connected);


                    break;

                case NetGameStateId.Failed:
                    ShowFinalState(newId, StateColors.Failed);
                    break;

            }
        }


        // Use this for initialization
        void Start()
        {
            SwitchState(NetGameStateId.Disconnected);
        }

        public bool dbgSwitch;
        public NetGameStateId dbgNewState;
        // Update is called once per frame
        void Update()
        {
            if (dbgSwitch)
            {
                dbgSwitch = false;
                SwitchState(dbgNewState);
            }
        }
    }
}
