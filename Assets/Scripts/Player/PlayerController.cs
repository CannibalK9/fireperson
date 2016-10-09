using Assets.Scripts.CameraHandler;
using Assets.Scripts.Heat;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.Player.Config;
using Assets.Scripts.Player.PL;
using UnityEngine;

namespace Assets.Scripts.Player
{
	public class PlayerController : MonoBehaviour
	{
		public GameObject PilotedLight;
		public GameObject Tether;

		public float BaseStability { get; private set; }
		public float BaseIntensity { get; private set; }
		public float BaseControl { get; private set; }

		private float _currentStability = 1f;
		public float Stability { get { return _currentStability; } }
		private float _stabilityLastFrame;

		private float _currentIntensity = 1f;
		public float Intensity { get { return _currentIntensity; } }
		private float _intensityLastFrame;

		private float _currentControl = 1f;
		public float Control { get { return _currentControl; } }
		private float _controlLastFrame;

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
			SetBases();
			SetStabilityToDefault();
			SetIntensityToDefault();
			SetControlToDefault();
		}

		void Update()
		{
			SetBases();
			_currentControl = BaseControl;
			if (ChannelingHandler.IsChanneling && ChannelingHandler.ChannelingSet == false && ChannelingHandler.PlExists)
			{
				_currentStability = BaseStability - ChannelingHandler.ChannelPercent * BaseStability;
			}
			UpdateStabilityIntensity();
		}

		private void SetBases()
		{
			BaseStability = PlayerPrefs.GetFloat(FloatVariable.Stability.ToString());
			BaseIntensity = PlayerPrefs.GetFloat(FloatVariable.Intensity.ToString());
			BaseControl = PlayerPrefs.GetFloat(FloatVariable.Control.ToString());
		}

		private void UpdateStabilityIntensity()
		{
			if (_currentStability != BaseStability && _stabilityLastFrame == _currentStability && _currentStability >= 0 && ChannelingHandler.IsChanneling == false)
			{
				_currentStability = BaseStability;
				_currentIntensity = BaseIntensity;
			}

			if (_currentStability < 0)
			{
				_currentStability = 0;
				//frozen
			}

			if (_stabilityLastFrame != _currentStability || _intensityLastFrame != _currentIntensity)
			{
				foreach (var hh in _heatHandlers)
				{
					hh.UpdateHeat(new HeatMessage(_currentIntensity / 50, 1 + _currentStability / 50));
				}
			}
			
			_stabilityLastFrame = _currentStability;
			_intensityLastFrame = _currentIntensity;
		}

		private void SetStabilityToDefault()
		{
			_currentStability = BaseStability;
		}

		private void SetIntensityToDefault()
		{
			_currentIntensity = BaseIntensity;
		}

		private void SetControlToDefault()
		{
			_currentControl = BaseControl;
		}

		public void CreateLight()
		{
			ChannelingHandler.PlExists = true;

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
				var tetherObject = Instantiate(Tether);

				var tether = tetherObject.GetComponent<Tether>();
				tether.Player = transform;
				tether.Pl = pl.transform;
			}
		}

		public void StopChanneling()
		{
			ChannelingHandler.StopChanneling();
		}

		public void Spotted()
		{
			_currentStability -= Time.deltaTime * 3f;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			var box = transform.parent.parent.GetComponent<BoxCollider2D>();
			Gizmos.DrawCube(box.bounds.center, new Vector3(box.bounds.size.x, box.bounds.size.y, 1));
		}
	}
}