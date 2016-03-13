using Assets.Scripts.Heat;
using Assets.Scripts.Movement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour, IController, IVariableHeater
    {
        public event Action<Collider2D> onTriggerEnterEvent;
        public event Action<Collider2D> onTriggerStayEvent;
        public event Action<Collider2D> onTriggerExitEvent;

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

        public float _heatRayDistance = 0.2f;
        public float HeatRayDistance { get { return _heatRayDistance; } }

        public Transform Transform { get; set; }
        public BoxCollider2D BoxCollider { get; set; }
        public CollisionState CollisionState { get; set; }
        public bool IsGrounded { get { return CollisionState.below; } }
        public Vector3 Velocity { get; set; }
        public List<RaycastHit2D> RaycastHitsThisFrame { get; set; } 
        public float VerticalDistanceBetweenRays { get; set; }
        public float HorizontalDistanceBetweenRays { get; set; }

        public MovementHandler _movement;
        private HeatHandler _heatHandler;

        void Awake()
        {
            _movement = new MovementHandler(this);
            _heatHandler = new HeatHandler(this);

            Transform = GetComponent<Transform>();
            BoxCollider = GetComponent<BoxCollider2D>();
            CollisionState = new CollisionState();
            RaycastHitsThisFrame = new List<RaycastHit2D>(2);
            SkinWidth = _skinWidth;
            IgnoreCollisionLayersOutsideTriggerMask();
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
            _heatHandler.Heat();
        }

        public void CreatePilotedLight()
        {
            Vector3 pilotedLightPosition = _movement.IsFacingRight
                ? transform.position + new Vector3(0.4f, 0, 0)
                : transform.position + new Vector3(-0.4f, 0, 0);

            Instantiate(
                Resources.Load("PilotedLight"),
                pilotedLightPosition,
                transform.rotation);
        }

        public void OnTriggerEnter2D(Collider2D col)
        {
            if (onTriggerEnterEvent != null)
                onTriggerEnterEvent(col);
        }

        public void OnTriggerStay2D(Collider2D col)
        {
            if (onTriggerStayEvent != null)
                onTriggerStayEvent(col);
        }


        public void OnTriggerExit2D(Collider2D col)
        {
            if (onTriggerExitEvent != null)
                onTriggerExitEvent(col);
        }

        public void WarpToGrounded()
        {
            while (!IsGrounded)
            {
                _movement.Move(new Vector3(0, -1f, 0));
            }
        }

        private void RecalculateDistanceBetweenRays()
        {
            var colliderUseableHeight = BoxCollider.size.y * Mathf.Abs(Transform.localScale.y) - (2f * _skinWidth);
            VerticalDistanceBetweenRays = colliderUseableHeight / (TotalHorizontalRays - 1);

            var colliderUseableWidth = BoxCollider.size.x * Mathf.Abs(Transform.localScale.x) - (2f * _skinWidth);
            HorizontalDistanceBetweenRays = colliderUseableWidth / (TotalVerticalRays - 1);
        }
    }
}