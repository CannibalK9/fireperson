using Assets.Scripts.Heat;
using Assets.Scripts.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PilotedLightController : MonoBehaviour, IVariableHeater, IController
    {
        public float DurationInSeconds = 5f;
        private float MinimumScale = 2.5f;
        private Vector3 InitialScale;

        private HeatHandler _heatHandler;
        public float _heatRayDistance = 0.2f;
        public float HeatRayDistance { get { return _heatRayDistance; } }

        [Range(2, 20)]
        public int _totalHorizontalRays = 4;
        public int TotalHorizontalRays { get { return _totalHorizontalRays; } }

        [Range(2, 20)]
        public int _totalVerticalRays = 4;
        public int TotalVerticalRays { get { return _totalVerticalRays; } }

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

        public LayerMask _triggerMask = 0;
        public LayerMask _platformMask = 0;
        public LayerMask PlatformMask { get { return _platformMask; } }

        public MovementHandler _movement;
        public BoxCollider2D BoxCollider { get; set; }
        public CollisionState CollisionState { get; set; }
        public Transform Transform { get; set; }
        public Vector3 Velocity { get; set; }

        public bool IsGrounded { get { return CollisionState.below; } }
        public List<RaycastHit2D> RaycastHitsThisFrame { get; set; }
        public float VerticalDistanceBetweenRays { get; set; }
        public float HorizontalDistanceBetweenRays { get; set; }

        void Awake()
        {
            _movement = new MovementHandler(this);
            _heatHandler = new HeatHandler(this);

            InitialScale = transform.localScale;

            BoxCollider = GetComponent<BoxCollider2D>();
            CollisionState = new CollisionState();
            Transform = GetComponent<Transform>();
            RaycastHitsThisFrame = new List<RaycastHit2D>(2);
        }
        void Update()
        {
            DurationInSeconds -= Time.deltaTime;
            Vector3 newScale = Vector3.one * (DurationInSeconds + MinimumScale);
            if (newScale.x < InitialScale.x)
            {
                transform.localScale = newScale;
                RecalculateDistanceBetweenRays();
            }

            if (DurationInSeconds <= 0)
                DestroyObject(gameObject);
        }

        public void HeatIce()
        {
            _heatHandler.Heat();
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
