using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Placenote;


namespace GameUiNs
{
    public class UIManager : PlacenotePunMultiplayerBehaviour
    {
        // Child object references
        public GameObject InitializationScreens;
        public GameObject MainMenuScreens;
        public GameObject PNMMScreens;
        public GameObject GameplayScreens;
        public GameObject MenuButton;
        public GameObject MenuPanel;

        protected override void Start ()
        {
            base.Start ();
            InitializationScreens.SetActive (true);
            MainMenuScreens.SetActive (false);
            PNMMScreens.SetActive (false);
            GameplayScreens.SetActive (false);
            MenuButton.SetActive (false);
            MenuPanel.SetActive (false);
        }

        public void ToggleMenu ()
        {
            MenuPanel.SetActive (!MenuPanel.gameObject.activeSelf);
        }

        // Turn on PNMM Screens when player joins a room.
        protected override void OnStartJoiningRoom ()
        {
            MainMenuScreens.SetActive (false);
            PNMMScreens.SetActive (true);
            MenuButton.SetActive (true);
        }

        protected override void OnHostRoomError (string error)
        {
            MainMenuScreens.SetActive (true);
            PNMMScreens.SetActive (false);
            MenuButton.SetActive (false);
        }

        // Turn off PNMM screens when gameplay begins.
        protected override void OnGameStart ()
        {
            PNMMScreens.SetActive (false);
            GameplayScreens.SetActive (true);
        }

        // Reset to MainMenu when user quits game session.
        protected override void OnGameQuit ()
        {
            MainMenuScreens.SetActive (true);
            PNMMScreens.SetActive (false);
            GameplayScreens.SetActive (false);
            MenuButton.SetActive (false);
            MenuPanel.SetActive (false);
        }
    }
}
