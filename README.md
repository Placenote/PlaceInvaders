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
  *- Player 1 and Player 2 can join or leave at anytime. They just need to localize before they can play.*

<br/>

### Used technologies:

**ArKit**: https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-arkit-plugin-92515

**Placenote** (creating AR maps): https://github.com/Placenote/PlacenoteSDK-Unity

**Photon** (for Multiplayer): https://assetstore.unity.com/packages/tools/network/photon-unity-networking-free-1786

**Space Invader Models and Animations**: https://www.turbosquid.com/3d-models/space-invader-set-3d-model/768637

<br/>

### Important Scripts:

**Project Setup**: This project's workflow is split into three different sections: *Main Menu*, *Placenote Multiplayer Manager*, and *Gameplay Session*. This project is designed so that you can easily take the Placenote Multiplayer Manager section, apply that to your own game, and turn it into a multiplayer AR masterpiece!

#### <p align="center">Main Menu Section</p>

This section is all of the menus before a player joins or hosts a game. It does not have any super important scripts. It is mostly just Unity UI navigation. Note that the UI is **controlled by Unity's built in UI system.**

#### <p align="center">Placenote Multiplayer Manager Section</p>

**PlacenoteMultiplayerManager.cs**: this is the most important class in the project. It is the master class that controls all of the Placenote mapping and Photon Multiplayer integration you will need. It has functionality to handle Photon connection, room setup, room joining, and room leaving. It also handles Placenote mapping, map saving, map loading, and localization. It contains subscribable events designed to interface with the rest of the project. This class is mostly called on OnClick button events and MappingText.cs.

**PlacenotePunMultiplayerBehaviour.cs**: an extendable class that provides all callbacks that PlacenoteMultiplayerManager can call. Most UI scripts extend this class to get updates on Placenote and Photon changes.

**MappingText.cs**: controls the user feedback for a Placenote mapping session. Uses most of the callbacks from PlacenotePunMultiplayerBehaviour.

#### <p align="center">Gameplay Session Section</p>
**GameController.cs**: controls game logic including starting game, creating enemies, and managing players (creating new, registering, removing).

**GameData.cs**: stores game data about enemies and players. Also creates events about changes in game data and game state for UI components to update.

**PlayerPhotonGenerator.cs**: controls spawning of players.

**PlayerController.cs**: controls player lives, damaging enemies, and synchronizing with Photon.

**WeaponController.cs**: shoots enemies and calls missing or hit actions on AnimatedGun script.

**AnimatedGun.cs**: creates shot that damages enemies. Also controls shooting animation, time between shots, and synchronization shots with Photon.

**EnemyAI.cs**: controls enemies behaviour (attacking players, moving), and contains enemy properties( AttackPower, AttackTimeLimit, Speed etc.).

**EnemyState.cs**: contains enemies health, player shot interaction, and enemy death.

**RespawnPoint.cs**: controls spawn rate of enemy spawning.
