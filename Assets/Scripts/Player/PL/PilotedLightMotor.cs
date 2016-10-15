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
		private Renderer _renderer;
		private bool _noGravity;
		private FirePlace _fireplace;
		private MovementHandler _movement;
		private bool _isFireplaceActive;
		private ParticleSystem _particles;
		private float _timeClicked;
		private float _burstTime;
		private bool _fireplaceWasLit;

		public float FlySpeed = 2f;
		public float AirDamping = 500f;
		public float Gravity = 0f;

        public Collider2D Collider { get; set; }
		public float SlopeLimit { get { return 0; } }
		public MovementState MovementState { get; set; }
		public Transform Transform { get; set; }
		public Rigidbody2D Rigidbody { get; set; }
		public bool IsScouting { get; private set; }

		void Awake()
		{
			Transform = transform;
			Collider = GetComponent<CircleCollider2D>();
			MovementState = new MovementState();
			Rigidbody = Transform.GetComponent<Rigidbody2D>();

			_movement = new MovementHandler(this);
			_renderer = GetComponent<Renderer>();
			_controller = GetComponent<PilotedLightController>();
			_particles = GetComponentInChildren<ParticleSystem>();
		}

		void Update()
		{
			if (ChannelingHandler.ChannelingSet)
				HandleActions();
		}

		private void HandleActions()
		{
			if (KeyBindings.GetKeyDown(Controls.Light) && Pointer.IsPointerOverUIObject() == false)
			{
				float timeClicked = Time.realtimeSinceStartup;
				if (Mathf.Abs(timeClicked - _timeClicked) < 0.4f)
					Destroy(gameObject);
				else
					_timeClicked = timeClicked;
			}

			if (((AbilityState.IsActive(Ability.Flash) && _fireplace == null)
				|| (AbilityState.IsActive(Ability.Ignite) && _fireplace != null))
				&& IsScouting == false)
			{
				if (KeyBindings.GetKeyDown(Controls.Light) && Pointer.IsPointerOverUIObject() == false)
				{
					_burstTime += Time.deltaTime;
				}

				if (KeyBindings.GetKey(Controls.Light) && Pointer.IsPointerOverUIObject() == false)
				{
					if (_burstTime > 0)
					{
						float maxTime = 2f;
						_burstTime += Time.deltaTime;
						_burstTime = _burstTime > maxTime ? maxTime : _burstTime;
						_controller.DecreaseVariables(1 - _burstTime / maxTime);
					}
				}

				if (KeyBindings.GetKeyUp(Controls.Light))
				{
					_burstTime = 0;
					_controller.ResetVariables();
					Burst();
				}
			}
			else
			{
				_burstTime = 0;
			}
		}

		void FixedUpdate()
		{
			if (ChannelingHandler.ChannelingSet == false)
				return;

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

			GetComponentInChildren<CircleCollider2D>().enabled = IsScouting == false;

			if (_fireplace != null)
			{
				if (OnPoint())
				{
					MoveTowardsPoint();
					MovementState.MovementOverridden = false;

					if (_isFireplaceActive)
					{
						ActivatePoint();
						_isFireplaceActive = false;
					}

					if (_fireplace.IsLit && _fireplaceWasLit == false && IsScouting == false)
					{
						_controller.EnterFireplace(_fireplace);
						_fireplaceWasLit = true;
					}
					else if (_fireplace.IsLit == false && _fireplaceWasLit && IsScouting == false)
					{
						_controller.LeaveFireplace(_fireplace);
						_fireplaceWasLit = false;
					}
				}
				else if (OnPoint() == false && MovementState.MovementOverridden == false)
				{
					if (_fireplace.IsLit && IsScouting == false)
						_controller.LeaveFireplace(_fireplace);

					_fireplaceWasLit = false;
					LeaveSpot();
				}
				else
					MoveTowardsPoint();
			}
			else if (_controller.IsWithinPlayerDistance() == false)
			{
				if (AbilityState.IsActive(Ability.Scout) && _controller.IsWithinScoutingDistance())
				{
					if (IsScouting == false)
						_controller.DecreaseVariables(0);
					IsScouting = true;
				}
				else
					DestroyObject(gameObject);
			}
			else
			{
				if (IsScouting)
					_controller.ResetVariables();
				IsScouting = false;
			}
		}

		private void MoveTowardsPoint()
		{
			_noGravity = true;
			_movement.MoveLinearly(0.2f);
		}

		public PilotedLightMotor(MovementHandler movement)
		{
			_movement = movement;
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
			FirePlace switchedFireplace = GetSwitchedFireplace(_fireplace, direction);
			if (switchedFireplace != null)
			{
				_fireplace.PlLeave();
				_fireplace = switchedFireplace;
				MoveToFireplace();
				return true;
			}
			return false;
		}

		private FirePlace GetSwitchedFireplace(FirePlace currentFireplace, Vector2 direction)
		{
			foreach (FirePlace fireplace in currentFireplace.GetConnectedFireplaces())
			{
				if (Vector2.Angle(direction, fireplace.transform.position - transform.position) < 10f)
				{
					FirePlace switchedFireplace = fireplace is Pipe && fireplace.IsAccessible == false
						? fireplace.GetConnectedFireplaces().First(fp => fp != currentFireplace)
						: switchedFireplace = fireplace;

					if (switchedFireplace is Pipe && fireplace.IsAccessible == false)
					{
						switchedFireplace = switchedFireplace.GetConnectedFireplaces().First(fp => fp != fireplace);
					}

					return switchedFireplace;
				}
			}
			return null;
		}

		private void MoveToFireplace()
		{
			MovementState.SetPivot(_fireplace.GetComponent<Collider2D>(), ColliderPoint.Centre, ColliderPoint.Centre);
			MovementState.MovementOverridden = true;
			_isFireplaceActive = true;
		}

		public void ActivatePoint()
		{
			_renderer.enabled = false;
			_particles.Stop();
			if (IsScouting == false)
				_fireplace.PlEnter();
		}

		public void LeaveSpot()
		{
			_movement.IgnorePlatforms(false);
			_renderer.enabled = true;
			_particles.Play();
			_noGravity = false;

			if (IsScouting == false)
				_fireplace.PlLeave();

			_fireplaceWasLit = false;
			_fireplace = null;
		}

		public bool OnPoint()
		{
			return _fireplace != null && Vector2.Distance(_fireplace.transform.position, transform.position) < 0.1f;
		}

		public void Burst()
		{
			if (IsScouting)
				return;

			if (_fireplace != null && AbilityState.IsActive(Ability.Ignite))
			{
				_fireplace.Burst();
			}
			else if (AbilityState.IsActive(Ability.Flash))
			{
				_controller.Flash();
			}
		}

		void OnTriggerEnter2D(Collider2D col)
		{
			if (ShouldTrigger(col))
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

		private bool ShouldTrigger(Collider2D col)
		{
			return MovementState.MovementOverridden == false && col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot);
		}

		void OnDestroy()
		{
			DestroyObject(MovementState.Pivot);
			ChannelingHandler.BreakChannel();
		}
	}
}
