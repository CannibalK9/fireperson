using Assets.Scripts.Helpers;
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
		private const float _slopeLimit = 50f;
		private Orientation _orientation;

		void Awake()
		{
			_col = gameObject.GetComponent<BoxCollider2D>();
		}

		void Update()
		{
			if (LeftEdge == false && _leftEdge != null)
				DestroyObject(_leftEdge);

			if (RightEdge == false && _rightEdge != null)
				DestroyObject(_rightEdge);

			if (_orientation != Orientation.Upright && ((LeftEdge && _leftEdge == null) || (RightEdge && _rightEdge == null)))
				_orientation = Orientation.None;

			if (LeftEdge || RightEdge)
			{
				Orientation currentOrientation = _orientation;
				float rotation = transform.rotation.eulerAngles.z;

				if (rotation < _slopeLimit || rotation > 360f - _slopeLimit)
					_orientation = Orientation.Flat;
				else if (rotation < 180f + _slopeLimit && rotation > 180f - _slopeLimit)
					_orientation = Orientation.UpsideDown;
				else
					_orientation = Orientation.Upright;

				if (currentOrientation != _orientation)
				{
					DeactiveEdges();
					CreateEdges(rotation);
				}
			}
		}

		private void DeactiveEdges()
		{
			DestroyObject(_leftEdge);
			DestroyObject(_rightEdge);

			_leftEdge = null;
			_rightEdge = null;
		}

		private void CreateEdges(float rotation)
		{
			Quaternion currentRotation = transform.rotation;
			Transform currentParent = transform.parent;
			transform.parent = null;
			transform.rotation = new Quaternion();

			if (LeftEdgeObject != null && LeftEdgeObject.GetComponent<Collider2D>().IsCorner())
			{
				if (_orientation == Orientation.Flat)
					CreateLeftEdge();
			}

			if (RightEdgeObject != null && RightEdgeObject.GetComponent<Collider2D>().IsCorner())
			{
				if (_orientation == Orientation.Flat)
					CreateRightEdge();
			}

			if (_orientation == Orientation.Upright)
			{
				if (rotation > 180 && CreateLeftEdge())
					_leftEdge.name = Orientation.Upright.ToString();
				else if (CreateRightEdge())
					_rightEdge.name = Orientation.Upright.ToString();
			}
			else
			{
				CreateLeftEdge();
				CreateRightEdge();
			}
			
			transform.rotation = currentRotation;
			transform.parent = currentParent;
		}

		private bool CreateLeftEdge()
		{
			if (LeftEdge && LeftEdgeObject != null && _leftEdge == null)
			{
				_leftEdge = Instantiate(LeftEdgeObject);
				_leftEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownLeft(_col)
					: FlatLeft(_col);
				_leftEdge.transform.parent = transform;
				return true;
			}
			return false;
		}

		private bool CreateRightEdge()
		{
			if (RightEdge && RightEdgeObject != null && _rightEdge == null)
			{
				_rightEdge = Instantiate(RightEdgeObject);

				_rightEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownRight(_col)
					: FlatRight(_col);
				_rightEdge.transform.parent = transform;
				return true;
			}
			return false;
		}

		private Vector2 FlatLeft(Collider2D col)
		{
			return new Vector2(col.bounds.min.x, col.bounds.max.y);
		}

		private Vector2 FlatRight(Collider2D col)
		{
			return col.bounds.max;
		}

		private Vector2 UpsideDownLeft(Collider2D col)
		{
			return new Vector2(col.bounds.max.x - 1, col.bounds.min.y + 0.5f);
		}

		private Vector2 UpsideDownRight(Collider2D col)
		{
			return new Vector2(col.bounds.min.x + 1, col.bounds.min.y + 0.5f);
		}
	}
}
