using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class ShortTextMessage : MonoBehaviour
    {
        [Tooltip("Normally it is empty string")]
        public string DefaultText = "No new messages";
        public bool IsAutoClear = false;
        public float AutoClearTimeOut;

        float curTimeOut;
        float curTimer;


        bool DoResetMsg = false;
        bool isMsgShown;

        public Text text;
        // Use this for initialization
        void Start()
        {
            if (text == null)
                text = transform.GetComponentInChildren<Text>();
            text.text = DefaultText;
            dbgDoShowTestMsg = false;
            dbgMsgText = "<color=green> Just test message </color>";
            ClearMsg();
        }

        public void ClearMsg()
        {
            text.text = DefaultText;
            isMsgShown = false;
            curTimeOut = 0;
            curTimer = curTimeOut + 0.001f;
            Debug.Log("Message cleared to default");
        }

        public bool dbgDoShowTestMsg;
        public string dbgMsgText = "<color=green> Just test message </color>";
        public float dbgMsgFloat;
        

        public void ShowMsg(string newMsgText, float newMsgTime = 0)
        {
            if (string.IsNullOrEmpty(newMsgText))
            {
                ClearMsg();
                return;
            }
            
            text.text = newMsgText;
            curTimeOut = (newMsgTime < 0.01 ? AutoClearTimeOut : newMsgTime);
            curTimer = 0;
            isMsgShown = true;

        }


        // Update is called once per frame
        void Update()
        {
            if (isMsgShown && IsAutoClear)
            {
                if (curTimer > curTimeOut)
                    ClearMsg();
                else
                    curTimer = curTimer + Time.deltaTime;
            }

            if (dbgDoShowTestMsg)
            {

                dbgDoShowTestMsg = false;
                ShowMsg(dbgMsgText, dbgMsgFloat);

            }
        }
    }
}
