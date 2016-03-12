using System;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
    public class DenizenMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 8f;
        public float GroundDamping = 20f;
        public float JumpHeight = 3f;

        private float _normalizedHorizontalSpeed = 0;

        private DenizenController _controller;
        private Animator _animator;
        private RaycastHit2D _lastControllerColliderHit;
        private Vector3 _velocity;
        public event Action<RaycastHit2D> onControllerCollidedEvent;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<DenizenController>();
        }

        void Update()
        {
            HandleMovementInputs();

            // apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
            if (_controller.IsGrounded)
                _velocity.x = Mathf.Lerp(_velocity.x, _normalizedHorizontalSpeed * RunSpeed, Time.deltaTime * GroundDamping);

            _velocity.y += Gravity * Time.deltaTime;
            _controller._movement.Move(_velocity * Time.deltaTime);

            // send off the collision events if we have a listener
            if (onControllerCollidedEvent != null)
            {
                foreach (RaycastHit2D hit in _controller.RaycastHitsThisFrame)
                    onControllerCollidedEvent(hit);
            }

            _velocity = _controller.Velocity;

            _controller.HeatIce();
        }

        bool GoRight = true;
        float time = 3f;

        private void HandleMovementInputs()
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                GoRight = !GoRight;
            }
            if (_controller.IsGrounded)
                _velocity.y = 0;

            if (GoRight)
            {
                _normalizedHorizontalSpeed = 1;
                if (GetDirectionFacing() == DirectionFacing.Left)
                {
                    FlipSprite();
                    time = 3f;
                }
            }
            else if (GoRight == false)
            {
                _normalizedHorizontalSpeed = -1;
                if (GetDirectionFacing() == DirectionFacing.Right)
                {
                    time = 3f;
                    FlipSprite();
                }
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
            }
            SetAnimationWhenGrounded();
        }

        private void FlipSprite()
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }

        private void SetAnimationWhenGrounded()
        {
            if (_controller.IsGrounded && _normalizedHorizontalSpeed == 0)
                _animator.Play(Animator.StringToHash("Idle"));
            else
                _animator.Play(Animator.StringToHash("Run"));
        }

        private enum DirectionFacing
        {
            Right,
            Left
        }

        private DirectionFacing GetDirectionFacing()
        {
            DirectionFacing directionFacing;
            if (transform.localScale.x > 0f)
                directionFacing = DirectionFacing.Right;
            else
                directionFacing = DirectionFacing.Left;

            return directionFacing;
        }
    }
}