using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Fader : MonoBehaviour {
    const string FaderObjectName = "Fader";
    [HideInInspector]
	public bool isWorking = false;
    //[HideInInspector]
    //public
    const float defaultVelocityRate = 4f;
    float velocityRate = defaultVelocityRate;
    [HideInInspector]
    public string fadeScene;
    [HideInInspector]
    public float alpha = 0.0f;
    [HideInInspector]
    public Color fadeColor = Color.black;
    [HideInInspector]
    public bool isFadeIn = false;
    
    //Set callback
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    //Remove callback
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }


    public static Fader GetFader()
    {
        var ret = GameObject.FindObjectOfType<Fader>();
        if (ret == null)
        {
            GameObject newFaderObj = new GameObject();
            newFaderObj.name = FaderObjectName;
            ret = newFaderObj.AddComponent<Fader>();
            DontDestroyOnLoad(newFaderObj);
        }
        return ret;
    }

    public static void LoadScene(string newSceneName,float  damp = defaultVelocityRate)
    {
        var fader = GetFader();
        fader.velocityRate = damp;
        fader.fadeScene = newSceneName;
        fader.isWorking = true;
        fader.IsSceneLoading = false;
    }
    public static void Fade(string newSceneName, Color col , float damp )
    {
        
        var fader = GetFader();

        fader.velocityRate = damp;
        fader.fadeScene = newSceneName;
        fader.fadeColor = col;
        fader.isWorking = true;
        fader.IsSceneLoading = false;
    }

    bool IsSceneLoading = false;
    //Create a texture , Color it, Paint It , then Fade Away
    void OnGUI () {
        //Fallback check
        if (!isWorking)
			return;
        //Assign the color with variable alpha
		GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        //Temp Texture
		Texture2D tempTex;
		tempTex = new Texture2D (1, 1);
		tempTex.SetPixel (0, 0, fadeColor);
		tempTex.Apply ();
        //Print Texture
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), tempTex);
        //Fade in and out control
        if (isFadeIn)
			alpha = Mathf.Lerp (alpha, -0.1f, velocityRate * Time.deltaTime);
		else
			alpha = Mathf.Lerp (alpha, 1.1f, velocityRate * Time.deltaTime);
        //Load scene
		if (alpha >= 1 && !isFadeIn && !IsSceneLoading)
        {
            IsSceneLoading = true;
            SceneManager.LoadSceneAsync(fadeScene);
            Debug.Log("Fader: started async loading of "+ fadeScene);
            
		} else
		if (alpha <= 0 && isFadeIn)
        {
            isWorking = false;

            Destroy(gameObject);		
		}

	}

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //We can now fade in
        isFadeIn = true;
        IsSceneLoading = false;
    }

}
