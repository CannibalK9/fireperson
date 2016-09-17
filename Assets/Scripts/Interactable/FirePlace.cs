using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class FirePlace : MonoBehaviour
    {
        public bool IsLit;
        public bool IsAccessible;
        public bool IsHeatSource;
		public bool IsFullyIgnited;

		public Collider2D Collider { get; set; }
		public float HeatRayDistance { get; set; }
		public float HeatIntensity { get; set; }
		public bool ContainsPl { get; set; }

		public FirePlace F1;
        public FirePlace F2;
        public FirePlace F3;
        public FirePlace F4;

		private HeatHandler[] _heatHandlers;

	    public float DefaultHeatIntensity = 1f;
	    public float DefaultHeatRayDistance = 1f;

	    void Awake()
		{
			Collider = GetComponent<CircleCollider2D>();
			HeatIntensity = DefaultHeatIntensity;
			HeatRayDistance = DefaultHeatRayDistance;
			_heatHandlers = GetComponentsInChildren<HeatHandler>();
		}

		private bool _firstUpdate = true;

        void Update()
        {
			var stove = this as Stove;
			if (IsFullyIgnited && stove != null)
			{
				if (ContainsPl == false && stove.AllChimneysAreClosed() == false)
				{
					IsFullyIgnited = false;
					IsLit = false;
					if (stove.HasConnectedFullyLitStove() == false)
						stove.ExtinguishAllConnectedFireplaces();
				}
			}

			foreach (var hh in _heatHandlers)
			{
				hh.EnableCollider(IsLit);
			}

			if (IsLit)
			{
				var effect = EmberEffect.None;

				if (_firstUpdate)
				{
					effect = EmberEffect.Strong;
					_firstUpdate = false;
				}
			}
			else
			{
				_firstUpdate = false;
			}

            if (IsLit == false || IsHeatSource == false)
                return;

            LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Denizens) | 1 << LayerMask.NameToLayer(Layers.OutdoorWood);

            Vector2 rotation = new Vector2(transform.rotation.x, transform.rotation.y);

            RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left + rotation, 10f, mask);
            if (leftHit)
                SendRaycastMessage(leftHit, DirectionTravelling.Right);

            RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right + rotation, 10f, mask);
            if (rightHit)
                SendRaycastMessage(rightHit, DirectionTravelling.Left);
        }

        private void SendRaycastMessage(RaycastHit2D hit, DirectionTravelling direction)
        {
            hit.collider.SendMessage("MoveToFireplace", direction, SendMessageOptions.DontRequireReceiver);
        }

        public List<FirePlace> GetConnectedFireplaces()
        {
			var fpList = new List<FirePlace>();
			if (F1 != null)
				fpList.Add(F1);
			if (F2 != null)
				fpList.Add(F2);
			if (F3 != null)
				fpList.Add(F3);
			if (F4 != null)
				fpList.Add(F4);
			return fpList;
		}

		public void PlEnter()
		{
			ContainsPl = true;
			LightFireplace();
		}

		public void PlLeave()
		{
			ContainsPl = false;

			var stove = this as Stove;
			if (stove != null && stove.IsFullyIgnited == false)
			{
				IsLit = false;
				stove.ExtinguishAllConnectedFireplaces();
			}
			else if (IsFullyIgnited == false && GetConnectedFireplaces().Any() == false)
				IsLit = false;
		}

		public void Burst()
		{
			if (IsHeatSource)
				IsFullyIgnited = !IsFullyIgnited;
		}

		public void LightFully()
		{
			IsFullyIgnited = true;
			LightFireplace();
		}

		private void LightFireplace()
		{
			_firstUpdate = true;
			IsLit = true;

			Stove stove = this as Stove;
			if (stove != null)
				stove.LightAllConnectedFireplaces();
		}

		public void Disconnect(FirePlace fp)
		{
			IsAccessible = true;

			if (F1 == fp)
				F1 = null;
			else if (F2 == fp)
				F2 = null;
			else if (F3 == fp)
				F3 = null;
			else if (F4 == fp)
				F4 = null;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(0, 1, 0, 0.5F);
			var circle = GetComponent<CircleCollider2D>();
			if (circle != null)
				Gizmos.DrawSphere(circle.bounds.center, circle.radius);
		}
	}
}
