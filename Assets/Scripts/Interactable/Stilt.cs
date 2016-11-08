using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class Stilt : MonoBehaviour
	{
		public bool IsExtended;
		public bool RunnerRight;

        private HingeJoint2D _hingeJoint;
		private SliderJoint2D _sliderJoint;
		private float _extendedPosition;
		private float _collapsedPosition;
		private float _runnerLength;
		private Transform _interactionPoint;
		private bool _wasExtended;

        void Awake()
        {
			_runnerLength = GetComponentInChildren<Rigidbody2D>().transform.localScale.x;
			_sliderJoint = GetComponentInChildren<SliderJoint2D>();
            _hingeJoint = _sliderJoint.GetComponent<HingeJoint2D>();
			_interactionPoint = GetComponentInChildren<BoxCollider2D>().transform;

			_sliderJoint.useLimits = true;
			_sliderJoint.useMotor = true;
			_hingeJoint.useLimits = true;

			var limits = new JointTranslationLimits2D();

			if (IsExtended)
			{
				limits.min = RunnerRight ? 0 : -_runnerLength;
				limits.max = RunnerRight ? _runnerLength : 0;

				_extendedPosition = _hingeJoint.connectedAnchor.x;
				_collapsedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x + _runnerLength
					: _hingeJoint.connectedAnchor.x - _runnerLength;
				SetInteractionPoint();
			}
			else
			{
				limits.min = RunnerRight ? -_runnerLength : 0;
				limits.max = RunnerRight ? 0 : _runnerLength;

				_collapsedPosition = _hingeJoint.connectedAnchor.x;
				_extendedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x - _runnerLength
					: _hingeJoint.connectedAnchor.x + _runnerLength;
				SetInteractionPoint();
			}
			_sliderJoint.limits = limits;
			_wasExtended = !IsExtended;
		}

		void Update()
		{
			if (IsExtended != _wasExtended)
			{
				SetSliderMotor();
				_wasExtended = IsExtended;
				SetInteractionPoint();
			}

			SetHingeLimits();
		}

		private void SetHingeLimits()
		{
			var limits = new JointAngleLimits2D();
			float angleLimit = 5 + (175 * (Mathf.Abs(_hingeJoint.transform.localPosition.x) / _runnerLength));

			limits.max = -angleLimit;
			limits.min = angleLimit;
			_hingeJoint.limits = limits;
		}

		private void SetSliderMotor()
		{
			var motor = new JointMotor2D();
			motor.motorSpeed = IsExtended == RunnerRight ? -0.5f : 0.5f;
			motor.maxMotorTorque = 1000;
			_sliderJoint.motor = motor;
		}

		private void SetInteractionPoint()
		{
			_interactionPoint.localPosition = new Vector2(IsExtended ? 0 : RunnerRight ? _runnerLength : -_runnerLength, _interactionPoint.localPosition.y);
		}
	}
}
