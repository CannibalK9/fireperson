using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PilotedLightMotor : MonoBehaviour
    {
        private PilotedLightController _controller;
        private Vector3 _velocity;
        private float _normalizedHorizontalSpeed = 0;
        private float _normalizedVerticalSpeed = 0;
        private float _acceleration = 0.05f;
        private float _timeToWake = 1f;

        public float FlySpeed = 2f;
        public float AirDamping = 500f;

        public bool IsReadyToMove { get; set; }

        void Awake()
        {
            _controller = GetComponent<PilotedLightController>();
        }

        void Update()
        {
            _timeToWake -= Time.deltaTime;
            if (_timeToWake < 0)
                IsReadyToMove = true;

            if (IsReadyToMove && _controller.IsMovementOverridden == false)
            {
                HandleMovementInputs();

                _velocity.x = Mathf.Lerp(_velocity.x, _normalizedHorizontalSpeed * FlySpeed, Time.deltaTime * AirDamping);
                _velocity.y = Mathf.Lerp(_velocity.y, _normalizedVerticalSpeed * FlySpeed, Time.deltaTime * AirDamping);

                _controller._movement.Move(_velocity * Time.deltaTime);
                _velocity = _controller.Velocity;
            }
            else
            {
                _normalizedHorizontalSpeed = 0;
                _normalizedVerticalSpeed = 0;
                _velocity = Vector3.zero;
            }
            _controller.HeatIce();
        }

        private void HandleMovementInputs()
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                if (_normalizedHorizontalSpeed < 1)
                    _normalizedHorizontalSpeed += _acceleration;
            }
            else if (Input.GetAxis("Mouse X") < 0)
            {
                if (_normalizedHorizontalSpeed > -1)
                    _normalizedHorizontalSpeed -= _acceleration;
            }

            if (Input.GetAxis("Mouse Y") > 0)
            {
                if (_normalizedVerticalSpeed < 1)
                    _normalizedVerticalSpeed += _acceleration;
            }
            else if (Input.GetAxis("Mouse Y") < 0)
            {
                if (_normalizedVerticalSpeed > -1)
                    _normalizedVerticalSpeed -= _acceleration;
            }
        }
    }
}
