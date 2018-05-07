using GameplayNs;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Placenote
{
	public class UiController : MonoBehaviour 
	{
		[SerializeField] private Text StatusText;
		[SerializeField] private Button StartScanningBtn;
		[SerializeField] private Button FinishScanningBtn;
		[SerializeField] private Button GetMapsCountBtn;
		[SerializeField] private Button LoadLatestMapBtn;
		[SerializeField] private Button DeleteAllMapsBtn;
		
		private string loadLatestMapBtnText = "Load Latest Map";
		private string stopMapUsingBtnText = "Stop Map Using";
		private bool isUsingMap = false;
	
		

		private void Start()
		{
			Initialize();
		}
		
		#region > Initialization
		
		private void Initialize()
		{
			StartScanningBtn.onClick.AddListener(StartScanning);
			FinishScanningBtn.onClick.AddListener(FinishScanning);
			GetMapsCountBtn.onClick.AddListener(GetMapsCount);
			LoadLatestMapBtn.onClick.AddListener(LoadLatestMap);
			DeleteAllMapsBtn.onClick.AddListener(DeleteAllMaps);

			
			EnvironmentScannerController.Instance.OnARKitInitialized.AddListener(ARKitInitialized);
			EnvironmentScannerController.Instance.OnPlacenoteStatusChange.AddListener(PlacenoteStatusChange);
		}
		
		#endregion

		private void ARKitInitialized()
		{
			StatusText.text = "ARKit Initialized";
		}

		private void PlacenoteStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
		{
			if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) 
			{				
				StatusText.text = "Localized";
			} 
			else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
				StatusText.text = "Mapping";
			} 
			else if (currStatus == LibPlacenote.MappingStatus.LOST) {
				StatusText.text = "Searching for position lock";
			}
		}

		#region > Buttons Listeners
	    
		#region >> Start Scanning
		
	    private void StartScanning()
	    {
		    if (EnvironmentScannerController.Instance.StartScanning())
		    {
			    StartScanningBtn.gameObject.SetActive(false);
			    FinishScanningBtn.gameObject.SetActive(true);
			    GetMapsCountBtn.gameObject.SetActive(false);
			    LoadLatestMapBtn.gameObject.SetActive(false);
			    DeleteAllMapsBtn.gameObject.SetActive(false);
		    }
		    else
		    {
			    StatusText.text = "SDK not yet initialized";
		    }
	    }

		#endregion
		
		#region >> Finish Scanning
		
	    private void FinishScanning()
	    {
		    FinishScanningBtn.gameObject.SetActive(false);
		    EnvironmentScannerController.Instance.FinishScanning(
			    SavingFinished,
			    (percent) =>
			    {
				    StatusText.text = "Saving..." + (percent * 100f) + "%";
			    });
	    }

		private void SavingFinished(bool success)
		{
			StatusText.text = success ? "Map Saved Successfully" : "Map Saving Error";
			
			StartScanningBtn.gameObject.SetActive(true);
			FinishScanningBtn.gameObject.SetActive(false);
			GetMapsCountBtn.gameObject.SetActive(true);
			LoadLatestMapBtn.gameObject.SetActive(true);
			DeleteAllMapsBtn.gameObject.SetActive(true);
		}

		#endregion
		
		#region >> Maps Count
		
	    private void GetMapsCount()
	    {
		    if (EnvironmentScannerController.Instance.GetMapsCount(MapsLoadedShowCount))
		    {
			    StatusText.text = "Loading Maps From Remote...";
		    }
		    else
		    {
			    StatusText.text = "SDK not yet initialized";
		    }
	    }

		private void MapsLoadedShowCount(LibPlacenote.MapInfo[] maps)
		{
			if(maps.Length == 0)
				StatusText.text = "Maps Count - 0";
			else
				StatusText.text = "Maps Count - " + maps.Length + "; Latest ID - " + maps.Last().placeId;
		}
	    		
		#endregion
		
		#region >> Latest Map Loading
		
	    private void LoadLatestMap()
	    {
		    if (!isUsingMap)
		    {
			    //Load Latest Map
			    if (EnvironmentScannerController.Instance.LoadLatestMap(LoadingLatestMapLoaded, LoadingLatestMapFailed, LoadingLatestMapPercentage))
			    {
				    StatusText.text = "Loading Maps From Remote...";
				    StartScanningBtn.gameObject.SetActive(false);
				    FinishScanningBtn.gameObject.SetActive(false);
				    GetMapsCountBtn.gameObject.SetActive(false);
				    DeleteAllMapsBtn.gameObject.SetActive(false);
				   // LoadLatestMapBtn.interactable = false;
			    }
			    else
			    {
				    StatusText.text = "SDK not yet initialized";
			    }
		    }
		    else
		    {
			    //Stop Map Using
			    EnvironmentScannerController.Instance.StopUsingMap();
			    isUsingMap = false;
			    StartScanningBtn.gameObject.SetActive(true);
			    FinishScanningBtn.gameObject.SetActive(true);
			    GetMapsCountBtn.gameObject.SetActive(true);
			    DeleteAllMapsBtn.gameObject.SetActive(true);
			    LoadLatestMapBtn.GetComponentInChildren<Text>().text = loadLatestMapBtnText;
		    }
	    }

		private void LoadingLatestMapLoaded(string mapId)
		{
			StatusText.text = "Map Loaded ID - " + mapId;
			LoadLatestMapBtn.interactable = true;
			LoadLatestMapBtn.GetComponentInChildren<Text>().text = stopMapUsingBtnText;
			isUsingMap = true;
            GameController.Data.NotifyWorldRootPlaced();


        }

		private void LoadingLatestMapFailed(string mapId)
		{
			StatusText.text = "Map Loading Failed! ID - " + mapId;
			StartScanningBtn.gameObject.SetActive(true);
			FinishScanningBtn.gameObject.SetActive(true);
			GetMapsCountBtn.gameObject.SetActive(true);
			DeleteAllMapsBtn.gameObject.SetActive(true);
			LoadLatestMapBtn.interactable = true;
		}

		private void LoadingLatestMapPercentage(float percentage)
		{
			StatusText.text = "Loading Latest Map..." + (percentage * 100f) + "%";
		}
		
		#endregion

		#region >> All Maps Deleting
		
	    private void DeleteAllMaps()
	    {
		    if (EnvironmentScannerController.Instance.DeleteAllMaps(AllMapsDeleted, MapDeleting))
		    {
			    StatusText.text = "Deleting All Maps...";
			    
			    StartScanningBtn.gameObject.SetActive(false);
			    FinishScanningBtn.gameObject.SetActive(false);
			    GetMapsCountBtn.gameObject.SetActive(false);
			    LoadLatestMapBtn.gameObject.SetActive(false);
			    DeleteAllMapsBtn.interactable = false;
		    }
		    else
		    {
			    StatusText.text = "SDK not yet initialized";
		    }
	    }

		private void AllMapsDeleted(bool deleteStatus)
		{
			if (deleteStatus)
				StatusText.text = "All Maps Deleted";
			else
				StatusText.text = "Maps Deleting Error!";
			
			StartScanningBtn.gameObject.SetActive(true);
			FinishScanningBtn.gameObject.SetActive(false);
			GetMapsCountBtn.gameObject.SetActive(true);
			LoadLatestMapBtn.gameObject.SetActive(true);
			DeleteAllMapsBtn.interactable = true;
		}
		
		private void MapDeleting(string mapId)
		{
			StatusText.text = "Deleting Map ID - " + mapId;
		}
	    
		#endregion
		
	    #endregion
		
		
	}
}
