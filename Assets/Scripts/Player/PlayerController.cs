using Assets.Scripts.CameraHandler;
using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player;
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
		public float DistanceFromPlayer { get { return 10 + _currentControl / 5; } }
		private float _controlLastFrame;

		public Transform Transform { get; set; }
		public Collider2D Collider { get; set; }

		private HeatHandler[] _heatHandlers;
		private SmoothCamera2D _camera;
		private float _timeSpotted;
		private float _timeSpottedLastFrame;
		private ControlBorder _controlBorder;
		private bool _isScoutActive;

		void Awake()
		{
			Transform = transform.parent.parent;
			Collider = Transform.GetComponent<BoxCollider2D>();

			_heatHandlers = Transform.GetComponentsInChildren<HeatHandler>();
			_camera = Transform.GetComponent<AnimationScript>().CameraScript;
			_controlBorder = GetComponentInChildren<ControlBorder>();
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
			ResetTimeIfNotSpotted();
			_currentControl = BaseControl;
			UpdateStabilityIntensity();
			UpdateControlBorder();

			if (KeyBindings.GetKeyUp(Controls.Ability1) && ChannelingHandler.ChannelingSet)
			{
				ChannelingHandler.IsTethered = !ChannelingHandler.IsTethered;
			}
		}

		private void SetBases()
		{
			BaseStability = PlayerPrefs.GetFloat(FloatVariable.Stability.ToString());
			BaseIntensity = PlayerPrefs.GetFloat(FloatVariable.Intensity.ToString());
			BaseControl = PlayerPrefs.GetFloat(FloatVariable.Control.ToString());
		}

		private void ResetTimeIfNotSpotted()
		{
			if (_timeSpotted == _timeSpottedLastFrame)
				_timeSpotted = 0;

			_timeSpottedLastFrame = _timeSpotted;
		}

		private void UpdateStabilityIntensity()
		{
			_currentStability = BaseStability - ChannelingHandler.ChannelPercent * BaseStability - _timeSpotted;
			_currentIntensity = BaseIntensity - ChannelingHandler.ChannelPercent * BaseIntensity - _timeSpotted;

			if (_currentStability < 0)
			{
				_currentStability = 0;
				//frozen
			}

			if (_stabilityLastFrame != _currentStability || _intensityLastFrame != _currentIntensity)
			{
				foreach (var hh in _heatHandlers)
				{
					hh.UpdateHeat(new HeatMessage(_currentIntensity, 1 + _currentStability / ConstantVariables.StabilityHeatRangeModifier));
				}
			}
			
			_stabilityLastFrame = _currentStability;
			_intensityLastFrame = _currentIntensity;
		}

		private void UpdateControlBorder()
		{
			if (_currentControl != _controlLastFrame || _isScoutActive != AbilityState.IsActive(Ability.Scout))
			{
				_controlBorder.SetSize(DistanceFromPlayer * (AbilityState.IsActive(Ability.Scout) ? 4 : 2));
				_isScoutActive = AbilityState.IsActive(Ability.Scout);
				_controlLastFrame = _currentControl;
			}
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
			_timeSpotted += Time.deltaTime * 3;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			var box = transform.parent.parent.GetComponent<BoxCollider2D>();
			Gizmos.DrawCube(box.bounds.center, new Vector3(box.bounds.size.x, box.bounds.size.y, 1));
		}
	}
}