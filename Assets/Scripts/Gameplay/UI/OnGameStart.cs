using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayNs
{
    public class OnGameStart : MonoBehaviour
    {

        [System.Serializable]
        public class OnGameStartEvent : UnityEvent { }

        public OnGameStartEvent OnReceiving;

        #region event handlers to override

        virtual protected void __onGameStart()
        {
            if (GameController.Instance != null)
                OnReceiving.Invoke();
            else
                Debug.Log("Attempt to call __onGameStart when the GameController is not ready");
        }

        virtual protected void NotifySomethingHappened(GameData.SomethingId id)
        {
            if (id == GameData.SomethingId.GameStart)
                __onGameStart();
        }
        #endregion event handlers to override


        #region Subscribing to events

        bool doSubscibe = true;


        private void OnEnable()
        {
            doSubscibe = true;
        }
        private void OnDisable()
        {
            if (GameController.Data != null)
                GameController.Data.NotifySomethingHappened -= NotifySomethingHappened;
        }

        protected virtual void Subscribe()
        {
            GameController.Data.NotifySomethingHappened += NotifySomethingHappened;
        }


        void Update()
        {
            if (doSubscibe)
            {
                doSubscibe = false;
                Subscribe();

            }
        }
        #endregion Subscribing to events

    }
}
