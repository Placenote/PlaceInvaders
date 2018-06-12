using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Placenote
{
    /// <summary>
    /// This class provides all callbacks that PlacenoteMultiplayerManager can call. 
    /// Override the events/methods you want to use.
    /// IMPORTANT: This class has callbacks included in PunBehaviour. They exist within this class, so you 
    /// don't have to implement both PunBehaviour and PlacenotePunMultiplayerBehaviour.
    /// </summary>
    /// <remarks>
    /// When extending this class into a class that has Start (), make sure to call base.Start().
    /// By extending this class, you can implement individual methods as override.
    /// Visual Studio and MonoDevelop should provide the list of methods when you begin typing "override".
    /// Your implementation does not have to call "base.method()" (except for Start()).
    /// </remarks>
    public class PlacenotePunMultiplayerBehaviour : MonoBehaviour
    {
        protected virtual void Start ()
        {
            // Photon connection events
            PlacenoteMultiplayerManager.Instance.OnConnectedToPhotonEvent += OnConnectedToPhoton;
            PlacenoteMultiplayerManager.Instance.OnFailedToConnectToPhotonEvent += OnFailedToConnectToPhoton;

            // Room events
            PlacenoteMultiplayerManager.Instance.OnRoomListUpdateEvent += OnRoomListUpdate;
            PlacenoteMultiplayerManager.Instance.OnStartJoiningRoomEvent += OnStartJoiningRoom;
            PlacenoteMultiplayerManager.Instance.OnJoinedRoomEvent += OnJoinedRoom;
            PlacenoteMultiplayerManager.Instance.OnJoinedMappedRoomEvent += OnJoinedMappedRoom;
            PlacenoteMultiplayerManager.Instance.OnHostRoomErrorEvent += OnHostRoomError;

            // Mapping events
            PlacenoteMultiplayerManager.Instance.OnMappingStartEvent += OnMappingStart;
            PlacenoteMultiplayerManager.Instance.OnMappingFailedEvent += OnMappingFailed;
            PlacenoteMultiplayerManager.Instance.OnMappingCompleteEvent += OnMappingComplete;
            PlacenoteMultiplayerManager.Instance.OnMappingSufficientEvent += OnMappingSufficient;

            // Mapping progress events
            PlacenoteMultiplayerManager.Instance.OnMapSavingStatusUpdateEvent += OnMapSavingStatusUpdate;
            PlacenoteMultiplayerManager.Instance.OnMapSavingProgressUpdateEvent += OnMapSavingProgressUpdate;
            PlacenoteMultiplayerManager.Instance.OnMapLoadingStatusUpdateEvent += OnMapLoadingStatusUpdate;
            PlacenoteMultiplayerManager.Instance.OnMapLoadingProgressUpdateEvent += OnMapLoadingProgressUpdate;

            // Localization events
            PlacenoteMultiplayerManager.Instance.OnLocalizationLostEvent += OnLocalizationLost;
            PlacenoteMultiplayerManager.Instance.OnLocalizedEvent += OnLocalized;

            // Game events
            PlacenoteMultiplayerManager.Instance.OnGameStartEvent += OnGameStart;
            PlacenoteMultiplayerManager.Instance.OnGameQuitEvent += OnGameQuit;

            // Player counting value events
            PlacenoteMultiplayerManager.Instance.OnPlayerValueUpdateEvent += OnPlayerValueUpdate;
        }

        #region Photon connection events
        /// <summary>
        /// Extended from PunBehaviour.
        /// Called when connection to Photon server is successful.
        /// </summary>
        protected virtual void OnConnectedToPhoton () { }

        /// <summary>
        /// Extended from PunBehaviour.
        /// Called when connection to Photon server fails.
        /// </summary>
        protected virtual void OnFailedToConnectToPhoton () { }

        #endregion Photon connection events

        #region Room events

        /// <summary>
        /// Extended from PunBehaviour.
        /// Called when the list of Photon rooms is updated.
        /// (Ie. someone creates or destorys a room).
        /// </summary>
        protected virtual void OnRoomListUpdate () { }

        /// <summary>
        /// Called when a player attempts to start joining a photon room.
        /// </summary>
        protected virtual void OnStartJoiningRoom () { }

        /// <summary>
        /// Extended from PunBehaviour.
        /// Called when player successfully joins a room.
        /// </summary>
        protected virtual void OnJoinedRoom () { }

        /// <summary>
        /// Called when player successfully joins a room that has a MapId set
        /// </summary>
        protected virtual void OnJoinedMappedRoom () { }

        /// <summary>
        /// Called when player attempts to host room without a room name
        /// OR that room name already exists.
        /// </summary>
        /// <param name="error">Reason for error.</param>
        protected virtual void OnHostRoomError (string error) { }

        #endregion Room events

        #region Mapping events
        /// <summary>
        /// Called when player (should only be the host) successfully starts a mapping session.
        /// </summary>
        protected virtual void OnMappingStart () { }

        /// <summary>
        /// Called when player (should only be the host) fails to start a mapping session.
        /// A mapping session will fail if the SDK is not initialized.
        /// </summary>
        protected virtual void OnMappingFailed () { }

        /// <summary>
        /// Called when the mapping session has ended AND the map has saved successfully
        /// </summary>
        protected virtual void OnMappingComplete () { }

        /// <summary>
        /// Called when the mapping session has sufficient points.
        /// </summary>
        protected virtual void OnMappingSufficient () { }

        #endregion Mapping events

        #region Mapping progress events

        /// <summary>
        /// Called when the Placenote session finishes saving a map.
        /// Sends result of saving.
        /// </summary>
        protected virtual void OnMapSavingStatusUpdate (bool savingResult) {}

        /// <summary>
        /// Called continuously while map saving is happening.
        /// Sends current saving progress.
        /// </summary>
        protected virtual void OnMapSavingProgressUpdate (float progress) {}

        /// <summary>
        /// Called when the Placenote session finishes loading a map.
        /// Sends result of loading.
        /// </summary>
        protected virtual void OnMapLoadingStatusUpdate (bool loadingResult) {}

        /// <summary>
        /// Called continuously while map loading is happening.
        /// Send current loading progress
        /// </summary>
        protected virtual void OnMapLoadingProgressUpdate (float progress) {}

        #endregion Mapping progress events

        #region Localization events

        /// <summary>
        /// Called when the player loses localization with the map
        /// </summary>
        protected virtual void OnLocalizationLost () { }

        /// <summary>
        /// Called when the player localizes with the map
        /// </summary>
        protected virtual void OnLocalized () { }

        #endregion Localization events

        #region Game events

        /// <summary>
        /// Called when game starts
        /// (Game refers to the gameplay after the mapping and localization is
        /// complete).
        /// </summary>
        protected virtual void OnGameStart () { }

        /// <summary>
        /// Called when player quits game.
        /// </summary>
        protected virtual void OnGameQuit () { }

        #endregion Game events

        #region Player counting value events

        /// <summary>
        /// Called when player joins/leaves a room, or another player joins/leaves the room.
        /// </summary>
        protected virtual void OnPlayerValueUpdate () { }

        #endregion Player counting value events
    }
}