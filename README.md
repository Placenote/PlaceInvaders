# Place Invaders

### Multiplayer Place Invaders using Placenote Unity SDK with ARkit and Photon

Placenote SDK: 1.5.0

Project's Unity Version: 2018.1.1f1

Xcode: 9.3

Devices tested OS: 11.3


### Steps to Deploy on 2 devices

##### Before compiling to device:

1. Register for a Free Photon Multiplayer Account and place your account's "AppId" into the "PhotonServerSettings" in the editor.  Now every device your demo app is compiled to will be in the same "PrototypeRoom".

<img width="1101" alt="photon appid" src="https://user-images.githubusercontent.com/13069075/38306822-9d5942ca-37c6-11e8-95ee-387d4eb2a614.png">

2. Register for a free Placenote account and get your api key here: https://developer.placenote.com. To Add your Placenote API key, find the ARCameraManager object in the Scene hierarchy and add your API key under the LibPlacenote component in the Inspector Panel.

3. Build the Unity project by Switching Platform to iOS in the Build Settings and click build to generate the XCode project. To build and run the XCode, follow the instructions on this page:
https://placenote.com/install/unity/build-xcode/
**You will also need to add a "Privacy - Location When In Use Usage Description" key into your *Info.plist*. (This is so that you will only see photon rooms that are close by)**

##### Playing the Game:

1. Open Place Invaders on both phones.

2. Player 1 must press the *Host Game* button. (Add image)

3. Then they must enter a room name and press the arrow to create the room. (Add image)

4. Player 2 must press the *Join Game* button. They will then be able to join Player 1's room once it is created. (Add image)

5. Player 1 will now be in the game and they must scan the play area - moving side to side.  When finished scanning, Player 1 will press "Finished Mapping" button to complete the scan and save the space's data. (Add image)

6. Now both players must look at the same scanned area to synch location and position then Player 1 can press the start button.

*Note:*
  *- There is also a single player option. To play, press Single Player and follow the host steps.*
  *- Player 2 can join and leave at anytime. They just need to localize before they can play.*


### Used technologies:

ArKit: https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-arkit-plugin-92515

Placenote (creating AR maps): https://github.com/Placenote/PlacenoteSDK-Unity

Photon (for multiplaying): https://assetstore.unity.com/packages/tools/network/photon-unity-networking-free-1786

Space Invader Models with animation: https://www.turbosquid.com/3d-models/space-invader-set-3d-model/768637


### Important Scripts:

GameController - control game logic: starting game, creating enemies, managing players (creating new, registering,removing)

GameSetupController - controls EnvironmentScannerController so that the map can be setup for gameplay.  

GameData - Simple class mostly for UI  to store game data with support of notifications about changes, UI component have to be responsible itself to decide when it have be notified and which data should be shown

PlayerController - control current player: lifes, fighting with enemies, synchronizing with Photon

WeaponController - Shooting enemies and call missing or hit actions on AnimatedGun script

AnimatedGun - make shots: damage enemies, shooting animation, control time between shots, synchronize shots with Photon

EnemyAI -   each enemy has this component, that control enemies behaviour (attacking players, moving), contain enemy properties( AttackPower, AttackTimeLimit, Speed etc.)

EnemyState - each enemy has this component, contain enemies health, and when player hit, decrease enemy health, when health <= 0, animated destroying enemy

EnvironmentScannerController - managing Placenote (start scanning, finish scanning, get maps count, loading latest map,stop using map, delete all maps), starting ARKIt, control Placenote status

MainMenuUIController/GameUIController -  control UI element: buttons, text labels, etc. Performs buttons click actions. Shows debug texts, informations about processes, placenote status on text labels

*Note: This project is being updated, and not everything is commented or organized. The scripts will be commented to provide further insight to how this demo works.*
