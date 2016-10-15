using Assets.Scripts.Helpers;
using PicoGames.VLS2D;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class Trapdoor : MonoBehaviour
	{
		private Ice.Ice[] _ice;
		private bool _fallen;

		public GameObject LeftPlatform;
		public GameObject RightPlatform;
		public bool IsLeftOnCorner;
		public bool IsRightOnCorner;

		public bool RemainsSolid;
		public float Mass;

		void Awake()
		{
			_ice = gameObject.GetComponentsInChildren<Ice.Ice>();
		}

		void Update()
		{
			if (_fallen == false && _ice.Any(i => i.IsAnchored) == false)
			{
				_fallen = true;
				CreateFallenTrapdoor();
				AddEdges();
			}
		}

		private void CreateFallenTrapdoor()
		{
			foreach (var ice in _ice)
			{
				Destroy(ice.gameObject);
			}
			var r = gameObject.AddComponent<Rigidbody2D>();
			r.mass = Mass;

			if (RemainsSolid == false)
			{
				gameObject.layer = LayerMask.NameToLayer(Layers.Background);
				gameObject.GetComponent<VLSObstructor>().ClearLocalVertices();
			}
		}

		private void AddEdges()
		{
			if (LeftPlatform != null)
			{
				if (IsLeftOnCorner)
				{
					LeftPlatform.GetComponent<ClimbableEdges>().IsRightCorner = false;
					LeftPlatform.GetComponent<ClimbableEdges>().Reset();
				}
				else
					LeftPlatform.GetComponent<ClimbableEdges>().RightEdge = true;
			}
			if (RightPlatform != null)
			{
				if (IsRightOnCorner)
				{
					RightPlatform.GetComponent<ClimbableEdges>().IsLeftCorner = false;
					RightPlatform.GetComponent<ClimbableEdges>().Reset();
				}
				else
					RightPlatform.GetComponent<ClimbableEdges>().LeftEdge = true;
			}
		}
	}
}
