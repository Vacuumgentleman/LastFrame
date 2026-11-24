using UnityEngine;

namespace Senses2026
{
    [AddComponentMenu("CatBorg Studio/Senses/ B: Bonus Content/Player/BC 1: Player Stats", 1)]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Movement")]
        public float WalkSpeed = 2.0f;
        public float SprintSpeed = 5.335f;
        public float CrouchSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;
        public float RotationSmoothTime = 0.12f;

        [Header("Jump & Fall")]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Crouch")]
        public float CrouchHeight = 1.2f;
        public Vector3 CrouchCenter = new Vector3(0f, 0.5f, 0f);
        public float CrouchTransitionSpeed = 7f;

        [Header("Audio")]
        public AudioClip LandingSound;
        public AudioClip[] FootstepSounds;
        [Range(0f, 1f)] public float FootstepVolume = 0.5f;
    }
}
