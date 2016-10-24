using Assets.Scripts.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class Stilt : MonoBehaviour
	{
		public bool IsExtended;
		public bool RunnerRight;

        private HingeJoint2D _hingeJoint;
		private float _extendedPosition;
		private float _collapsedPosition;
		private float _runnerLength;
		private Transform _interactionPoint;

        void Awake()
        {
			_runnerLength = GetComponentInChildren<Rigidbody2D>().transform.localScale.x;
            _hingeJoint = GetComponentInChildren<HingeJoint2D>();
			_interactionPoint = GetComponentInChildren<BoxCollider2D>().transform;

			if (IsExtended)
			{
				_extendedPosition = _hingeJoint.connectedAnchor.x;
				_collapsedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x + _runnerLength
					: _hingeJoint.connectedAnchor.x - _runnerLength;
				SetInteractionPoint();
			}
			else
			{
				_collapsedPosition = _hingeJoint.connectedAnchor.x;
				_extendedPosition = RunnerRight
					? _hingeJoint.connectedAnchor.x - _runnerLength
					: _hingeJoint.connectedAnchor.x + _runnerLength;
				SetInteractionPoint();
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
				SetInteractionPoint();
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
				SetInteractionPoint();
			}

			var limits = new JointAngleLimits2D();
			float angleLimit = 5 + (175 * ((_hingeJoint.connectedAnchor.x - _extendedPosition) / (_collapsedPosition - _extendedPosition)));

			limits.max = -angleLimit;
			limits.min = angleLimit;
			_hingeJoint.limits = limits;
		}

		private void SetInteractionPoint()
		{
			_interactionPoint.localPosition = new Vector2(-0.5f, IsExtended ? 0 : RunnerRight ? -_runnerLength : _runnerLength);
		}
	}
}
