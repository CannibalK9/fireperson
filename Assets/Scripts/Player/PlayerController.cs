using System;
using Assets.Scripts.Heat;
using Assets.Scripts.Movement;
using System.Collections.Generic;
using UnityEngine;

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

		public LayerMask _triggerMask = 0;
		public LayerMask _platformMask = 0;
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

		private float _defaultHeatRayDistance;
		private float _currentHeatRayDistance;
		public float _heatRayDistance = 0.2f;
		public float HeatRayDistance { get { return _heatRayDistance; } }

		public Transform Transform { get; set; }
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
			Transform = transform.parent.parent;
			BoxCollider = GetComponent<BoxCollider2D>();
			CollisionState = new CollisionState();
			RaycastHitsThisFrame = new List<RaycastHit2D>(2);

			SkinWidth = _skinWidth;

			Movement = new MovementHandler(this);
			_heatHandler = new HeatHandler(this);

			IgnoreCollisionLayersOutsideTriggerMask();

			_defaultHeatRayDistance = _heatRayDistance;
			_currentHeatRayDistance = _heatRayDistance;
		}

		void Update()
		{
			if (Math.Abs(_currentHeatRayDistance - _heatRayDistance) < 0.01f)
				_heatRayDistance = _defaultHeatRayDistance;
			else
				_currentHeatRayDistance = _heatRayDistance;
		}

		private void IgnoreCollisionLayersOutsideTriggerMask()
		{
			for (var i = 0; i < 32; i++)
			{
				if ((_triggerMask.value & 1 << i) == 0)
					Physics2D.IgnoreLayerCollision(gameObject.layer, i);
			}
		}

		public void HeatIce()
		{
			_heatHandler.OneCircleHeat();
			_heatHandler.TwoCirclesHeat();
		}

		public void CreatePilotedLight()
		{
			Vector3 pilotedLightPosition = Movement.IsFacingRight
				? transform.position + new Vector3(0.4f, 0, 0)
				: transform.position + new Vector3(-0.4f, 0, 0);

			Instantiate(
				PilotedLight,
				pilotedLightPosition,
				transform.rotation);
		}

		public void Spotted()
		{
			_heatRayDistance -= Time.deltaTime * 0.3f;
		}

		public void WarpToGrounded()
		{
			while (!IsGrounded)
			{
				Movement.Move(new Vector3(0, -1f, 0));
			}
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