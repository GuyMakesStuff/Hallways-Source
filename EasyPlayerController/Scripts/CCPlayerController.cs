using Experiments.EndlessHallway.Managers;
using Experiments.Global.Audio;
using UnityEngine;

namespace EasyPlayerController
{
    [RequireComponent(typeof(CharacterController))]
    public class CCPlayerController : PlayerController
    {
        CharacterController CC;
        Vector3 PrevVelocity;
        float Damp;
        bool IsDamping = false;
        bool IsLanded = true;
        bool IsJumped = false;

        public override void MoveComponentInit()
        {
            base.MoveComponentInit();
            CC = GetComponent<CharacterController>();
        }

        public override void RequestMove(Vector3 MoveVect, Vector3 GravVect)
        {
            base.RequestMove(MoveVect, GravVect);
            if(CanMove)
            {
                CC.Move(MoveVect * Time.deltaTime);
                CC.Move(GravVect * Time.deltaTime);
                PrevVelocity = MoveVect + GravVect;

                bool PlayMoveSFX = MoveVect.magnitude > 0.1f && IsGrounded;
                SoundEffectBehaviour behaviour = (PlayMoveSFX) ? SoundEffectBehaviour.Play : SoundEffectBehaviour.Stop;
                string MoveSFXName = (IsSprinting) ? "Running" : "Walking";
                string OtherMoveSFXName = (!IsSprinting) ? "Running" : "Walking";
                AudioManager.Instance.InteractWithSFXOneShot(MoveSFXName, behaviour);
                AudioManager.Instance.InteractWithSFXOneShot(OtherMoveSFXName, SoundEffectBehaviour.Stop);
            }
            else
            {
                AudioManager.Instance.InteractWithSFXOneShot("Walking", SoundEffectBehaviour.Stop);
                AudioManager.Instance.InteractWithSFXOneShot("Running", SoundEffectBehaviour.Stop);
            }

            if(IsDamping)
            {
                Damp += Time.deltaTime;
                CC.Move(Vector3.Lerp(PrevVelocity, Vector3.zero, Damp) * Time.deltaTime);

                if(Damp >= 1f)
                {
                    IsDamping = false;
                    Damp = 0f;
                }
            }
        }

        public void DampVelocity()
        {
            IsDamping = true;
        }

        public override void OnGrounded()
        {
            base.OnGrounded();
            IsJumped = false;
            if(!IsLanded)
            {
                IsLanded = true;
                if(GameManager.Instance.GameState == GameManager.GameStates.Playing) { AudioManager.Instance.InteractWithSFX("Land", SoundEffectBehaviour.Play); }
            }
        }
        public override void OnNotGrounded()
        {
            base.OnNotGrounded();
            IsLanded = false;
            if(GravVel.y > 0f && !IsJumped)
            {
                IsJumped = true;
                if(GameManager.Instance.GameState == GameManager.GameStates.Playing) { AudioManager.Instance.InteractWithSFX("Jump", SoundEffectBehaviour.Play); }
            }
        }
    }
}