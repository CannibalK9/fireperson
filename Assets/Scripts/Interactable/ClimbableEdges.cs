using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class ClimbableEdges : MonoBehaviour
	{
		public bool LeftEdge;
		public bool IsLeftCorner;
		public bool IsLeftCornerInverted;
		public bool IsLeftDropless;

		public bool RightEdge;
		public bool IsRightCorner;
		public bool IsRightCornerInverted;
		public bool IsRightDropless;

        private GameObject _leftEdgeObject;
		private GameObject _rightEdgeObject;

		public Collider2D Exception;
		public Collider2D Exception2;

		private GameObject _leftEdge;
		private GameObject _rightEdge;

		private bool _wasLeft;
		private bool _wasRight;

		private BoxCollider2D _col;
		private Orientation _orientation;

		void Awake()
		{
			_col = gameObject.GetComponent<BoxCollider2D>();
			_wasLeft = LeftEdge;
			_wasRight = RightEdge;

            _leftEdgeObject = Resources.Load("edges/left edge") as GameObject;
            _rightEdgeObject = Resources.Load("edges/right edge") as GameObject;
        }

        void Update()
		{
			if (LeftEdge != _wasLeft || RightEdge != _wasRight)
			{
				_orientation = Orientation.None;
				DeactiveEdges();
				_wasLeft = LeftEdge;
				_wasRight = RightEdge;
			}
			if (LeftEdge || RightEdge)
			{
				Orientation currentOrientation = _orientation;
				float rotation = transform.rotation.eulerAngles.z;
                _orientation = OrientationHelper.GetOrientation(rotation);

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

			if (_orientation == Orientation.UprightAntiClockwise)
			{
                if (IsRightCorner == false)
					CreateRightUpright();
			}
            else if (_orientation == Orientation.UprightClockwise)
            {
                if (IsLeftCorner == false)
                    CreateLeftUpright();
            }
			else
			{
				if (IsLeftCorner)
				{
					if ((_orientation == Orientation.Flat && IsLeftCornerInverted == false)
						|| (_orientation == Orientation.UpsideDown && IsLeftCornerInverted))
						CreateLeftEdge();
				}
				else
					CreateLeftEdge();

				if (IsRightCorner)
				{
					if ((_orientation == Orientation.Flat && IsRightCornerInverted == false)
						|| (_orientation == Orientation.UpsideDown && IsRightCornerInverted))
						CreateRightEdge();
				}
				else
					CreateRightEdge();
			}
			transform.rotation = currentRotation;
			transform.parent = currentParent;
		}

		private void CreateLeftUpright()
		{
			if (LeftEdge && _leftEdge == null)
			{
				InstantiateUprightObjects();

				_leftEdge.transform.position = UpsideDownRight(_col);
				var col = _leftEdge.GetComponent<BoxCollider2D>();
				col.offset = new Vector2(-1 + col.offset.x, -1 - col.offset.y);
				_leftEdge.transform.parent = transform;

				_rightEdge.transform.position = FlatLeft(_col);
				var col2 = _rightEdge.GetComponent<BoxCollider2D>();
				col2.offset = new Vector2(-col2.offset.x, col2.offset.y);
				_rightEdge.transform.parent = transform;
			}
		}

		private void CreateRightUpright()
		{
			if (RightEdge && _rightEdge == null)
			{
				InstantiateUprightObjects();

				_leftEdge.transform.position = FlatRight(_col);
				var col = _leftEdge.GetComponent<BoxCollider2D>();
				col.offset = new Vector2(-col.offset.x, col.offset.y);
				_leftEdge.transform.parent = transform;

				_rightEdge.transform.position = UpsideDownLeft(_col);
				var col2 = _rightEdge.GetComponent<BoxCollider2D>();
				col2.offset = new Vector2(1 + col2.offset.x, -1 - col2.offset.y);
				_rightEdge.transform.parent = transform;
			}
		}

		private void InstantiateUprightObjects()
		{
			_leftEdge = Instantiate(_leftEdgeObject);
			_rightEdge = Instantiate(_rightEdgeObject);

			_leftEdge.name += " upright corner";
			_rightEdge.name += " upright corner";
		}

		private void CreateLeftEdge()
		{
			if (LeftEdge && _leftEdge == null)
			{
				_leftEdge = _orientation == Orientation.UpsideDown || IsLeftCornerInverted
					? Instantiate(_rightEdgeObject)
					: Instantiate(_leftEdgeObject);

				if (IsLeftCorner)
				{
					_leftEdge.name += " corner";
					if (IsLeftCornerInverted)
						_leftEdge.name += " inv";
				}

				if (IsLeftDropless)
					_leftEdge.name += " dropless";

				SetLeftEdgeTransform();
			}
		}

		private void SetLeftEdgeTransform()
		{
			_leftEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownRight(_col)
					: FlatLeft(_col);

			var col = _leftEdge.GetComponent<BoxCollider2D>();
			if (_orientation == Orientation.UpsideDown)
				col.offset = new Vector2(-1 - col.offset.x, -1 - col.offset.y);

			_leftEdge.transform.parent = transform;
		}

		private void CreateRightEdge()
		{
			if (RightEdge && _rightEdge == null)
			{
				_rightEdge = _orientation == Orientation.UpsideDown || IsRightCornerInverted
					? Instantiate(_leftEdgeObject)
					: Instantiate(_rightEdgeObject);

				if (IsRightCorner)
				{
					_rightEdge.name += " corner";
					if (IsRightCornerInverted)
						_rightEdge.name += " inv";
				}

				if (IsRightDropless)
					_rightEdge.name += " dropless";

				SetRightEdgeTransform();
			}
		}

		private void SetRightEdgeTransform()
		{
			_rightEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownLeft(_col)
					: FlatRight(_col);

			var col = _rightEdge.GetComponent<BoxCollider2D>();
			if (_orientation == Orientation.UpsideDown)
				col.offset = new Vector2(1 - col.offset.x, -1 - col.offset.y);
			_rightEdge.transform.parent = transform;
		}

		private static Vector2 FlatLeft(Collider2D col)
		{
			return new Vector2(col.bounds.min.x, col.bounds.max.y);
		}

		private static Vector2 FlatRight(Collider2D col)
		{
			return col.bounds.max;
		}

		private static Vector2 UpsideDownLeft(Collider2D col)
		{
			return new Vector2(col.bounds.max.x - 1, col.bounds.min.y + 1);
		}

		private static Vector2 UpsideDownRight(Collider2D col)
		{
			return new Vector2(col.bounds.min.x + 1, col.bounds.min.y + 1);
		}
	}
}
