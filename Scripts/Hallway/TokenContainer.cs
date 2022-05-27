using System.Collections;
using Experiments.EndlessHallway.Managers;
using UnityEngine;

namespace Experiments.EndlessHallway.Gameplay
{
    public class TokenContainer : MonoBehaviour
    {
        public GameObject TokenObject;
        [HideInInspector]
        public float SpawnValue;

        // Update is called once per frame
        void Update()
        {
            TokenObject.SetActive(SpawnValue <= GameManager.Instance.CoinSpawnPercent && !GameManager.Instance.IsExplorer);
        }
    }
}