﻿using System;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerMotor : MonoBehaviour
    {
        public float Gravity = -25f;
        public float RunSpeed = 8f;
        public float GroundDamping = 20f;
        public float JumpHeight = 3f;

        private float _normalizedHorizontalSpeed = 0;

        private PlayerController _controller;
        private Animator _animator;
        private RaycastHit2D _lastControllerColliderHit;
        private Vector3 _velocity;
        public event Action<RaycastHit2D> onControllerCollidedEvent;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<PlayerController>();

            onControllerCollidedEvent += onControllerCollider;
            _controller.onTriggerEnterEvent += onTriggerEnterEvent;
            _controller.onTriggerExitEvent += onTriggerExitEvent;
        }

        void onControllerCollider(RaycastHit2D hit)
        {
            if (hit.normal.y == 1f)
            return;

            //Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
        }


        void onTriggerEnterEvent(Collider2D col)
        {
            Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
        }


        void onTriggerExitEvent(Collider2D col)
        {
            Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
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

        private void HandleMovementInputs()
        {
            if (_controller.IsGrounded)
                _velocity.y = 0;

            if (Input.GetKey(KeyCode.RightArrow))
            {
                _normalizedHorizontalSpeed = 1;
                if (GetDirectionFacing() == DirectionFacing.Left)
                    FlipSprite();
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                _normalizedHorizontalSpeed = -1;
                if (GetDirectionFacing() == DirectionFacing.Right)
                    FlipSprite();
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
            }
            SetAnimationWhenGrounded();

            if (_controller.IsGrounded && Input.GetKeyDown(KeyCode.UpArrow))
            {
                _velocity.y = Mathf.Sqrt(2f * JumpHeight * -Gravity);
                _animator.Play(Animator.StringToHash("Jump"));
            }

            if (_controller.IsGrounded && Input.GetKeyDown(KeyCode.DownArrow))
            {
                _controller.CreatePilotedLight();
            }
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