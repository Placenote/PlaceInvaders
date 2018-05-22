using System.Collections.Generic;
using System.Collections;
using System.Xml;
using GameplayNs;
using Placenote;
using UnityEngine;
using UnityEngine.UI;
using PunServerNs;
using TargetNs;

namespace GameUiNs
{

    public class MainMenuUIController : MonoBehaviour
    {
        public SrvController Srv;
        public GameUIController GameUI;
        public GameSetupController GameSetup;

        #region Main UI Elements

        // Stack to keep track of current UI layer.
        private Stack UIStack = new Stack ();

        // MainMenu.
        public GameObject MainMenuPanel;
        public Button JoinGameBtn;
        public Button HostGameBtn;
        public Button SinglePlayerBtn;

        // Back Button.
        public Button BackBtn;

        // Loading Circle.
        public GameObject LoadingCircle;

        // Host a room panel.
        public GameObject CreateHostRoomPanel;
        public InputField HostingRoomNameInput;
        public Button HostRoomBtn;
        public GameObject FailToCreateRoomPanel;
        public Button ConfirmBtn;


        // View all available rooms to join.
        public GameObject ViewRoomsPanel;
        public GameObject RoomButtonsParent;    // Need parent to instaniate room buttons in correct location.
        [SerializeField] GameObject roomButtonPrefab;
        private RoomInfo [] roomsArray;

        #endregion Main UI Elements



        private void Start ()
        {
            if (Srv == null)
                Srv = FindObjectOfType<SrvController> ();
            if (GameUI == null)
                GameUI = FindObjectOfType<GameUIController> ();
            if (GameSetup == null)
                GameSetup = FindObjectOfType<GameSetupController> ();

            BackBtn.onClick.AddListener (GoBack);

            // Hosting path.
            HostGameBtn.onClick.AddListener (ActivateHostRoomUI);
            HostRoomBtn.onClick.AddListener (HostRoom);
            ConfirmBtn.onClick.AddListener (FailToCreateRoomConfirm);

            // Joining path.
            JoinGameBtn.onClick.AddListener (ActivateViewRoomsUI);

            // SinglePlayer path.
            SinglePlayerBtn.onClick.AddListener (StartSinglePlayer);

            Initialize ();
        }

        public void Initialize ()
        {
            MainMenuPanel.SetActive (true);
            BackBtn.gameObject.SetActive (false);
            LoadingCircle.SetActive (false);
            CreateHostRoomPanel.SetActive (false);
            ViewRoomsPanel.SetActive (false);

            // Adding MainMenu to UIStack.
            UIStack = new Stack ();
            UIStack.Push (MainMenuPanel);
        }

        #region > Buttons On Click Events

        private void ActivateHostRoomUI ()
        {
            if (CreateHostRoomPanel != null)
                ActivateUI (CreateHostRoomPanel);
        }

        private void HostRoom ()
        {
            string hostingRoomName;
            if (string.IsNullOrEmpty (HostingRoomNameInput.text))
                hostingRoomName = "Untitled";
            else
                hostingRoomName = HostingRoomNameInput.text;
            // Hide MenuUIs
            GameObject prevUI = (GameObject) UIStack.Peek ();
            prevUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            LoadingCircle.SetActive (true);

            // TODO Host the room then when room is hosted callback to GameSetupController to start mapping
            Srv.HostRoom (hostingRoomName);
        }

        private void ActivateViewRoomsUI ()
        {
            Srv.Connect ();
            LoadingCircle.SetActive (true);
            DeleteViewRooms ();
            if (ViewRoomsPanel != null)
                ActivateUI (ViewRoomsPanel);
        }

        public void GoBack ()
        {
            GameObject currentUI = (GameObject) UIStack.Pop ();
            currentUI.SetActive (false);

            GameObject prevUI = (GameObject) UIStack.Peek ();
            prevUI.SetActive (true);

            LoadingCircle.SetActive (false);

            // Hide back button if at menu
            if (UIStack.Peek () == MainMenuPanel)
                BackBtn.gameObject.SetActive (false);

            // Discommect if leaving the view rooms panel.
            if (currentUI == ViewRoomsPanel)
            {
                Disconnect ();
            }
        }

        private void ActivateUI (GameObject UIToActivate)
        {
            // Show back button if last state was menu
            if (UIStack.Peek () == MainMenuPanel)
                BackBtn.gameObject.SetActive (true);
            // Hides currentUI
            GameObject currentUI = (GameObject) UIStack.Peek ();
            currentUI.SetActive (false);
            UIStack.Push (UIToActivate);
            UIToActivate.SetActive (!UIToActivate.activeSelf);

        }


        private void StartSinglePlayer ()
        {
            // Hide MenuUIs
            GameObject prevUI = (GameObject) UIStack.Peek ();
            prevUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);


            GameSetup.EnvironmentMappingStart ();
        }
        #endregion > Buttons On Click Events

        #region Dynamic UI generation

        public void GenerateViewRooms ()
        {
            // Clear the rooms first.
            DeleteViewRooms ();
            // Show loading circle while finding rooms
            LoadingCircle.SetActive (true);

            int counter = 0;
            roomsArray = Srv.GetRooms ();
            if (roomsArray.Length > 0)
                LoadingCircle.SetActive (false);
            float roomLatitude;
            float roomLongitude;
            float [] roomGPS;
            foreach (RoomInfo game in roomsArray)
            {
                roomGPS = (float []) game.CustomProperties ["GPS"];
                roomLatitude = roomGPS [0];
                roomLongitude = roomGPS [1];
                if (Mathf.Abs (roomLatitude - Srv.latitude) <= Srv.GPSThreshold && Mathf.Abs (roomLongitude - Srv.longitude) <= Srv.GPSThreshold)
               {
                    GameObject room = (GameObject) Instantiate (roomButtonPrefab);
                    int roomIndex = counter;
                    room.GetComponent<Button> ().onClick.AddListener (
                                                    () => { JoinRoom (roomIndex); }
                                                    );
                    room.transform.GetChild (0).GetComponent<Text> ().text = game.Name;
                    room.transform.GetChild (1).GetComponent<Text> ().text = game.PlayerCount + "/" + game.MaxPlayers;
                    room.transform.GetChild (2).GetComponent<Text> ().text = "Latitude: " + roomLatitude.ToString ();
                    room.transform.GetChild (3).GetComponent<Text> ().text = "Longitude: " + roomLongitude.ToString ();
                    room.transform.SetParent (RoomButtonsParent.transform, false);
                    counter++;
                }

            }
        }

        void DeleteViewRooms ()
        {
            foreach (Transform child in RoomButtonsParent.transform)
            {
                GameObject.Destroy (child.gameObject);
            }
        }

        public void FailToCreateRoom ()
        {
            GameObject currentUI = (GameObject) UIStack.Peek ();
            currentUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            FailToCreateRoomPanel.SetActive (true);
        }

        public void FailToCreateRoomConfirm ()
        {
            FailToCreateRoomPanel.SetActive (false);
            GameObject prevUI = (GameObject) UIStack.Peek ();
            prevUI.SetActive (true);
            BackBtn.gameObject.SetActive (true);
            LoadingCircle.SetActive (false);
        }

        #endregion  Dynamic UI generation

        #region Networking

        void Disconnect ()
        {
            Srv.Disconnect ();
        }

        void JoinRoom (int roomIndex)
        {
            GameObject currentUI = (GameObject) UIStack.Peek ();
            currentUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            Srv.JoinRoom (roomsArray [roomIndex].Name);
        }

        #endregion Networking
    }
}
