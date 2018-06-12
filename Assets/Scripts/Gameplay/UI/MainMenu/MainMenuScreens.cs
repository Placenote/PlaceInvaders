using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Placenote;

namespace GameUiNs
{
    public class MainMenuScreens : PlacenotePunMultiplayerBehaviour
    {

        // Child object references
        public GameObject MainMenuButtons;
        public GameObject CreateHostRoom;
        public GameObject ViewRooms;
        public GameObject ErrorMessage;

        protected override void Start ()
        {
            base.Start ();
            MainMenuButtons.SetActive (true);
            CreateHostRoom.SetActive (false);
            ViewRooms.SetActive (false);
            ErrorMessage.SetActive (false);
        }

        protected override void OnHostRoomError (string error)
        {
            ErrorMessage.SetActive (true);
            ErrorMessage.GetComponent<ErrorMessage> ().SetErrorText (error);
        }

        protected override void OnGameQuit ()
        {
            MainMenuButtons.SetActive (true);
            CreateHostRoom.SetActive (false);
            ViewRooms.SetActive (false);
        }
    }
}