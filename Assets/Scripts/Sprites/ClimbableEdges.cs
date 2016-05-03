using UnityEngine;

namespace Assets.Scripts.Sprites
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClimbableEdges : MonoBehaviour
	{
		public bool LeftEdge;
		public bool RightEdge;

		public GameObject LeftEdgeObject;
		public GameObject RightEdgeObject;

		private GameObject _leftEdge;
		private GameObject _rightEdge;

        private BoxCollider2D _col;

		void Awake()
		{
            _col = gameObject.GetComponent<BoxCollider2D>();
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
            Quaternion currentRotation = gameObject.transform.rotation;
            gameObject.transform.rotation = new Quaternion();

            if (LeftEdge && LeftEdgeObject != null && _leftEdge == null)
			{
				_leftEdge = Instantiate(LeftEdgeObject);

				_leftEdge.transform.parent = transform;
                _leftEdge.transform.position = new Vector3(_col.bounds.min.x, _col.bounds.max.y);
            }

            if (RightEdge && RightEdgeObject != null && _rightEdge == null)
			{
				_rightEdge = Instantiate(RightEdgeObject);

				_rightEdge.transform.parent = transform;
                _rightEdge.transform.position = _col.bounds.max;
            }
            gameObject.transform.rotation = currentRotation;
        }
    }
}
