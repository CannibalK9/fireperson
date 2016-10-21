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

		protected HeatHandler[] _heatHandlers;

		public float DefaultHeatIntensity = 1f;
	    public float DefaultHeatRayDistance = 1f;

		private GameObject _particleObject;
		private PipeParticle _currentPipeParticle;

		void Update()
		{
			PipeParticle particle;
			if (IsAccessible && IsLit)
				particle = PipeParticle.Flames;
			else if (IsAccessible && IsLit == false)
				particle = PipeParticle.Wind;
			else if (IsAccessible == false && this is ChimneyLid && IsLit)
				particle = PipeParticle.Smoke;
			else
				particle = PipeParticle.None;

			if (_currentPipeParticle != particle)
			{
				_currentPipeParticle = particle;
				Destroy(_particleObject);
				if (particle == PipeParticle.Flames)
				{
					GameObject obj = Resources.Load("particles/" + particle) as GameObject;
					if (obj == null)
						return;
					_particleObject = Instantiate(obj);
					ParticleSystem particles = _particleObject.GetComponent<ParticleSystem>();
					_particleObject.transform.parent = transform.GetComponentInChildren<CircleCollider2D>().transform;
					_particleObject.transform.localPosition = new Vector2(0, 0f);
					switch (particle)
					{
						case PipeParticle.Flames:
							SetParticlesTransform();
							break;
						case PipeParticle.Wind:
							SetParticlesTransform();
							break;
						case PipeParticle.Smoke:
							SetParticlesTransform();
							break;
						default:
							break;
					}
				}
			}
		}

		private void SetParticlesTransform()
		{
			
		}

		void Awake()
		{
			Collider = GetComponent<CircleCollider2D>();
			HeatIntensity = DefaultHeatIntensity;
			HeatRayDistance = DefaultHeatRayDistance;
			_heatHandlers = GetComponentsInChildren<HeatHandler>();
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
			if (IsFullyIgnited == false)
			{
				IsLit = false;
				var stove = this as Stove;
				if (stove != null)
					stove.ExtinguishAllConnectedFireplaces();
			}
		}

		public void Burst()
		{
			if (IsHeatSource)
			{
				IsFullyIgnited = !IsFullyIgnited;
				LightFireplace();
			}
		}

        public void LightFully()
        {
            if (IsHeatSource)
            {
                IsFullyIgnited = true;
                LightFireplace();
            }
        }

		private void LightFireplace()
		{
			IsLit = true;
			SendMessages();

			Stove stove = this as Stove;
            if (stove != null)
                stove.LightAllConnectedFireplaces();
		}

		private void SendMessages()
		{
			if (IsHeatSource)
			{
				LayerMask mask = 1 << LayerMask.NameToLayer(Layers.Denizens) | Layers.Platforms;

				Vector2 rotation = new Vector2(transform.rotation.x, transform.rotation.y);

				RaycastHit2D leftHit = Physics2D.Raycast(transform.position, Vector2.left + rotation, 10f, mask);
				if (leftHit)
					SendRaycastMessage(leftHit, DirectionTravelling.Right);

				RaycastHit2D rightHit = Physics2D.Raycast(transform.position, Vector2.right + rotation, 10f, mask);
				if (rightHit)
					SendRaycastMessage(rightHit, DirectionTravelling.Left);
			}
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

		public void Connect(FirePlace fp)
		{
			IsAccessible = false;

			if (F1 == null)
				F1 = fp;
			else if (F2 == null)
				F2 = fp;
			else if (F3 == null)
				F3 = fp;
			else if (F4 == null)
				F4 = fp;
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
