using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Placenote;

namespace GameUiNs
{
    public class FinishMappingButton : MonoBehaviour
    {
        public MappingText mappingText;

        public void FinishMapping ()
        {
            if (mappingText != null)
                PlacenoteMultiplayerManager.Instance.
                StopMapping (mappingText.MapSavingStatus,
                mappingText.MapSavingProgress);
        }
    }
}
