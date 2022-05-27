using System.Collections;
using Experiments.EndlessHallway.Managers;
using Experiments.Global.Managers;
using UnityEngine;

namespace Experiments.EndlessHallway.Gameplay
{
    public class Token : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                GameManager.Instance.CollectToken();
                FXManager.Instance.SpawnFX("Coin Collect", transform.position);
                Destroy(transform.parent.gameObject);
            }
        }
    }
}