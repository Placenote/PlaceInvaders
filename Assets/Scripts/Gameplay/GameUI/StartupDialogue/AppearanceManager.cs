using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUiNs
{
    [RequireComponent(typeof(Animator))]
    public class AppearanceManager : MonoBehaviour
    {
        public Animator anim;
        public string ShowTriggerName = "Show";
        public string HideTriggerName = "Hide";


        public bool IsReady; //{ get; private set; }

        // to be set from animation clip to disable gameobject
        public bool DoDisableGameobjectOnce = false;

        private void Start()
        {
            DoDisableGameobjectOnce = false;
        }

        private void Update()
        {
            if (!IsReady)
            {
                IsReady = true;
            }
            if (DoDisableGameobjectOnce)
            {
                DoDisableGameobjectOnce = false;
                gameObject.SetActive(false);
                IsHiding = false;
            }
        }

        public Animator Anim
        {
            get
            {
                if (anim == null)
                    anim = GetComponent<Animator>();
                return anim;

            }
        }


        public bool IsHiding
        {
            get; private set;
        }
            
        public void Show()
        {
            if(!string.IsNullOrEmpty(ShowTriggerName) && Anim != null)
                Anim.SetTrigger(ShowTriggerName);
        }

        public void Hide()
        {
            if (!string.IsNullOrEmpty(HideTriggerName) && Anim != null)
            {
                IsHiding = true;
                Anim.SetTrigger(HideTriggerName);
            }
            
        }


        public bool IsGameobjectActive
        {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }


    }
}
