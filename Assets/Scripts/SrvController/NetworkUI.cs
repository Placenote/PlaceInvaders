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
		public ConnectionButton HostBtn;
        public ShortTextMessage TextMsg;
        public InputField PlayerName;
		public InputField RoomName;

        // Use this for initialization
        void Start()
        {
   
            if (Srv == null)
                Srv = FindObjectOfType<SrvController>();

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
