using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameplayNs;
using Placenote;
using PunServerNs;

namespace GameUiNs
{
    public class GameUIController : EventsSubscriber
    {

        public SrvController Srv;
        public GameSetupController GameSetup;

        public GameObject DeathPanel;
        public GameObject GameOverPanel;
        public ShotButton ShotBtn;
        public GameObject CrossHairPanel;
        public GameInfoPanel InfoPanel;
        public GameObject MenuPanel;
        public Button StartGameBtn;
        public GameObject ButtonPanel;

        // Mapping finish button
        public Button FinishMappingBtn;

        // Main Menu button
        public Button MainMenuBtn;

        // Text to guide user
        public Text HelperText;

        // GameInfoPanel
        public Button OpenMenuBtn;

        Button shotBtn;

        void Start ()
        {
            if (Srv == null)
                Srv = FindObjectOfType<SrvController> ();
            if (GameSetup == null)
                GameSetup = FindObjectOfType<GameSetupController> ();
            shotBtn = ShotBtn.GetComponent<Button> ();
            FinishMappingBtn.onClick.AddListener (MappingStop);
            StartGameBtn.onClick.AddListener (StartGame);
            MainMenuBtn.onClick.AddListener (GoToMainMenu);

            OpenMenuBtn.onClick.AddListener (ToggleGameMenu);
        }
        protected override void Update ()
        {
            base.Update ();
        }


        override protected void NotifySomethingHappened (GameData.SomethingId id)
        {
            switch (id)
            {
                case GameData.SomethingId.PlayerDied:
                    DeathPanel.SetActive (true);
                    ShotBtn.StopFiring ();
                    ShotBtn.enabled = false;
                    shotBtn.interactable = false;
                    break;

                case GameData.SomethingId.GameOver:
                    DeathPanel.SetActive (false);
                    GameOverPanel.SetActive (true);
                    ShotBtn.StopFiring ();
                    ShotBtn.enabled = false;
                    shotBtn.interactable = false;
                    break;

                case GameData.SomethingId.PlayerResurrected:
                    DeathPanel.SetActive (false);
                    ShotBtn.enabled = true;
                    shotBtn.interactable = true;
                    break;

                case GameData.SomethingId.GamePreparing:
                    ShotBtn.gameObject.SetActive (false);
                    ShotBtn.enabled = false;
                    shotBtn.interactable = false;
                    InfoPanel.gameObject.SetActive (true);
                    CrossHairPanel.gameObject.SetActive (false);
                    HelperText.gameObject.SetActive (true);
                    if (PhotonNetwork.connected)
                    {
                        if (Srv.IsHost)
                            FinishMappingBtn.gameObject.SetActive (true);
                    }
                    else
                    {
                        FinishMappingBtn.gameObject.SetActive (true);
                    }
                    break;

                case GameData.SomethingId.GameStart:
                    ShotBtn.gameObject.SetActive (true);
                    ShotBtn.enabled = true;
                    shotBtn.interactable = true;
                    InfoPanel.gameObject.SetActive (true);
                    CrossHairPanel.gameObject.SetActive (true);
                    break;

                case GameData.SomethingId.ToMainMenu:
                    MenuPanel.gameObject.SetActive (false);
                    ShotBtn.gameObject.SetActive (false);
                    DeathPanel.SetActive (false);
                    GameOverPanel.SetActive (false);
                    ShotBtn.StopFiring ();
                    ShotBtn.enabled = false;
                    shotBtn.interactable = false;
                    InfoPanel.gameObject.SetActive (false);
                    CrossHairPanel.gameObject.SetActive (false);
                    FinishMappingBtn.gameObject.SetActive (false);
                    StartGameBtn.gameObject.SetActive (false);
                    ButtonPanel.SetActive (true);
                    HelperText.gameObject.SetActive (false);
                    break;
            }
        }

        private void MappingStop ()
        {
            FinishMappingBtn.gameObject.SetActive (false);
            GameSetup.EnvironmentMappingStop ();
        }

        private void StartGame ()
        {
            GameSetup.StartGame ();
        }

        private void ToggleGameMenu ()
        {
            MenuPanel.gameObject.SetActive (!MenuPanel.gameObject.activeSelf);
            ButtonPanel.gameObject.SetActive (!MenuPanel.gameObject.activeSelf);

            return;
        }

        private void GoToMainMenu ()
        {
            // Remove player is handeled by PlayerPhotonGenerator
            // Remove enemies
            GameController.RemoveAllEnemies ();
            GameController.Instance.QuitGame ();
            GameController.Data.OnToMainMenu ();
        }



    }
}
