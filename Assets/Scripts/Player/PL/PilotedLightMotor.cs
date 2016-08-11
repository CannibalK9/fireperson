using Assets.Scripts.Helpers;
using Assets.Scripts.Interactable;
using Assets.Scripts.Movement;
using Assets.Scripts.Player.Config;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player.PL
{
	public class PilotedLightMotor : MonoBehaviour, IMotor
	{
		private PilotedLightController _controller;
		private Vector3 _velocity;
		private float _normalizedHorizontalSpeed;
		private float _normalizedVerticalSpeed;
		private const float _acceleration = 0.05f;
		private float _timeToWake = 1f;
		private SpriteRenderer _renderer;
		private bool _noGravity;
		private FirePlace _fireplace;
		private MovementHandler _movement;

		public float FlySpeed = 2f;
		public float AirDamping = 500f;
		public float Gravity = 0f;
		public Collider2D Collider { get; set; }
		public float SlopeLimit { get { return 0; } }
		public MovementState MovementState { get; set; }
		public Transform Transform { get; set; }
		public Rigidbody2D Rigidbody { get; set; }
		private bool _isFireplaceActive;

		void Awake()
		{
			Transform = transform;
			Collider = GetComponent<CircleCollider2D>();
			MovementState = new MovementState();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();

			_movement = new MovementHandler(this);
			_renderer = GetComponent<SpriteRenderer>();
			_controller = GetComponent<PilotedLightController>();
		}

		void FixedUpdate()
		{
			Rigidbody.isKinematic = false;

			if (MovementState.MovementOverridden == false)
				HandleMovementInputs();

			if (MovementState.MovementOverridden == false)
			{ 
				float appliedGravity = _noGravity ? 0 : Gravity;

				_velocity.x = Mathf.SmoothDamp(_velocity.x, _normalizedHorizontalSpeed * FlySpeed, ref _velocity.x, Time.deltaTime * AirDamping);
				_velocity.y = Mathf.SmoothDamp(_velocity.y, _normalizedVerticalSpeed * FlySpeed - appliedGravity, ref _velocity.y, Time.deltaTime * AirDamping);

				Transform.Translate(_velocity, Space.World);
			}
			else
			{
				_normalizedHorizontalSpeed = 0;
				_normalizedVerticalSpeed = 0;
			}

			if (_fireplace != null)
			{
				if (OnPoint())
				{
					if (_isFireplaceActive)
					{
						ActivatePoint();
						_isFireplaceActive = false;
					}
					MoveTowardsPoint();
					MovementState.MovementOverridden = false;
					if (_fireplace.IsLit)
					{
						_controller.HeatHandler.SetColliderSizes(_fireplace.HeatIntensity);
						_controller.HeatHandler.HeatMessage = new Heat.HeatMessage(_fireplace.HeatIntensity);
					}
					else
					{
						_controller.HeatHandler.SetColliderSizes(0);
						_controller.HeatHandler.HeatMessage = new Heat.HeatMessage(0);
					}
				}
				else if (OnPoint() == false && MovementState.MovementOverridden == false)
				{
					LeaveSpot();
				}
				else
					MoveTowardsPoint();
			}
			else if (_controller.IsWithinPlayerDistance() == false)
			{
				DestroyObject(gameObject);
			}

			_timeToWake -= Time.deltaTime;
			if (_timeToWake > 0)
				return;

            HandleActions();
		}

		private void MoveTowardsPoint()
		{
			_noGravity = true;
			_movement.MoveLinearly(0.2f);
		}

		private float _lightPressTime;

		public PilotedLightMotor(MovementHandler movement)
		{
			_movement = movement;
		}

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
				Burst();
            }
        }

		private void HandleMovementInputs()
		{
            if (OnPoint())
			{
				float x = Input.GetAxis("Mouse X");
				float y = Input.GetAxis("Mouse Y");

				if (SwitchFireplace(new Vector2(x, y)) || _fireplace.IsAccessible == false)
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

		public bool SwitchFireplace(Vector2 direction)
		{
			foreach (FirePlace fireplace in _fireplace.GetConnectedFireplaces())
			{
				if (fireplace != null)
				{
					if (Vector2.Angle(direction, fireplace.transform.position - transform.position) < 10f)
					{
						FirePlace switchedFireplace = fireplace is Pipe
							? fireplace.GetConnectedFireplaces().First(fp => fp != _fireplace)
							: switchedFireplace = fireplace;

						_fireplace = switchedFireplace;

						MovementState.SetPivot(switchedFireplace.GetComponent<Collider2D>(), ColliderPoint.Centre, ColliderPoint.Centre);
						MovementState.MovementOverridden = true;
						return true;
					}
				}
			}
			return false;
		}

		public void ActivatePoint()
		{
			_renderer.enabled = false;
			if (_fireplace.IsHeatSource)
				_fireplace.PlEnter();
		}

		public void LeaveSpot()
		{
			_fireplace.PlLeave();
			_renderer.enabled = true;
			_noGravity = false;
			_fireplace = null;
		}

		public bool OnPoint()
		{
			return _fireplace != null && Vector2.Distance(_fireplace.transform.position, transform.position) < 0.1f;
		}

		public void Burst()
		{
			if (_fireplace != null)
			{
				_fireplace.Burst();
			}
			else
			{
				_controller.FirstUpdate = true;
			}
		}

		void OnTriggerEnter2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
			{
				if (col.GetComponent<FirePlace>().IsAccessible)
				{
					_fireplace = col.GetComponent<FirePlace>();
					MovementState.SetPivot(col, ColliderPoint.Centre, ColliderPoint.Centre);
					MovementState.MovementOverridden = true;
					_isFireplaceActive = true;
				}
			}
		}

		void OnTriggerExit2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
			{
				col.GetComponent<FirePlace>().PlLeave();
			}
		}

		void OnDestroy()
		{
			DestroyObject(MovementState.Pivot);
			ChannelingHandler.ChannelingSet = false;
			ChannelingHandler.BreakChannel();
		}
	}
}
