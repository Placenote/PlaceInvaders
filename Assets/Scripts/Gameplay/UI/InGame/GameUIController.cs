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
        // External class references.
        public ServerController Server;
        public GameSetupController GameSetup;

        // Child object references.
        public GameObject DeathPanel;
        public GameObject GameOverPanel;
        public ShotButton ShotBtn;
        public GameObject CrossHairPanel;
        public GameInfoPanel InfoPanel;
        public GameObject MenuPanel;
        public Button StartGameBtn;
        public GameObject ButtonPanel;
        public Button FinishMappingBtn;
        public Button MainMenuBtn;
        public Text HelperText;
        public GameObject HelperTextPanel;
        public Button OpenMenuBtn;
        Button shotBtn;

        void Start ()
        {
            if (Server == null)
                Server = FindObjectOfType<ServerController> ();
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
                    InfoPanel.gameObject.SetActive (false);
                    OpenMenuBtn.gameObject.SetActive (true);
                    CrossHairPanel.gameObject.SetActive (false);
                    HelperText.gameObject.SetActive (true);
                    HelperTextPanel.SetActive (true);
                    if (PhotonNetwork.connected)
                    {
                        if (Server.IsHost)
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
                    OpenMenuBtn.gameObject.SetActive (false);
                    CrossHairPanel.gameObject.SetActive (false);
                    FinishMappingBtn.gameObject.SetActive (false);
                    StartGameBtn.gameObject.SetActive (false);
                    ButtonPanel.SetActive (true);
                    HelperText.gameObject.SetActive (false);
                    HelperTextPanel.SetActive (false);
                    break;
            }
        }


        #region Button Onclick events

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
            GameSetup.QuitGame ();
            GameController.Instance.QuitGame ();
        }

        #endregion Button OnClick events
    }
}
