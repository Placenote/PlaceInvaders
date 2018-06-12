using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayNs;

namespace GameUiNs
{
    public class GameplayScreens : GameEventsSubscriber
    {
        // Child object references
        public GameObject PlayerHUDPanel;
        public GameObject DeathPanel;
        public GameObject GameOverPanel;

        protected override void OnGameEvent (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.PlayerDied:
                    DeathPanel.SetActive (true);
                    break;

                case GameData.SomethingId.GameOver:
                    DeathPanel.SetActive (false);
                    GameOverPanel.SetActive (true);
                    break;

                case GameData.SomethingId.PlayerResurrected:
                    DeathPanel.SetActive (false);
                    break;
            }
        }

        /// <summary>
        /// Similar to GameStart event, 
        /// but GameStart is sometimes called before this object is active
        /// </summary>
        protected override void OnEnable ()
        {
            base.OnEnable ();   
            // Must be enabled to trigger OnEnable on PlayerHUDPanel script
            PlayerHUDPanel.SetActive (true);
            DeathPanel.SetActive (false);
            GameOverPanel.SetActive (false);
        }

        /// <summary>
        /// Similar to ToMainMenu event, 
        /// but ToMainMenu is sometimes called after this object is inactive
        /// </summary>
        protected override void OnDisable ()
        {
            base.OnDisable ();
            PlayerHUDPanel.SetActive (false);
            DeathPanel.SetActive (false);
            GameOverPanel.SetActive (false);
        }
    }   
}