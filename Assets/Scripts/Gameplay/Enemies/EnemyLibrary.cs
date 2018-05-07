using System.Collections;
using System.Collections.Generic;
using UniHoloToolkit;
using UnityEngine;

namespace EnemiesNs
{
   
    [System.Serializable]
    public class EnemyLibrary 
    {
        public string EnemyTag = "Enemy";
        public string DamageReceiverMethodName = "Damage";

        public List<EnemyDef> Definitions;
        public EnemyDef GetRandomEnemy()
        {
            int idx = Random.Range(0, Definitions.Count);
            return Definitions[idx];
        }
    }
}
