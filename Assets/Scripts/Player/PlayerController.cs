using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Movement;
using Assets.Scripts.Player.Config;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player
{
	public class PlayerController : MonoBehaviour, IController, IVariableHeater
	{
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

		public LayerMask _platformMask = Layers.Platforms;
		public LayerMask PlatformMask { get { return _platformMask; } }

		[Range(0f, 90f)]
		public float _slopeLimit = 30f;
		public float SlopeLimit { get { return _slopeLimit; } }

		public AnimationCurve SlopeSpeedMultiplier
		{
			get
			{
				return new AnimationCurve(new Keyframe(-90f, 1.5f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
			}
		}

		[Range(2, 20)]
		public int _totalHorizontalRays = 8;
		public int TotalHorizontalRays { get { return _totalHorizontalRays; } }

		[Range(2, 20)]
		public int _totalVerticalRays = 4;
		public int TotalVerticalRays { get { return _totalVerticalRays; } }

		[Range(1, 10)]
		public float BaseStability;

		[Range(1, 10)]
		public float BaseIntensity;

		[Range(1, 10)]
		public float BaseControl;

		private float _stability;
		private float _currentHeatRayDistance;
		public float HeatRayDistance { get { return _currentHeatRayDistance; } }

		
		private float _intensity;
		private float _currentHeatIntensity;
		public float HeatIntensity { get { return _currentHeatIntensity; } }

		private float _control;

		public Transform Transform { get; set; }
		public Collider2D Collider { get; set; }
		public BoxCollider2D BoxCollider { get; set; }
		public CollisionState CollisionState { get; set; }
		public bool IsGrounded { get { return CollisionState.Below; } }
		public Vector3 Velocity { get; set; }
		public List<RaycastHit2D> RaycastHitsThisFrame { get; set; } 
		public float VerticalDistanceBetweenRays { get; set; }
		public float HorizontalDistanceBetweenRays { get; set; }

		public MovementHandler Movement;
		private HeatHandler _heatHandler;

		public GameObject PilotedLight;

		void Awake()
		{
			SetupVariables();

			Transform = transform.parent.parent;
			Collider = GetComponent<BoxCollider2D>();
			BoxCollider = GetComponent<BoxCollider2D>();
			CollisionState = new CollisionState();
			RaycastHitsThisFrame = new List<RaycastHit2D>(2);

			SkinWidth = _skinWidth;

			Movement = new MovementHandler(this);
			_heatHandler = new HeatHandler(this);

			SetHeatRayDistanceToDefault();
		}

		void Start()
		{
			BaseStability = PlayerPrefs.GetFloat(Variable.Stability.ToString());
			BaseIntensity = PlayerPrefs.GetFloat(Variable.Intensity.ToString());
			BaseControl = PlayerPrefs.GetFloat(Variable.Control.ToString());
		}

		private void SetupVariables()
		{
			foreach (Variable variable in Enum.GetValues(typeof(Variable)))
			{
				if (PlayerPrefs.HasKey(variable.ToString()) == false)
					PlayerPrefs.SetFloat(variable.ToString(), 1f);
			}
		}

		private float _heatRayDistanceLastFrame;
		private float _emberEffectTime;

		void Update()
		{
			_stability = ChannelingHandler.Stability(BaseStability);
			_intensity = ChannelingHandler.Intensity(BaseIntensity);
			_control = ChannelingHandler.Control(BaseControl);

            var effect = EmberEffect.None;

            if (_currentHeatRayDistance != _stability && _heatRayDistanceLastFrame == _currentHeatRayDistance && _currentHeatRayDistance != 0)
            {
                SetHeatRayDistanceToDefault();
                effect = EmberEffect.Strong;
                //play a sound
            }
            else if (_heatRayDistanceLastFrame != _currentHeatRayDistance)
                effect = EmberEffect.Light;
            else if (_currentHeatRayDistance < 0)
            {
                _currentHeatRayDistance = 0;
                //frozen
            }
			_heatRayDistanceLastFrame = _currentHeatRayDistance;

			_emberEffectTime -= Time.deltaTime;

			if (_emberEffectTime < 0)
			{
				effect = EmberEffect.Light;
				_emberEffectTime = Random.Range(10, 60);
			}

            HeatIce(effect);
		}

		private void SetHeatRayDistanceToDefault()
		{
			_currentHeatRayDistance = _stability;
			_heatRayDistanceLastFrame = _stability;
		}

		public void HeatIce(EmberEffect effect)
		{
			_heatHandler.OneCircleHeat(effect);
			_heatHandler.TwoCirclesHeat(effect);
		}

		public void CreateLight()
		{
			ChannelingHandler.StopChanneling(_stability, _intensity, _control);

			Vector3 pilotedLightPosition = Movement.IsFacingRight
				? transform.position + new Vector3(0.4f, 0, 0)
				: transform.position + new Vector3(-0.4f, 0, 0);

			var pl = (GameObject)Instantiate(
				PilotedLight,
				pilotedLightPosition,
				transform.rotation);

			pl.GetComponent<PilotedLightController>().Player = this;
		}

		public void Spotted()
		{
			_currentHeatRayDistance -= Time.deltaTime * 3f;
		}

		private void RecalculateDistanceBetweenRays()
		{
			var colliderUseableHeight = BoxCollider.size.y * Mathf.Abs(Transform.localScale.y) - (2f * _skinWidth);
			VerticalDistanceBetweenRays = colliderUseableHeight / (TotalHorizontalRays - 1);

			var colliderUseableWidth = BoxCollider.size.x * Mathf.Abs(Transform.localScale.x) - (2f * _skinWidth);
			HorizontalDistanceBetweenRays = colliderUseableWidth / (TotalVerticalRays - 1);
		}

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			var box = GetComponent<BoxCollider2D>();
			Gizmos.DrawCube(box.bounds.center, new Vector3(box.bounds.size.x, box.bounds.size.y, 1));
		}
	}
}