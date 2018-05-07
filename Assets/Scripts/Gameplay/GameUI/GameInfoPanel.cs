using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class GameInfoPanel : EventsSubscriber
    {
        public Text Lifes;
        public Text Kills;
        public Button Heart;
        public Button Info;

        private void Start()
        {
            Heart.onClick.AddListener( () => GameController.Data.ResurrectPlayer());
            //Heart.interactable = false;
        }

        override protected void NotifySomeDataChanged()
        {
            Kills.text = GameController.Data.Kills.ToString("D3");
            Lifes.text = GameController.Data.Lifes.ToString("D2");
        }

        protected override void NotifySomethingHappened(GameData.SomethingId id)
        {
          // if(id == GameData.SomethingId.GameStart)
          //      Heart.interactable = true;
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
