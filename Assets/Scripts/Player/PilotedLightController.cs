using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Movement;
using UnityEngine;
using Assets.Scripts.Interactable;

namespace Assets.Scripts.Player
{
	public class PilotedLightController : MonoBehaviour, IVariableHeater, IMotor
	{
		public float DistanceFromPlayer
		{
			get; set;
		}

		private HeatHandler _heatHandler;

		public float SlopeLimit { get { return 0; } }

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

		public MovementHandler Movement;
		public BoxCollider2D BoxCollider { get; set; }
		public Collider2D Collider { get; set; }
		public CollisionState CollisionState { get; set; }
		public Transform Transform { get; set; }
		public Vector3 Velocity { get; set; }

		public FirePlace Fireplace { get; set; }

		private SpriteRenderer _renderer;
		public PlayerController Player;
		public MovementState MovementState { get; set; }
		public bool NoGravity;

		void Awake()
		{
			CollisionState = new CollisionState();
			Movement = new MovementHandler(this);
			_heatHandler = new HeatHandler(this);

			Collider = GetComponent<BoxCollider2D>();
			BoxCollider = GetComponent<BoxCollider2D>();
			Transform = GetComponent<Transform>();
			_renderer = GetComponent<SpriteRenderer>();
			MovementState = new MovementState();
		}

		void Start()
		{
			Stability = ChannelingHandler.PlStability();
			Intensity = ChannelingHandler.PlIntensity();
			Control = ChannelingHandler.PlControl();
			DistanceFromPlayer = Stability;
		}

        private bool _firstUpdate = true;
		private float _emberEffectTime;

		void Update()
		{
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

		public void ActivatePoint()
		{
			_renderer.enabled = false;
			Fireplace.PlEnter(this);
		}

		public void DeactivatePoint()
		{
			_renderer.enabled = true;
			Fireplace.PlLeave();
			NoGravity = false;
			Fireplace = null;
		}

		public bool OnPoint()
		{
			return Fireplace != null && Vector2.Distance(Fireplace.transform.position, transform.position) < 0.1f;
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
						Fireplace = fireplace;
						Collider2D col = fireplace.GetComponent<Collider2D>();
						MovementState.SetPivot(col.transform.position, col);
						MovementState.MovementOverridden = true;
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

		void OnTriggerEnter2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
			{
				if (col.GetComponent<FirePlace>().IsAccessible)
				{
					Fireplace = col.GetComponent<FirePlace>();
					MovementState.SetPivot(col.transform.position, col);
					MovementState.MovementOverridden = true;
				}
			}
		}
	}
}
