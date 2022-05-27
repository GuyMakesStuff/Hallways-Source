using System.Collections;
using Experiments.EndlessHallway.Managers;
using UnityEngine;

namespace EasyPlayerController
{
    public abstract class PlayerController : MonoBehaviour
    {
        [HideInInspector]
        public PlayerInputReciever InputReciever;

        [Header("Camera")]
        public Transform Head;
        public float Sens = 100f;
        public float HeadHeight = 0.5f;
        Transform Cam;
        float XRig;

        [Header("Movement")]
        public bool CanMove = true;
        public float BaseSpeed = 6f;
        public float SprintSpeedMult = 2f;
        float Speed;
        protected bool IsSprinting;
        public Transform GroundChecker;
        public LayerMask GroundLayer;
        public float Gravity = -9.81f;
        public float JumpHeight = 10f;
        protected bool IsGrounded;
        bool IsHittingCelling;
        protected Vector3 GravVel;

        void Awake()
        {
            PlayerInputReciever playerInputReciever = FindObjectOfType<PlayerInputReciever>();
            if(playerInputReciever == null)
            {
                InputReciever = new GameObject("Player Input Manager").AddComponent<PlayerInputReciever>();
            }
            else
            {
                InputReciever = playerInputReciever;
            }

            Cam = Camera.main.transform;
            if(Head == null)
            {
                Head = new GameObject("Head").transform;
            }
            Head.SetParent(transform);

            MoveComponentInit();
        }
        public virtual void MoveComponentInit()
        {
            Debug.Log("Initalizing Movement Component");
        }

        void Update()
        {
            UpdateCam();
            UpdateMove();
        }
        void UpdateCam()
        {
            Head.position = transform.position + new Vector3(0, HeadHeight, 0);
            Cam.position = Head.position;
            Cam.rotation = Head.rotation;

            if(CanMove)
            {
                XRig -= InputReciever.MouseY * Sens * Time.deltaTime;
                XRig = Mathf.Clamp(XRig, -90f, 90f);

                transform.Rotate(Vector3.up, InputReciever.MouseX * Sens * Time.deltaTime);
            }
            
            if(GameManager.IsInstanced)
            {
                if(GameManager.Instance.GameState == GameManager.GameStates.Menu || GameManager.Instance.GameState == GameManager.GameStates.Intro)
                {
                    XRig = 0f;
                }
            }

            Head.localRotation = Quaternion.Euler(XRig, 0, 0);
        }
        void UpdateMove()
        {
            IsGrounded = Physics.CheckSphere(GroundChecker.position, 0.48f, GroundLayer);
            IsHittingCelling = Physics.Raycast(transform.position, Vector3.up, 1.1f);

            IsSprinting = InputReciever.SprintKeyHold;
            Speed = (!IsSprinting) ? BaseSpeed : BaseSpeed * SprintSpeedMult;

            if (GravVel.y > 0f && IsHittingCelling)
            {
                GravVel.y = -0.1f;
            }

            Vector3 Movement = (InputReciever.Hori * transform.right) + (InputReciever.Vert * transform.forward);
            RequestMove(Movement * Speed, GravVel);

            if(IsGrounded)
            {
                OnGrounded();
            }
            else
            {
                OnNotGrounded();
            }
        }

        public virtual void RequestMove(Vector3 MoveVect, Vector3 GravVect)
        {
            // Debug.Log("Movement Vector-" + MoveVect.ToString() + ".Gravity Vector-" + GravVect.ToString() + ".Total Velocity-" + (MoveVect + GravVect).ToString());
        }
        public virtual void OnGrounded()
        {
            if(GravVel.y < 0f)
            {
                GravVel.y = -2f;
            }

            if(InputReciever.RequestingJump)
            {
                GravVel.y = Mathf.Sqrt(JumpHeight * -2 * Gravity);
            }
        }
        public virtual void OnNotGrounded()
        {
            GravVel.y += Gravity * Time.deltaTime;
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(GroundChecker.position, 0.48f);
        }
    }
}