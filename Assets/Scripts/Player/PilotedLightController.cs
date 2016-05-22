using System.Collections.Generic;
using Assets.Scripts.Heat;
using Assets.Scripts.Movement;
using UnityEngine;
using Assets.Scripts.Denizens;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Config;

namespace Assets.Scripts.Player
{
	public class PilotedLightController : MonoBehaviour, IVariableHeater, IController
	{
		public float DurationInSeconds;
		private float _remainingDurationInSeconds;
		private const float _minimumScale = 1f;
		private Vector2 _initialScale;

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

        [Range(1, 10)]
        public float _heatRayDistance;
        public float HeatRayDistance { get { return _heatRayDistance; } }

        [Range(1, 10)]
        public float _heatIntensity;
        public float HeatIntensity { get { return _heatIntensity; } }

        public LayerMask _triggerMask = 0;
		public LayerMask _platformMask = 0;
		public LayerMask PlatformMask { get { return _platformMask; } }

		public MovementHandler Movement;
		public BoxCollider2D BoxCollider { get; set; }
		public CollisionState CollisionState { get; set; }
		public Transform Transform { get; set; }
		public Vector3 Velocity { get; set; }

		public List<RaycastHit2D> RaycastHitsThisFrame { get; set; }
		public float VerticalDistanceBetweenRays { get; set; }
		public float HorizontalDistanceBetweenRays { get; set; }
		public FirePlace Fireplace { get; set; }

		private SpriteRenderer _renderer;

		void Awake()
		{
			IsMovementOverridden = false;
			_initialScale = transform.localScale;

			RaycastHitsThisFrame = new List<RaycastHit2D>(2);
			CollisionState = new CollisionState();
			Movement = new MovementHandler(this);
			_heatHandler = new HeatHandler(this);

			BoxCollider = GetComponent<BoxCollider2D>();
			Transform = GetComponent<Transform>();
			_renderer = GetComponent<SpriteRenderer>();

			IgnoreCollisionLayersOutsideTriggerMask();
		}

        void Start()
        {
            _heatRayDistance = PlayerPrefs.GetFloat(Variable.PlRange.ToString());
            _heatIntensity = PlayerPrefs.GetFloat(Variable.PlIntensity.ToString());
            DurationInSeconds = PlayerPrefs.GetFloat(Variable.PlDuration.ToString());

			_remainingDurationInSeconds = DurationInSeconds;
        }

        private void IgnoreCollisionLayersOutsideTriggerMask()
		{
			for (var i = 0; i < 32; i++)
			{
				if ((_triggerMask.value & 1 << i) == 0)
					Physics2D.IgnoreLayerCollision(gameObject.layer, i);
			}
		}

		public bool NoGravity;

		void Update()
		{
			if (IsMovementOverridden)
			{
				MoveTowardsPoint();
				NoGravity = true;
			}
			else if (OnPoint() == false)
			{
				_renderer.enabled = true;

				if (NoGravity)
				{
					NoGravity = false;
					Fireplace.IsLit = false;
				}
				DecreaseLifeSpan();
				DecreaseScale();

				if (_remainingDurationInSeconds <= 0)
				{
					DestroyObject(gameObject);
				}
			}
		}

		public Collider2D CollidingPoint;

		private void MoveTowardsPoint()
		{
			Movement.Move((CollidingPoint.transform.position - transform.position) * 0.2f);
			if (OnPoint())
			{
				transform.localScale = _initialScale;
				_remainingDurationInSeconds = DurationInSeconds;
				IsMovementOverridden = false;

				_renderer.enabled = false;

				Fireplace = CollidingPoint.gameObject.GetComponent<FirePlace>();
				Fireplace.IsLit = true;
			}
		}

		public bool OnPoint()
		{
			return CollidingPoint != null && CollidingPoint.transform.position == transform.position;
		}

		private void DecreaseLifeSpan()
		{
			_remainingDurationInSeconds -= Time.deltaTime;
		}

		private void DecreaseScale()
		{
			Vector2 newScale = Vector2.one * (_remainingDurationInSeconds + _minimumScale);
			if (newScale.x < _initialScale.x)
			{
				transform.localScale = newScale;
				RecalculateDistanceBetweenRays();
			}
		}

		public void HeatIce()
		{
			_heatHandler.OneCircleHeat();
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
					CollidingPoint = col;
					IsMovementOverridden = true;
				}
			}
		}
	}
}
