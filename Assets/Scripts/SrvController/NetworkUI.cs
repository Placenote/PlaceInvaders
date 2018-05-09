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
		public InputField RoomName;


        // Use this for initialization
        void Start()
        {
   
            if (Srv == null)
                Srv = FindObjectOfType<SrvController>();

            
            ConnectionBtn.AddOnClickListener(DoConnect);
        }
			
        void DoConnect()
        {
            if (string.IsNullOrEmpty(PlayerName.text))
            {
                float pseudoUID = Time.realtimeSinceStartup;
                pseudoUID = pseudoUID - Mathf.FloorToInt(pseudoUID)*1000;

                PlayerName.text = "Player" + Mathf.FloorToInt(pseudoUID);
            }
			if (string.IsNullOrEmpty (RoomName.text)) 
			{
				RoomName.text = "Untitled";
			}
			Srv.Connect(PlayerName.text, RoomName.text);
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
