using GameplayNs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameUiNs
{
    public class PlayerHUDPanel : GameEventsSubscriber
    {
        public Text Lives;
        public Text Kills;
        public GameObject ShootButton;

        override protected void OnGameEvent (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.GameStart:
                    break;
                case GameData.SomethingId.PlayerDied:
                    ShootButton.SetActive (false);
                    break;

                case GameData.SomethingId.GameOver:
                    ShootButton.SetActive (false);
                    break;

                case GameData.SomethingId.PlayerResurrected:
                    ShootButton.SetActive (true);
                    break;
            }
        }

        protected override void OnEnable ()
        {
            base.OnEnable ();
            ShootButton.SetActive (true);
            if (GameController.Instance != null)
            {
                Kills.text = GameController.Instance.Data.Kills.ToString ("D2");
                Lives.text = GameController.Instance.Data.Lives.ToString ("D2");
            }
        }

        override protected void OnGameDataChanged ()
        {
            Kills.text = GameController.Instance.Data.Kills.ToString ("D2");
            Lives.text = GameController.Instance.Data.Lives.ToString ("D2");
        }
    }
}