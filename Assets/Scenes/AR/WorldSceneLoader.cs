using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.XR.iOS;
using ARKitSupportNs;

public class WorldSceneLoader : MonoBehaviour
{
    public string CurrentSceneName = null;
    public string SceneNameToLoad = "EmptySampleWorldScene";

    //  public GameObject ZeroPoint;
    public float Height = 2;
    public float MinHeight = 0;
    public float MaxHeight = 2;

    public Dropdown dropDown;

    public Button HideUIButton;
    public Button PlaceSphereButton;
    public Scrollbar scrollbar;

    public TapsTrigger TapsSensor;

   public WorldRoot sphereRoot;

    public Vector3 LastHitPosition;
    public Quaternion LastHitRotation;
    public Vector3 LastHitUp;

   

//    void UpdateLastHitData()
//    {
//        if (HitTestCube == null)
//            return;
//
//        LastHitPosition = HitTestCube.transform.position;
//        LastHitRotation = HitTestCube.transform.rotation;
//        LastHitUp = HitTestCube.transform.up;
//
//    }

//    ARHitTest HitTestCube;



    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    // Use this for initialization
    void Start ()
    {
        StartCoroutine(LoadDefaultScene());

     //   HitTestCube = FindObjectOfType<ARHitTest>();

        GameObject.DontDestroyOnLoad(gameObject);
        dropDown.onValueChanged.AddListener(OnNewSceneSelected);
        HideUIButton.onClick.AddListener(OnHideUIButtonClicked);
        PlaceSphereButton.onClick.AddListener(PlaceSphere);
        PlaceSphereButton.interactable = false;
        Height = Mathf.Clamp(Height,MinHeight, MaxHeight);

        scrollbar.onValueChanged.AddListener(OnScrollbarChanged);

        
//        UpdateLastHitData();

    }


    void OnScrollbarChanged(float newVal)
    {
        Height = Mathf.Lerp(MinHeight, MaxHeight, newVal);
        ChangeSphereHeight();
    }


    void OnHideUIButtonClicked()
    {

//        int newCameraMask = Camera.main.cullingMask ^ (1 << ARHitTest.planeLayerId);
//        Camera.main.cullingMask = newCameraMask;
//
//        HitTestCube.gameObject.SetActive(false);
//        gameObject.SetActive(false);
    }


    void ChangeSphereHeight()
    {
        if (sphereRoot != null && sphereRoot.gameObject != null)
        {
            sphereRoot.transform.rotation = LastHitRotation;
            sphereRoot.transform.position = LastHitPosition + Height * LastHitUp;

 
        }

    }

    void PlaceSphere()
    {
        if (sphereRoot != null && sphereRoot.gameObject != null)
        {
          //  UpdateLastHitData();
            ChangeSphereHeight();
        }

    }

    void OnNewSceneSelected(int newIdx)
    {
        var selection = dropDown.options[newIdx];
        LoadScene(selection.text);
    }

    IEnumerator LoadDefaultScene()
    {
        yield return new WaitForSeconds(5);
        LoadScene(SceneNameToLoad);
    }

    void LoadScene(string newScene)
    {

        if (string.IsNullOrEmpty(newScene))
            return;

        TapsSensor.enabled = false;
   //     HitTestCube.IgnoreTaps = true;
        if (CurrentSceneName != null)
        {
            Scene sc = SceneManager.GetSceneByName(CurrentSceneName);
            if (sc.IsValid() && sc.isLoaded)
            {
                sphereRoot = null;
                PlaceSphereButton.interactable = false;
                SceneManager.UnloadSceneAsync(CurrentSceneName);
            }
        }

        CurrentSceneName = null;
        SceneNameToLoad = newScene;
        SceneManager.LoadSceneAsync(SceneNameToLoad, LoadSceneMode.Additive);
    }



    void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        Debug.Log("Loaded " + loadedScene.name + "  -------------  ");
        string sceneName = loadedScene.name;
        if (sceneName.Equals(SceneNameToLoad, System.StringComparison.OrdinalIgnoreCase))
        {
           // SceneManager.sceneLoaded -= OnSceneLoaded;
            CurrentSceneName = SceneNameToLoad;
            SceneNameToLoad = null;
          //  HitTestCube.IgnoreTaps = false;
            TapsSensor.enabled = true;

           GameObject[] objects = loadedScene.GetRootGameObjects();
            foreach (var obj in objects)
            {

                var root = obj.GetComponentInChildren<WorldRoot>();
                if (root != null)
                {
                    root.ClearObjectsToDelete();
                    sphereRoot = root;
                    PlaceSphereButton.interactable = true;
                    ChangeSphereHeight();
                }


                /*
                Vector3 position = obj.transform.position;
                Quaternion rotation = obj.transform.rotation;
                obj.transform.SetParent(ZeroPoint.transform);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;
                */
            }

        }
        else
            Debug.Log(" ---   Ignoring "+ sceneName + " as waiting "+ (SceneNameToLoad??"null"));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
