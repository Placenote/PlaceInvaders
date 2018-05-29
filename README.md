# Place Invaders

### Multiplayer Place Invaders using Placenote Unity SDK with ARkit and Photon

Placenote SDK: 1.5.1

Project's Unity Version: 2018.1.1f1

Xcode: 9.3

Devices tested OS: 11.3

<br/>

### Steps to Deploy on 2 devices:

##### Before compiling to device:

1. Register for a Free Photon Multiplayer Account and place your account's "AppId" into the "PhotonServerSettings" in the editor.

<img width="1101" alt="photon appid" src="https://user-images.githubusercontent.com/13069075/38306822-9d5942ca-37c6-11e8-95ee-387d4eb2a614.png">

2. Register for a free Placenote account and get your api key here: https://developer.placenote.com. To Add your Placenote API key, find the ARCameraManager object in the Scene hierarchy and add your API key under the LibPlacenote component in the Inspector Panel.

3. Build the Unity project by Switching Platform to iOS in the Build Settings and click build to generate the XCode project. To build and run the XCode, follow the instructions on this page:
https://placenote.com/install/unity/build-xcode/

<br/>

**You will also need to add a "Privacy - Location When In Use Usage Description" key into your *Info.plist*. (This is so that you will only see photon rooms that are close by)**

##### Playing the Game:

1. Open Place Invaders on both phones.

2. Player 1 must press the *Host Game* button.

![PI_Title_Screen](/Screenshots/TitleScreen.png?raw=true "PI_Title_Screen")

3. Then Player 1 must enter a room name and press the arrow to create the room.

![PI_Hosting_Room](/Screenshots/HostRoom.png?raw=true "PI_Hosting_Room")

4. Player 2 must press the *Join Game* button. They will then be able to join Player 1's room once it is created.

![PI_View_Rooms](/Screenshots/ViewRooms.png?raw=true "PI_View_Rooms")

5. Player 1 will now be in the game and they must scan the play area - moving side to side.  When finished scanning, Player 1 will press the "Finish Mapping" button to complete the scan and save the map's data.

![PI_Mapping](/Screenshots/Mapping.png?raw=true "PI_Mapping")

6. Now both players must look at the same scanned area to synch location and position. Then Player 1 can press the start button.

![PI_Gameplay](/Screenshots/Gameplay.png?raw=true "PI_Gameplay")

*Note:* <br/>
  *- There is also a single player option. To play, press Single Player and follow the host steps.* <br/>
  *- Player 2 can join and leave at anytime. They just need to localize before they can play.*

<br/>

### Used technologies:

**ArKit**: https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-arkit-plugin-92515

**Placenote** (creating AR maps): https://github.com/Placenote/PlacenoteSDK-Unity

**Photon** (for Multiplayer): https://assetstore.unity.com/packages/tools/network/photon-unity-networking-free-1786

**Space Invader Models and Animations**: https://www.turbosquid.com/3d-models/space-invader-set-3d-model/768637

<br/>

### Important Scripts:

##### *The important folders are Scripts/Gameplay, Scripts/PlacenoteSample, and Scripts/ServerController*

**GameController.cs**: controls game logic including starting game, creating enemies, and managing players (creating new, registering,removing).

**GameSetupController.cs**: controls EnvironmentScannerController so that the map can be setup for gameplay. Starts gameplay after mapping.

**GameData.cs**: stores game data. Also creates events about changes in game data and game state for UI components to update.

**EnvironmentScannerController.cs**: manages Placenote (start mapping, finish mapping, get maps count, loading latest map,stop using map, delete all maps), starting ARKIt, and controls Placenote status.

**ServerController.cs**: controls photon connection, and manages room joining and creation.

**PlayerPhotonGenerator.cs**: controls spawning of players.

**PlayerController.cs**: controls current player including lives, fighting with enemies, and synchronizing with Photon.

**WeaponController.cs**: shooting enemies and call missing or hit actions on AnimatedGun script.

**AnimatedGun.cs**: creates shots that damage enemies. Also controls shooting animation, control time between shots, and synchronization shots with Photon.

**EnemyAI.cs**: controls enemies behaviour (attacking players, moving), and contains enemy properties( AttackPower, AttackTimeLimit, Speed etc.).

**EnemyState.cs**: contains enemies health, player shot interaction, and enemy death.

**RespawnPoint.cs**: controls spawn rate of enemy spawning.

**MainMenuUIController.cs/GameUIController.cs**: controls UI element: buttons, text labels, etc. Performs buttons click actions.

*Note: This project is being updated, and not everything is fully commented or organized. The scripts will be commented to provide further insight to how this demo works.*
