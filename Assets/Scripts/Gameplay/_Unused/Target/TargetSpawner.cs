using System.Collections;
using UnityEngine;

namespace TargetNs
{
	public class TargetSpawner : MonoBehaviour
	{
	    public GameObject FlagPrefab;

	    private GameObject flagObject = null;
	    
	    #region > Singleton

	    public static TargetSpawner Instance
	    {
	        get
	        {
	            if (instance == null)
	                instance = FindObjectOfType<TargetSpawner>();
	            
	            if(instance == null)
	                instance = new GameObject("FlagController").AddComponent<TargetSpawner>();

	            return instance;
	        }
	    }

	    private static TargetSpawner instance = null;

	    #endregion > Singleton

	    public void InstantiateAfterDelay()
	    {
	        if(flagObject == null)
	            StartCoroutine(InstantiateAfterDelayImpl(1f));
	    }

	    private IEnumerator InstantiateAfterDelayImpl(float delay)
	    {
	        yield return new WaitForSeconds(delay);

	        InstantiateFlag();
	    }

	    public void InstantiateFlag()
	    {
			if (flagObject == null) {
//				flagObject = Instantiate (FlagPrefab, new Vector3 (0f, -2f, 2f), Quaternion.identity);
				//flagObject.transform.parent = gameObject.transform;
			}
	        
	       // flagObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
	    }



	}
}