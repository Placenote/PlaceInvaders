using System.Collections;
using GameplayNs;
using UnityEngine;
using UnityEngine.UI;
using PunServerNs;

namespace GameUiNs
{

    public class MainMenuUIController : MonoBehaviour
    {
        // External class references.
        public ServerController Server;
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
        private RoomInfo[] roomsArray;
        public Text ViewRoomsText;

        // SDK Feedback
        public GameObject SDKFailedPanel;

        #endregion Main UI Elements


        private void Start ()
        {
            if (Server == null)
                Server = FindObjectOfType<ServerController> ();
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

            // SinglePlayer path. // TODO Delete singleplayer
            // SinglePlayerBtn.onClick.AddListener (StartSinglePlayer);

            Initialize ();
        }

        /// <summary>
        /// Reset the screen to the title screen main menu
        /// </summary>
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

            // Hide MenuUIs
            GameObject prevUI = (GameObject)UIStack.Peek ();
            prevUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            LoadingCircle.SetActive (true);

            string hostingRoomName = null;

            if (!LibPlacenote.Instance.Initialized ()) //TODO put this in better spot
            {
                SDKFailed ();
                return;
            }

            if (string.IsNullOrEmpty (HostingRoomNameInput.text))
            {
                FailToCreateRoom ();
                return;
            }
            else
                hostingRoomName = HostingRoomNameInput.text;

            // TODO Host the room then when room is hosted callback to GameSetupController to start mapping
            Server.HostRoom (hostingRoomName);
        }

        private void ActivateViewRoomsUI ()
        {
            if (!LibPlacenote.Instance.Initialized ()) //TODO put this in better spot
            {
                SDKFailed ();
                return;
            }

            Server.Connect ();
            LoadingCircle.SetActive (true);
            ViewRoomsText.text = "";
            DeleteViewRooms ();
            if (ViewRoomsPanel != null)
                ActivateUI (ViewRoomsPanel);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ViewRoomsText.text = "There are no rooms available...\nMake sure your device is connected to the internet.";
                LoadingCircle.SetActive (false);
            }
        }

        public void GoBack ()
        {
            GameObject currentUI = (GameObject)UIStack.Pop ();
            currentUI.SetActive (false);

            GameObject prevUI = (GameObject)UIStack.Peek ();
            prevUI.SetActive (true);

            LoadingCircle.SetActive (false);

            // Hide back button if at menu
            if (UIStack.Peek () == MainMenuPanel)
                BackBtn.gameObject.SetActive (false);

            // Disconnect if leaving the view rooms panel.
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
            GameObject currentUI = (GameObject)UIStack.Peek ();
            currentUI.SetActive (false);
            UIStack.Push (UIToActivate);
            UIToActivate.SetActive (!UIToActivate.activeSelf);

        }

        //TODO Delete 
        private void StartSinglePlayer ()
        {
            // Hide MenuUIs
            // GameObject prevUI = (GameObject)UIStack.Peek ();
            // prevUI.SetActive (false);
            // BackBtn.gameObject.SetActive (false);
            // GameSetup.EnvironmentMappingStart ();
        }

        #endregion > Buttons On Click Events


        #region Dynamic UI generation

        /// <summary>
        /// Generates all the available rooms that are nearby based on GPS
        /// </summary>
        public void GenerateViewRooms ()
        {
            // Clear the rooms first.
            DeleteViewRooms ();
            // Show loading circle while finding rooms
            LoadingCircle.SetActive (true);
            Debug.Log (Application.internetReachability == NetworkReachability.NotReachable);


            int counter = 0;
            roomsArray = Server.GetRooms ();
            bool NoRooms = true;
            if (roomsArray.Length > 0)
            {
                LoadingCircle.SetActive (false);
                NoRooms = false;
            }
            float roomLatitude;
            float roomLongitude;
            float[] roomGPS;

            foreach (RoomInfo game in roomsArray)
            {
                roomGPS = (float[])game.CustomProperties["GPS"];
                roomLatitude = roomGPS[0];
                roomLongitude = roomGPS[1];
                if (Mathf.Abs (roomLatitude - Server.mLatitude) <= Server.GPSThreshold && Mathf.Abs (roomLongitude - Server.mLongitude) <= Server.GPSThreshold)
                {
                    GameObject room = (GameObject)Instantiate (roomButtonPrefab);
                    int roomIndex = counter;
                    room.GetComponent<Button> ().onClick.AddListener (
                                                    () => { JoinRoom (roomIndex); }
                                                    );
                    room.transform.GetChild (0).GetComponent<Text> ().text = game.Name;
                    room.transform.GetChild (1).GetComponent<Text> ().text = game.PlayerCount + "/" + game.MaxPlayers;
                    room.transform.SetParent (RoomButtonsParent.transform, false);
                    counter++;
                }
            }
            if (NoRooms)
            {
                ViewRoomsText.text = "There are no rooms available...\nMake sure your device is connected to the internet.";
                LoadingCircle.SetActive (false);
            }
            else
                ViewRoomsText.text = "";

        }

        /// <summary>
        /// Removes all rooms from viewRoomsPanel
        /// </summary>
        void DeleteViewRooms ()
        {
            foreach (Transform child in RoomButtonsParent.transform)
            {
                GameObject.Destroy (child.gameObject);
            }
        }

        /// <summary>
        /// Error panel when room fails to be created
        /// </summary>
        public void FailToCreateRoom ()
        {
            GameObject currentUI = (GameObject)UIStack.Peek ();
            currentUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            LoadingCircle.SetActive (false);
            FailToCreateRoomPanel.SetActive (true);
        }

        /// <summary>
        /// Return to previous UI when user confirms that their room failed 
        /// to be created
        /// </summary>
        public void FailToCreateRoomConfirm ()
        {
            FailToCreateRoomPanel.SetActive (false);
            GameObject prevUI = (GameObject)UIStack.Peek ();
            prevUI.SetActive (true);
            BackBtn.gameObject.SetActive (true);
            LoadingCircle.SetActive (false);
        }

        public void SDKFailed ()
        {
            GameObject currentUI = (GameObject)UIStack.Peek ();
            currentUI.SetActive(false);
            BackBtn.gameObject.SetActive(false);
            LoadingCircle.SetActive(false);
            SDKFailedPanel.SetActive(true);
        }

        #endregion  Dynamic UI generation


        #region Networking

        void Disconnect ()
        {
            Server.Disconnect ();
        }

        void JoinRoom (int roomIndex)
        {
            GameObject currentUI = (GameObject)UIStack.Peek ();
            currentUI.SetActive (false);
            BackBtn.gameObject.SetActive (false);
            Server.JoinRoom (roomsArray[roomIndex].Name);
        }

        #endregion Networking
    }
}