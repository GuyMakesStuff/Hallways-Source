using System.Collections;
using Experiments.EndlessHallway.Managers;
using UnityEngine;

namespace Experiments.EndlessHallway.Visuals
{
    public class MenuCamera : MonoBehaviour
    {
        public float RotSpeed;
        public float SmoothSpeed;
        float Rot;
        [HideInInspector]
        public bool IsTestingMouse;
        float XRig;
        float YRig;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        void Update()
        {
            if(GameManager.Instance.GameState == GameManager.GameStates.Menu)
            {
                if(IsTestingMouse)
                {
                    XRig += GameManager.Instance.playerInputReciever.MouseX * GameManager.Instance.Player.Sens * Time.deltaTime;
                    YRig -= GameManager.Instance.playerInputReciever.MouseY * GameManager.Instance.Player.Sens * Time.deltaTime;
                    transform.rotation = Quaternion.Euler(YRig, XRig, 0f);
                }
                else
                {
                    XRig = Rot;
                    YRig = 0f;
                    Rot += RotSpeed * Time.deltaTime;
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(!IsTestingMouse)
            {
                Quaternion TargetRot = Quaternion.Euler(0f, Rot, 0f);
                Quaternion SmoothedRot = Quaternion.Lerp(transform.rotation, TargetRot, SmoothSpeed * Time.deltaTime);
                transform.rotation = SmoothedRot;
            }
        }
    }
}