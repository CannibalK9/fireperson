using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class HeatHandler : MonoBehaviour
	{
		public HeatMessage HeatMessage { get; set; }
		private GameObject _steam;
        private GameObject _emberLight;
        private GameObject _emberStrong;
		protected Collider _collider;

		private float _defaultHeight;
		private float _defautRadius;

		void Awake()
		{
			_collider = GetComponent<Collider>();

			if (_collider is CapsuleCollider)
			{
				CapsuleCollider collider = _collider as CapsuleCollider;
				_defaultHeight = collider.height;
				_defautRadius = collider.radius;
			}
			else
			{
				SphereCollider collider = _collider as SphereCollider;
				_defautRadius = collider.radius;
			}

			SetColliderSizes(1);
		}

		void Start()
		{
			_steam = (GameObject)Resources.Load("particles/steam");
			_emberLight = (GameObject)Resources.Load("particles/emberLight");
			_emberStrong = (GameObject)Resources.Load("particles/emberStrong");
		}

		public void SetColliderSizes(float additionalSize)
		{
			if (_collider is CapsuleCollider)
			{
				CapsuleCollider collider = _collider as CapsuleCollider;

				collider.radius = _defautRadius + additionalSize;
				collider.height = 1 + (collider.radius / 3) * 2;
			}
			else
			{
				SphereCollider collider = _collider as SphereCollider;
				collider.radius = _defautRadius + additionalSize;
			}
		}

		void OnDrawGizmos()
		{
			if (_collider is SphereCollider)
			{
				SphereCollider collider = _collider as SphereCollider;
				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(collider.bounds.center, collider.radius);
			}
		}
	}
}