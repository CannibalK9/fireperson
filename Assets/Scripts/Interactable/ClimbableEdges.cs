using UnityEngine;

namespace Assets.Scripts.Interactable
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
		private const float _slopeLimit = 70f;

		void Awake()
		{
			_col = gameObject.GetComponent<BoxCollider2D>();
		}

		void Start()
		{
			CreateEdges();
		}

		void Update()
		{
			if (_leftEdge != null)
			{
				if ((_leftEdge.name.Contains("corner") && _leftEdge.transform.rotation.eulerAngles.z > 90f - _slopeLimit)
					|| (_leftEdge.transform.rotation.eulerAngles.z > _slopeLimit && _leftEdge.transform.rotation.eulerAngles.z < 360f - _slopeLimit)
					_leftEdge.SetActive(false);
				else
					_leftEdge.SetActive(true);
			}

			if (_rightEdge != null)
			{
				if ((_rightEdge.name.Contains("corner") && _rightEdge.transform.rotation.eulerAngles.z > 90f - _slopeLimit)
					|| (_rightEdge.transform.rotation.eulerAngles.z > _slopeLimit && _rightEdge.transform.rotation.eulerAngles.z < 360f - _slopeLimit)
					_rightEdge.SetActive(false);
				else
					_rightEdge.SetActive(true);
			}
		}

		public void ActivateEdges()
		{
			LeftEdge = true;
			RightEdge = true;

			CreateEdges();
		}

		public void DeactiveEdges()
		{
			LeftEdge = false;
			RightEdge = false;

			Destroy(_leftEdge);
			Destroy(_rightEdge);
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
