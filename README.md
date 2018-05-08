# Place Invaders

### Multiplayer Space Invaders using Placenote Unity SDK with ARkit and Photon

Placenote SDK: 1.4.2

Project's Unity Version: 2017.3.1f1

Xcode: 9.3

Devices tested OS: 11.3


Steps to Deploy on 2 devices:

***Before compiling to device:

Register for a Free Photon Multiplayer Account and place your account's "AppId" into the "PhotonServerSettings" in the editor.  Now every device your demo app is compiled to will be in the same "PrototypeRoom".

<img width="1101" alt="photon appid" src="https://user-images.githubusercontent.com/13069075/38306822-9d5942ca-37c6-11e8-95ee-387d4eb2a614.png">

Step 1.): open Placenote Invaders on both phones,

Step 2.) Player 1 and Player 2 must tap the top left "Multiplayer Button" (3 horizontal lines) and press the the "connect" button on right of the Multiplayer Screen to join the "PrototypeRoom".  *Note, both players must be connected to the Prototype Room before Player 1 scans the room.

Step 3.) Player 1 will press the "Placenote Scan button" (2nd button) then scan play area - moving side to side.  When finished scanning, Player 1 will press "Placenote Scan Button" again to complete the scan and save the space's data,

Step 4.) Now player 1 and player 2 will press the "Play" button (3rd button).
(and of course look at the same scanned area to synch location and position)


Current Simple Placenote UI:

![placenote simple ui](https://user-images.githubusercontent.com/13069075/38260093-8cc1da36-371b-11e8-94b6-1694eae0931e.png)



Used technologies:

ArKit: https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-arkit-plugin-92515

Placenote (creating AR maps): https://github.com/Placenote/PlacenoteSDK-Unity

Photon (for multiplaying): https://assetstore.unity.com/packages/tools/network/photon-unity-networking-free-1786

Space Invader Models with animation: https://www.turbosquid.com/3d-models/space-invader-set-3d-model/768637


Scripts:

GameController - control game logic: starting game, creating enemies, managing players (creating new, registering,removing)

PlayerController - control current player: lifes, fighting with enemies, synchronizing with Photon

WeaponController - Shooting enemies and call missing or hit actions on AnimatedGun script

AnimatedGun - make shots: damage enemies, shooting animation, control time between shots, synchronize shots with Photon

EnemyAI -   each enemy has this component, that control enemies behaviour (attacking players, moving), contain enemy properties( AttackPower, AttackTimeLimit, Speed etc.)

EnemyState - each enemy has this component, contain enemies health, and when player hit, decrease enemy health, when health <= 0, animated destroying enemy

GameData - Simple class mostly for UI  to store game data with support of notifications about changes, UI component have to be responsible itself to decide when it have be notified and which data should be shown

EnvironmentScannerController - managing Placenote (start scanning, finish scanning, get maps count, loading latest map,stop using map, delete all maps), starting ARKIt, control Placenote status

UiController -  control UI element: buttons, text labels, etc. Performs buttons click actions. Shows debug texts, informations about processes, placenote status on text labels

*Note: the scripts are commented to provide further insight to how this demo works.
