using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using System.Collections.Generic;
using Assets.Scripts.Player.PL;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class FirePlace : MonoBehaviour, IVariableHeater
    {
        public bool IsLit;
        public bool IsAccessible;
        public bool IsHeatSource;

		public Collider2D Collider { get; set; }
		public float HeatRayDistance { get; set; }
		public float HeatIntensity { get; set; }

        public FirePlace F1;
        public FirePlace F2;
        public FirePlace F3;
        public FirePlace F4;

        private Vector2 _origin;
        private Vector2 _rotation;
        private RaycastHit2D _hit;
		private HeatHandler _heatHandler;

	    public float DefaultHeatIntensity = 1f;
	    public float DefaultHeatRayDistance = 3f;

	    void Awake()
		{
			_heatHandler = new HeatHandler(this);
			Collider = GetComponent<CircleCollider2D>();
			HeatIntensity = DefaultHeatIntensity;
			HeatRayDistance = DefaultHeatRayDistance;
			_wasLit = IsLit;
		}

		private bool _firstUpdate = true;

        void Update()
        {
			if (IsLit)
			{
				var effect = EmberEffect.None;

				if (_firstUpdate)
				{
					effect = EmberEffect.Strong;
					_firstUpdate = false;
				}

				_heatHandler.OneCircleHeat(effect);
			}
			else
				_firstUpdate = false;

            if (IsLit == false || IsHeatSource == false)
                return;

            LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Denizens) | 1 << LayerMask.NameToLayer(Layers.OutdoorWood);

            _origin = transform.position;
            _rotation = new Vector2(transform.rotation.x, transform.rotation.y);

            _hit = Physics2D.Raycast(_origin, Vector2.left + _rotation, 10f, mask);
            if (_hit)
                SendRaycastMessage(_hit, DirectionTravelling.Right);

            _hit = Physics2D.Raycast(_origin, Vector2.right + _rotation, 10f, mask);
            if (_hit)
                SendRaycastMessage(_hit, DirectionTravelling.Left);
        }

        private void SendRaycastMessage(RaycastHit2D hit, DirectionTravelling direction)
        {
            hit.collider.SendMessage("MoveToFireplace", direction, SendMessageOptions.DontRequireReceiver);
        }

        public List<FirePlace> GetConnectedFireplaces()
        {
            return new List<FirePlace> { F1, F2, F3, F4 };
        }

		private bool _wasLit;

		public void PlEnter(PilotedLightController pl)
		{
			HeatIntensity += pl.Intensity;
			HeatRayDistance += pl.Stability;
			_firstUpdate = true;
			IsLit = true;

			Stove stove = this as Stove;
			if (stove != null)
				stove.LightAllConnectedFireplaces();
		}

		public void PlLeave()
		{
			HeatIntensity = DefaultHeatIntensity;
			HeatRayDistance = DefaultHeatRayDistance;

			var stove = this as Stove;
			if (stove != null)
			{
				bool lightable = stove.CanBeLitByDenizens();
				_wasLit = !_wasLit && lightable;
				IsLit = IsHeatSource && _wasLit;

				if (IsLit == false)
					stove.ExtinguishAllConnectedFireplaces();
			}
		}

		public void Burst()
		{
			_firstUpdate = true;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(0, 1, 0, 0.5F);
			var circle = GetComponent<CircleCollider2D>();
			Gizmos.DrawSphere(circle.bounds.center, circle.radius);
		}
	}
}
