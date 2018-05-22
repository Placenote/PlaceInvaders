using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class GameInfoPanel : EventsSubscriber
    {
        public Text Lives;
        public Text Kills;
        public Button Heart;

        private void Start ()
        {
            Heart.onClick.AddListener (() => GameController.Data.ResurrectPlayer ());
        }

        protected override void OnEnable ()
        {
            base.OnEnable ();
            Kills.text = GameController.Data.Kills.ToString ("D2");
            Lives.text = GameController.Data.Lives.ToString ("D2");
        }

        override protected void NotifySomeDataChanged ()
        {
            Kills.text = GameController.Data.Kills.ToString ("D2");
            Lives.text = GameController.Data.Lives.ToString ("D2");
        }

        protected override void Update ()
        {
            base.Update ();
        }

    }
}
