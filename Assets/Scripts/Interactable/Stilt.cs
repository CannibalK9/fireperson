using Assets.Scripts.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class Stilt : MonoBehaviour
	{
		public bool IsExtended;
		public bool RunnerRight;

		private List<ParticleSystem> _particles;
        private HingeJoint2D _hingeJoint;
		private float _extendedPosition;
		private float _collapsedPosition;
		private float _runnerLength;

        void Awake()
        {
			_runnerLength = GetComponentInChildren<Rigidbody2D>().transform.localScale.x;
            _hingeJoint = GetComponentInChildren<HingeJoint2D>();
			if (IsExtended)
			{
				_extendedPosition = _hingeJoint.connectedAnchor.x;
				_collapsedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x + _runnerLength
					: _hingeJoint.connectedAnchor.x - _runnerLength;
			}
			else
			{
				_collapsedPosition = _hingeJoint.connectedAnchor.x;
				_extendedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x - _runnerLength
					: _hingeJoint.connectedAnchor.x + _runnerLength;
			}
		}

		void Update()
		{
			Vector2 movement = new Vector2(ConstantVariables.StiltSpeed * Time.deltaTime, 0);

			if (IsExtended)
			{
				if (RunnerRight && _hingeJoint.connectedAnchor.x > _extendedPosition)
				{
					_hingeJoint.connectedAnchor -= movement;
				}
				else if (RunnerRight == false && _hingeJoint.connectedAnchor.x < _extendedPosition)
				{
					_hingeJoint.connectedAnchor += movement;
				}
			}
			else
			{
				if (RunnerRight && _hingeJoint.connectedAnchor.x < _collapsedPosition)
				{
					_hingeJoint.connectedAnchor += movement;
				}
				else if (RunnerRight == false && _hingeJoint.connectedAnchor.x > _collapsedPosition)
				{
					_hingeJoint.connectedAnchor -= movement;
				}
			}

			var limits = new JointAngleLimits2D();
			float angleLimit = 5 + (175 * ((_hingeJoint.connectedAnchor.x - _extendedPosition) / (_collapsedPosition - _extendedPosition)));

			limits.max = -angleLimit;
			limits.min = angleLimit;
			_hingeJoint.limits = limits;
		}
	}
}
