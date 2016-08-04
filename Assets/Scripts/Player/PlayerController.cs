using Assets.Scripts.Heat;
using Assets.Scripts.Player.Config;
using System;
using Assets.Scripts.Player.PL;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player
{
	public class PlayerController : MonoBehaviour, IVariableHeater
	{
		public GameObject PilotedLight;

		[Range(1, 10)]
		public float BaseStability;

		[Range(1, 10)]
		public float BaseIntensity;

		[Range(1, 10)]
		public float BaseControl;

		private float _stability;
		private float _currentHeatRayDistance = 1f;
		public float HeatRayDistance { get { return _currentHeatRayDistance; } }

		private float _intensity;
		private float _currentHeatIntensity = 1f;
		public float HeatIntensity { get { return _currentHeatIntensity; } }

		private float _control;

		public Transform Transform { get; set; }
		public Collider2D Collider { get; set; }

		private HeatHandler _heatHandler;

		void Awake()
		{
			SetupVariables();

			Transform = transform.parent.parent;
			Collider = Transform.GetComponent<BoxCollider2D>();

			_heatHandler = new HeatHandler(this);

		}

		void Start()
		{
			BaseStability = PlayerPrefs.GetFloat(Variable.Stability.ToString());
			BaseIntensity = PlayerPrefs.GetFloat(Variable.Intensity.ToString());
			BaseControl = PlayerPrefs.GetFloat(Variable.Control.ToString());

			SetVariablesByChanneler();

			SetHeatRayDistanceToDefault();
			SetHeatIntensityToDefault();
		}

		private static void SetupVariables()
		{
			foreach (Variable variable in Enum.GetValues(typeof(Variable)))
			{
				if (PlayerPrefs.HasKey(variable.ToString()) == false)
					PlayerPrefs.SetFloat(variable.ToString(), 5f);
			}
		}

		private float _heatRayDistanceLastFrame;
		private float _emberEffectTime;

		void Update()
		{
			SetVariablesByChanneler();
			HeatIce(SelectEmberEffect());
		}

		private void SetVariablesByChanneler()
		{
			_stability = ChannelingHandler.Stability(BaseStability);
			_intensity = ChannelingHandler.Intensity(BaseIntensity);
			_control = ChannelingHandler.Control(BaseControl);
		}

		private EmberEffect SelectEmberEffect()
		{
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
			return effect;
		}

		private void SetHeatRayDistanceToDefault()
		{
			_currentHeatRayDistance = _stability;
			_heatRayDistanceLastFrame = _stability;
		}

		private void SetHeatIntensityToDefault()
		{
			_currentHeatIntensity = _intensity;
		}

		public void HeatIce(EmberEffect effect)
		{
			_heatHandler.OneCircleHeat(effect);
			_heatHandler.TwoCirclesHeat(effect);
		}

		public void CreateLight()
		{
			ChannelingHandler.StopChanneling(_stability, _intensity, _control);

			Vector3 pilotedLightPosition = Transform.localScale.x > 0f
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

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			var box = transform.parent.parent.GetComponent<BoxCollider2D>();
			Gizmos.DrawCube(box.bounds.center, new Vector3(box.bounds.size.x, box.bounds.size.y, 1));
		}
	}
}