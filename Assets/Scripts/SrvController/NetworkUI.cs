using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PunServerNs;
using GameUiNs;

namespace NetworkUiNs
{
    public class NetworkUI : MonoBehaviour
    {
        public SrvController Srv;
     
       
        public ConnectionButton ConnectionBtn;
        public ShortTextMessage TextMsg;
        public InputField PlayerName;


        // Use this for initialization
        void Start()
        {
   
            if (Srv == null)
                Srv = FindObjectOfType<SrvController>();

            
            ConnectionBtn.AddOnClickListener(DoConnect);
        }

       // public string dbgPlayerName;
        void DoConnect()
        {
            //  dbgPlayerName = PlayerName.text;
            if (string.IsNullOrEmpty(PlayerName.text))
            {
                float pseudoUID = Time.realtimeSinceStartup;
                pseudoUID = pseudoUID - Mathf.FloorToInt(pseudoUID)*1000;

                PlayerName.text = "Player" + Mathf.FloorToInt(pseudoUID);
            }
          //  dbgPlayerName = PlayerName.text;
          //  Debug.Log("player input -----'"+PlayerName.text+"'");
            Srv.Connect(PlayerName.text);
        }

        void OnConnectionStateChaged(NetGameStateId id, string val)
        {
            ConnectionBtn.SwitchState(id);
            TextMsg.ShowMsg(val);

        }

        bool doSubscribe = false;
        private void OnEnable()
        {
            doSubscribe = true;
        }

        private void OnDisable()
        {
            Srv.UnSubscribe(OnConnectionStateChaged);
        }



        // Update is called once per frame
        void Update()
        {
            if (doSubscribe)
            {
                doSubscribe = false;
                Srv.Subscribe(OnConnectionStateChaged);
            }
        }
    }
}
