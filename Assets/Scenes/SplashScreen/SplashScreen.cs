using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SplashScreen
{
    public class SplashScreen : MonoBehaviour
    {
        public string NextSceneName = "_ARKitGameplay";
        public float timeOut = 3;
        // Use this for initialization
        IEnumerator Start()
        {
            yield return new WaitForSeconds(timeOut);
            Fader.LoadScene(NextSceneName);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
