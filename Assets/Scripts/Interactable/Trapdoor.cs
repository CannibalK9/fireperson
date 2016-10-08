using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class Trapdoor : MonoBehaviour
	{
		private Ice.Ice _ice;
		private bool _fallen;

		public GameObject LeftPlatform;
		public GameObject RightPlatform;

		void Awake()
		{
			_ice = gameObject.GetComponentInChildren<Ice.Ice>();
		}

		void Update()
		{
			if (_fallen == false && _ice.AnyJointEnabled == false)
			{
				_fallen = true;
				CreateFallenTrapdoor();
				AddEdges();
			}
		}

		private void CreateFallenTrapdoor()
		{
			Destroy(_ice.gameObject);
			gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
			gameObject.layer = LayerMask.NameToLayer(Layers.Background);
		}

		private void AddEdges()
		{
			if (LeftPlatform != null)
				LeftPlatform.GetComponent<ClimbableEdges>().RightEdge = true;

			if (RightPlatform != null)
				RightPlatform.GetComponent<ClimbableEdges>().LeftEdge = true;
		}
	}
}
