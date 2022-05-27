using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyPlayerController
{
    public class PlayerInputReciever : MonoBehaviour
    {
        [Header("Mouse")]
        public string MouseXAxis = "Mouse X";
        [HideInInspector]
        public float MouseX;
        public string MouseYAxis = "Mouse Y";
        [HideInInspector]
        public float MouseY;
        public bool LockMouse;
        public bool InvertMouse;

        [Header("Movement")]
        public bool Smooth;
        public string HoriAxis = "Horizontal";
        [HideInInspector]
        public float Hori;
        public string VertAxis = "Vertical";
        [HideInInspector]
        public float Vert;

        [Header("Keybinds")]
        public KeyCode JumpKey = KeyCode.Space;
        [HideInInspector]
        public bool RequestingJump;
        public KeyCode SprintKey = KeyCode.LeftShift;
        [HideInInspector]
        public bool SprintKeyHold;

        // Update is called once per frame
        void Update()
        {
            Cursor.lockState = (LockMouse) ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !LockMouse;

            MouseX = (InvertMouse) ? -Input.GetAxis(MouseXAxis) : Input.GetAxis(MouseXAxis);
            MouseY = (InvertMouse) ? -Input.GetAxis(MouseYAxis) : Input.GetAxis(MouseYAxis);

            Hori = (!Smooth) ? Input.GetAxisRaw(HoriAxis) : Input.GetAxis(HoriAxis);
            Vert = (!Smooth) ? Input.GetAxisRaw(VertAxis) : Input.GetAxis(VertAxis);

            RequestingJump = Input.GetKeyDown(JumpKey);
            SprintKeyHold = Input.GetKey(SprintKey);
        }
    }
}