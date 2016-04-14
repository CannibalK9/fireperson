using UnityEngine;

namespace Assets.Scripts.Sprites
{
	public class ClimbableEdges : MonoBehaviour
	{
		public bool LeftEdge;
		public bool RightEdge;

		public GameObject LeftEdgeObject;
		public GameObject RightEdgeObject;

		private GameObject _leftEdge;
		private GameObject _rightEdge;

		void Awake()
		{
			CreateEdges();
		}

		public void ActivateEdges()
		{
			LeftEdge = true;
			RightEdge = true;

			CreateEdges();
		}

		private void CreateEdges()
		{
			if (LeftEdge && LeftEdgeObject != null && _leftEdge == null)
			{
				_leftEdge = Instantiate(LeftEdgeObject);

				_leftEdge.transform.parent = transform;
				_leftEdge.transform.localScale = new Vector2(1 / transform.lossyScale.x, 1);
				_leftEdge.transform.localRotation = new Quaternion();
				_leftEdge.transform.localPosition = new Vector2(-0.5f, 0);
			}

			if (RightEdge && RightEdgeObject != null && _rightEdge == null)
			{
				_rightEdge = Instantiate(RightEdgeObject);

				_rightEdge.transform.parent = transform;
				_rightEdge.transform.localScale = new Vector2(1 / transform.lossyScale.x, 1);
				_rightEdge.transform.localRotation = new Quaternion();
				_rightEdge.transform.localPosition = new Vector2(0.5f, 0);
			}
		}
	}
}
