using UnityEngine;

namespace Assets.Scripts.Player
{
	public class PilotedLightMotor : MonoBehaviour
	{
		private PilotedLightController _controller;
		private Vector3 _velocity;
		private float _normalizedHorizontalSpeed;
		private float _normalizedVerticalSpeed;
		private const float _acceleration = 0.05f;
		private float _timeToWake = 1f;

		public float FlySpeed = 2f;
		public float AirDamping = 500f;
		public float Gravity = 0f;

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

            if (IsReadyToMove)
                HandleActions();

			if (IsReadyToMove && _controller.IsMovementOverridden == false)
			{
				HandleMovementInputs();

				float appliedGravity = _controller.NoGravity == true ? 0 : Gravity;

				_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * FlySpeed, ref _velocity.x, Time.deltaTime * AirDamping);
				_velocity.y = Mathf.SmoothDamp(_velocity.y, _normalizedVerticalSpeed * FlySpeed - appliedGravity, ref _velocity.y, Time.deltaTime * AirDamping);

				_controller.Movement.Move(_velocity * Time.deltaTime);
				_velocity = _controller.Velocity;
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
				_normalizedVerticalSpeed = 0;
				_velocity = Vector3.zero;
			}
		}

        private float _lightPressTime;

        private void HandleActions()
        {
			if (ChannelingHandler.ChannelingSet == false)
			{
				ChannelingHandler.StopBreaking();
				Destroy(transform.root.gameObject);
			}

            if (KeyBindings.GetKey(Control.Light))
            {
				ChannelingHandler.BreakChannel();
            }

            if (KeyBindings.GetKeyUp(Control.Light))
            {
                ChannelingHandler.StopBreaking();
				_controller.Burst();
                //burst
            }
        }

		private void HandleMovementInputs()
		{
            if (_controller.OnPoint())
			{
				float x = Input.GetAxis("Mouse X");
				float y = Input.GetAxis("Mouse Y");

				if (_controller.SwitchFireplace(new Vector2(x, y)) || _controller.Fireplace.IsAccessible == false)
					return;
			}

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
