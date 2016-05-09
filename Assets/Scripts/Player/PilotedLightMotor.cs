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
            _controller.Movement.MoveWithBuilding();
            _controller.HeatIce();
		}

		private void HandleMovementInputs()
		{
			if (_controller.OnPoint())
			{
				if (Input.GetAxis("Mouse X") > 0)
				{
					if (_controller.Fireplace.Right != null)
					{
						_controller.Fireplace.IsLit = false;
						_controller.CollidingPoint = _controller.Fireplace.Right.GetComponent<Collider2D>();
						_controller.IsMovementOverridden = true;
						return;
					}
				}

				if (Input.GetAxis("Mouse X") < 0)
				{
					if (_controller.Fireplace.Left != null)
					{
						_controller.Fireplace.IsLit = false;
						_controller.CollidingPoint = _controller.Fireplace.Left.GetComponent<Collider2D>();
						_controller.IsMovementOverridden = true;
						return;
					}
				}

				if (Input.GetAxis("Mouse Y") > 0)
				{
					if (_controller.Fireplace.Up != null)
					{
						_controller.Fireplace.IsLit = false;
						_controller.CollidingPoint = _controller.Fireplace.Up.GetComponent<Collider2D>();
						_controller.IsMovementOverridden = true;
						return;
					}
				}

				if (Input.GetAxis("Mouse Y") < 0)
				{
					if (_controller.Fireplace.Down != null)
					{
						_controller.Fireplace.IsLit = false;
						_controller.CollidingPoint = _controller.Fireplace.Down.GetComponent<Collider2D>();
						_controller.IsMovementOverridden = true;
						return;
					}
				}

				if (_controller.Fireplace.IsAccessible == false)
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
