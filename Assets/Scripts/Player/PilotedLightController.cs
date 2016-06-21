using System.Collections.Generic;
using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Movement;
using Assets.Scripts.Player.Config;
using UnityEngine;
using Assets.Scripts.Interactable;

namespace Assets.Scripts.Player
{
	public class PilotedLightController : MonoBehaviour, IVariableHeater, IController
	{
		public float DistanceFromPlayer
		{
			get; set;
		}

		private HeatHandler _heatHandler;

		[Range(2, 20)]
		public int _totalHorizontalRays = 4;
		public int TotalHorizontalRays { get { return _totalHorizontalRays; } }

		[Range(2, 20)]
		public int _totalVerticalRays = 4;
		public int TotalVerticalRays { get { return _totalVerticalRays; } }

		[SerializeField]
		[Range(0.001f, 0.3f)]
		private float _skinWidth = 0.02f;
		public float SkinWidth
		{
			get { return _skinWidth; }
			set
			{
				_skinWidth = value;
				RecalculateDistanceBetweenRays();
			}
		}

		[Range(0f, 90f)]
		public float _slopeLimit = 30f;
		public float SlopeLimit { get { return _slopeLimit; } }

		public bool IsMovementOverridden { get; set; }

		public AnimationCurve SlopeSpeedMultiplier
		{
			get
			{
				return new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
			}
		}

		public float Stability;
		public float Intensity;
		public float Control;

		public float HeatIntensity { get { return Intensity; } }
		public float HeatRayDistance { get { return Stability; } }

		private LayerMask _platformMask = Layers.Platforms;
		public LayerMask PlatformMask { get { return _platformMask; } }

		public MovementHandler Movement;
		public BoxCollider2D BoxCollider { get; set; }
		public Collider2D Collider { get; set; }
		public CollisionState CollisionState { get; set; }
		public Transform Transform { get; set; }
		public Vector3 Velocity { get; set; }

		public List<RaycastHit2D> RaycastHitsThisFrame { get; set; }
		public float VerticalDistanceBetweenRays { get; set; }
		public float HorizontalDistanceBetweenRays { get; set; }
		public FirePlace Fireplace { get; set; }

		private SpriteRenderer _renderer;
        public PlayerController Player;

		void Awake()
		{
			IsMovementOverridden = false;

			RaycastHitsThisFrame = new List<RaycastHit2D>(2);
			CollisionState = new CollisionState();
			Movement = new MovementHandler(this);
			_heatHandler = new HeatHandler(this);

			Collider = GetComponent<BoxCollider2D>();
			BoxCollider = GetComponent<BoxCollider2D>();
			Transform = GetComponent<Transform>();
			_renderer = GetComponent<SpriteRenderer>();
		}

		void Start()
		{
			Stability = ChannelingHandler.PlStability();
			Intensity = ChannelingHandler.PlIntensity();
			Control = ChannelingHandler.PlControl();
		}

		public bool NoGravity;
        private bool _firstUpdate = true;
		private float _emberEffectTime;

		void Update()
		{
			if (IsMovementOverridden)
			{
				RemovePlatformMask();
				MoveTowardsPoint();
				NoGravity = true;
				if (OnPoint())
				{
					ReapplyPlatformMask();
					IsMovementOverridden = false;
					ActivatePoint();
				}
			}
			else if (OnPoint())
            {
                MoveTowardsPoint();
            }
            else if (OnPoint() == false)
			{
				DeactivatePoint();

				if (Vector2.Distance(Player.transform.position, transform.position) > DistanceFromPlayer * 5)
				{
					DestroyObject(gameObject);
				}
			}

            var effect = EmberEffect.None;
            
			_emberEffectTime -= Time.deltaTime;

			if (_emberEffectTime < 0)
			{
				effect = EmberEffect.Light;
				_emberEffectTime = Random.Range(10, 60);
			}

			if (_firstUpdate)
			{
				effect = EmberEffect.Strong;
				_firstUpdate = false;
			}

            HeatIce(effect);
		}

		private Collider2D _collidingPoint;

		private void MoveTowardsPoint()
		{
			Movement.Move((_collidingPoint.transform.position - transform.position) * 0.2f);
		}

		private void ActivatePoint()
		{
			IsMovementOverridden = false;
			_renderer.enabled = false;
			Fireplace = _collidingPoint.gameObject.GetComponent<FirePlace>();
			Fireplace.PlEnter(this);
		}

		private void DeactivatePoint()
		{
			if (Fireplace != null)
			{
				_renderer.enabled = true;
				Fireplace.PlLeave();
				NoGravity = false;
				_collidingPoint = null;
				Fireplace = null;
			}
		}

		public bool OnPoint()
		{
			return _collidingPoint != null && Vector2.Distance(_collidingPoint.transform.position, transform.position) < 0.1f;
		}

		public bool SwitchFireplace(Vector2 direction)
		{
			foreach (FirePlace fireplace in Fireplace.GetConnectedFireplaces())
			{
				if (fireplace != null)
				{
					if (Vector2.Angle(direction, fireplace.transform.position - transform.position) < 10f)
					{
						Fireplace.PlLeave();
						_collidingPoint = fireplace.GetComponent<Collider2D>();
						Fireplace = null;
						IsMovementOverridden = true;
						return true;
					}
				}
			}
			return false;
		}

		public void Burst()
		{
			if (Fireplace != null)
			{
				Fireplace.Burst();
			}
			else
			{
				_firstUpdate = true;
			}
		}

		public void HeatIce(EmberEffect effect)
		{
			_heatHandler.OneCircleHeat(effect);
		}

		public void RemovePlatformMask()
		{
			_platformMask = 0;
		}

		public void ReapplyPlatformMask()
		{
			_platformMask = Layers.Platforms;
		}

		private void RecalculateDistanceBetweenRays()
		{
			var colliderUseableHeight = BoxCollider.size.y * Mathf.Abs(Transform.localScale.y) - (2f * _skinWidth);
			VerticalDistanceBetweenRays = colliderUseableHeight / (TotalHorizontalRays - 1);

			var colliderUseableWidth = BoxCollider.size.x * Mathf.Abs(Transform.localScale.x) - (2f * _skinWidth);
			HorizontalDistanceBetweenRays = colliderUseableWidth / (TotalVerticalRays - 1);
		}

		void OnTriggerEnter2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
			{
				if (col.GetComponent<FirePlace>().IsAccessible)
				{
					_collidingPoint = col;
					IsMovementOverridden = true;
				}
			}
		}
	}
}
