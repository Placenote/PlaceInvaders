using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayNs
{
    public class OnGameStartEnabler : Photon.PunBehaviour
    {
        public bool ValueToApply = true;
        public List<GameObject> ObjectsToApply;
        public bool DisableAtGameobjectStart;

        private void Start()
        {
            if (DisableAtGameobjectStart)
                Apply(false);



        }

        void Apply(bool val)
        {
            if(ObjectsToApply != null)
                 ObjectsToApply.ForEach(o => { if (o != null) o.SetActive(val); });
        }

        #region event handlers to override

        virtual protected void __onGameStart()
        {
            Debug.Log(" -------- __onGameStart ------"+ ValueToApply);
            Apply(ValueToApply);
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
