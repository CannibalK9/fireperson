using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PilotedLightMotor : MonoBehaviour
    {
        private PilotedLightController _controller;
        private Vector3 _velocity;
        private float _normalizedHorizontalSpeed = 0;
        private float _normalizedVerticalSpeed = 0;
        public float FlySpeed = 2f;
        public float AirDamping = 500f;

        void Awake()
        {
            _controller = GetComponent<PilotedLightController>();
        }

        void Update()
        {
            HandleMovementInputs();

            _velocity.x = Mathf.Lerp(_velocity.x, _normalizedHorizontalSpeed * FlySpeed, Time.deltaTime * AirDamping);
            _velocity.y = Mathf.Lerp(_velocity.y, _normalizedVerticalSpeed * FlySpeed, Time.deltaTime * AirDamping);

            _controller._movement.Move(_velocity * Time.deltaTime);

            _velocity = _controller.Velocity;

            _controller.HeatIce();
        }

        private void HandleMovementInputs()
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                _normalizedHorizontalSpeed = 1;
            }
            else if (Input.GetAxis("Mouse X") < 0)
            {
                _normalizedHorizontalSpeed = -1;
            }

            if (Input.GetAxis("Mouse Y") > 0)
            {
                _normalizedVerticalSpeed = 1;
            }
            else if (Input.GetAxis("Mouse Y") < 0)
            {
                _normalizedVerticalSpeed = -1;
            }
        }
    }
}
