using Assets.Scripts.CameraHandler;
using Assets.Scripts.Heat;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.Player.Config;
using Assets.Scripts.Player.PL;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Player
{
	public class PlayerController : MonoBehaviour
	{
		public GameObject PilotedLight;
		public GameObject Tether;

		public float BaseStability { get; private set; }
		public float BaseIntensity { get; private set; }
		public float BaseControl { get; private set; }

		private float _stability;
		private float _currentHeatRayDistance = 1f;
		public float HeatRayDistance { get { return _currentHeatRayDistance; } }

		private float _intensity;
		private float _currentHeatIntensity = 1f;
		public float HeatIntensity { get { return _currentHeatIntensity; } }

		private float _control;

		public Transform Transform { get; set; }
		public Collider2D Collider { get; set; }

		private HeatHandler[] _heatHandlers;
		private SmoothCamera2D _camera;

		void Awake()
		{
			Transform = transform.parent.parent;
			Collider = Transform.GetComponent<BoxCollider2D>();

			_heatHandlers = Transform.GetComponentsInChildren<HeatHandler>();
			_camera = Transform.GetComponent<AnimationScript>().CameraScript;
		}

		void Start()
		{
			SetVariablesByChanneler();

			SetHeatRayDistanceToDefault();
			SetHeatIntensityToDefault();
		}

		private float _heatRayDistanceLastFrame;
		private float _emberEffectTime;

		void Update()
		{
			BaseStability = PlayerPrefs.GetFloat(FloatVariable.Stability.ToString());
			BaseIntensity = PlayerPrefs.GetFloat(FloatVariable.Intensity.ToString());
			BaseControl = PlayerPrefs.GetFloat(FloatVariable.Control.ToString());

			SetVariablesByChanneler();
			SelectEmberEffect();
		}

		private void SetVariablesByChanneler()
		{
			_stability = ChannelingHandler.Stability(BaseStability);
			_intensity = ChannelingHandler.Intensity(BaseIntensity);
			_control = ChannelingHandler.Control(BaseControl);
		}

		private void SelectEmberEffect()
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
			foreach (var hh in _heatHandlers)
			{
				hh.UpdateHeat(new HeatMessage(_currentHeatIntensity / 100, _heatRayDistanceLastFrame / 10));
			}
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
			_camera.Pl = pl.transform;

			if (AbilityState.IsActive(Ability.Tether))
			{
				var tetherObject = (GameObject)Instantiate(
					Tether,
					pilotedLightPosition,
					transform.rotation);

				var tether = tetherObject.GetComponent<Tether>();
				tether.Player = transform;
				tether.Pl = pl.transform;
			}
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