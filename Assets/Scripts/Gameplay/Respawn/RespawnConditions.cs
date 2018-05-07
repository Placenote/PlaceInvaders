using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RespawnNs
{
    [System.Serializable]
    public class RespawnCondition
    {

        [Tooltip(" Title to show when item is active")]
        public string Title = "New wave of enemies";

        [Tooltip(" How much time should elapset since start, negative value means 'undefined'")]
        public float ElapsedTime = 10;

        [Tooltip(" How much kills should be done, negative value means 'undefined'")]
        public int EnemiesKilled = 0;

        [Tooltip(" How much enemies have to be generated on activation of this item")]
        public int GenerateOnEnter = 0;

        [Header("Setup generation on timeout while this item is actual")]
        public float TimeOut = 15;
        public int GenerateOnTimeOut = 1;

    }

    [System.Serializable]
    public class RespawnConditions
    {

        [Header("Public for debug only purposes, use methods instead of these variables")]
        public int CurrentItem = -1;
        public List<RespawnCondition> Schedule;
    }
}
