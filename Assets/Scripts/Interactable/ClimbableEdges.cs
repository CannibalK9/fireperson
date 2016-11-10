using Assets.Scripts.Helpers;
using System.Linq;
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

		public Collider2D LeftException;
		public Collider2D RightException;
		public Collider2D OtherException;

		private GameObject _leftEdge;
		private GameObject _rightEdge;

        private bool[] _oldBools;

		private BoxCollider2D _col;
		private Orientation _orientation;

		void Awake()
		{
			_col = gameObject.GetComponent<BoxCollider2D>();
            _oldBools = GetBoolArray();
        }

		void Start()
		{
			_leftEdgeObject = Resources.Load("terrain/edges/left edge") as GameObject;
			_rightEdgeObject = Resources.Load("terrain/edges/right edge") as GameObject;
		}

        void Update()
		{
			if (GetBoolArray().SequenceEqual(_oldBools) == false)
			{
				_orientation = Orientation.None;
				DeactiveEdges();
                _oldBools = GetBoolArray();
			}
			if (LeftEdge || RightEdge)
			{
				Orientation currentOrientation = _orientation;
                _orientation = OrientationHelper.GetOrientation(transform);

				if (currentOrientation != _orientation)
				{
					DeactiveEdges();
					CreateEdges();
				}
			}
		}

		private bool[] GetBoolArray()
		{
			return new bool[] { LeftEdge, IsLeftCorner, IsLeftCornerInverted, IsLeftDropless, RightEdge, IsRightCorner, IsRightCornerInverted, IsRightDropless };
        }

		private void DeactiveEdges()
		{
			DestroyObject(_leftEdge);
			DestroyObject(_rightEdge);

			_leftEdge = null;
			_rightEdge = null;
		}

		private void CreateEdges()
		{
			Quaternion currentRotation = transform.rotation;
			Transform currentParent = transform.parent;
			transform.parent = null;
			transform.rotation = new Quaternion();

			CreateLeft();
			CreateRight();

			transform.rotation = currentRotation;
			transform.parent = currentParent;
		}

        private void CreateLeft()
        {
			bool shouldCreateLeft = false;
			bool isUpright = false;
			bool isCorner = false;
			bool isDropless = false;
			Rotation rotation = Rotation.None;
			Corner corner = Corner.TopLeft;

            switch (_orientation)
            {
                case Orientation.Flat:
				case Orientation.RightTilt:
					shouldCreateLeft = LeftEdge && IsLeftCorner == false;
					isDropless = IsLeftDropless;
					break;
                case Orientation.LeftTilt:
					corner = Corner.BottomLeft;
					rotation = Rotation.Left;
					shouldCreateLeft = LeftEdge && (IsLeftCorner == false || IsLeftCornerInverted);
					isDropless = IsLeftDropless;
					break;
                case Orientation.UprightAntiClockwise:
					corner = Corner.BottomLeft;
					rotation = Rotation.Left;
					isCorner = IsLeftCorner;
					shouldCreateLeft = LeftEdge && (IsLeftCorner == false || IsLeftCornerInverted);
					isDropless = IsLeftDropless;
					isUpright = true;
					break;
				case Orientation.UpsideDown:
				case Orientation.UpsideDownRightTilt:
					shouldCreateLeft = RightEdge && IsRightCorner == false;
					isDropless = IsRightDropless;
					corner = Corner.BottomRight;
					rotation = Rotation.About;
					break;
				case Orientation.UpsideDownLeftTilt:
					corner = Corner.TopRight;
					rotation = Rotation.Right;
					shouldCreateLeft = RightEdge && (IsRightCorner == false || IsRightCornerInverted == false);
					isDropless = IsRightDropless;
					break;
				case Orientation.UprightClockwise:
					corner = Corner.TopRight;
					rotation = Rotation.Right;
					isCorner = IsRightCorner;
					shouldCreateLeft = RightEdge && (IsRightCorner == false || IsRightCornerInverted == false);
					isDropless = IsRightDropless;
					isUpright = true;
					break;
            }

			if (shouldCreateLeft == false)
				return;

			_leftEdge = Instantiate(_leftEdgeObject);

			if (isUpright)
				_leftEdge.name += " upright";
			if (isCorner)
				_leftEdge.name += " corner";
			if (isDropless)
				_leftEdge.name += " dropless";

			SetRotation(_leftEdge, rotation);
			SetPosition(_leftEdge, corner);
        }

        private void CreateRight()
		{
			bool shouldCreateRight = false;
			bool isUpright = false;
			bool isCorner = false;
			bool isDropless = false;
			Rotation rotation = Rotation.None;
			Corner corner = Corner.TopRight;

			switch (_orientation)
			{
				case Orientation.Flat:
				case Orientation.LeftTilt:
					shouldCreateRight = RightEdge && IsRightCorner == false;
					isDropless = IsRightDropless;
					break;
				case Orientation.RightTilt:
					corner = Corner.BottomRight;
					rotation = Rotation.Right;
					shouldCreateRight = RightEdge && (IsRightCorner == false || IsRightCornerInverted);
					isDropless = IsRightDropless;
					break;
				case Orientation.UprightClockwise:
					corner = Corner.BottomRight;
					rotation = Rotation.Right;
					isCorner = IsRightCorner;
					shouldCreateRight = RightEdge && (IsRightCorner == false || IsRightCornerInverted);
					isDropless = IsRightDropless;
					isUpright = true;
					break;
				case Orientation.UpsideDown:
				case Orientation.UpsideDownLeftTilt:
					shouldCreateRight = LeftEdge && IsLeftCorner == false;
					isDropless = IsLeftDropless;
					corner = Corner.BottomLeft;
					rotation = Rotation.About;
					break;
				case Orientation.UpsideDownRightTilt:
					corner = Corner.TopLeft;
					rotation = Rotation.Left;
					shouldCreateRight = LeftEdge && (IsLeftCorner == false || IsLeftCornerInverted == false);
					isDropless = IsLeftDropless;
					break;
				case Orientation.UprightAntiClockwise:
					corner = Corner.TopLeft;
					rotation = Rotation.Left;
					isCorner = IsLeftCorner;
					shouldCreateRight = LeftEdge && (IsLeftCorner == false || IsLeftCornerInverted == false);
					isDropless = IsLeftDropless;
					isUpright = true;
					break;
			}

			if (shouldCreateRight == false)
				return;

			_rightEdge = Instantiate(_rightEdgeObject);

			if (isUpright)
				_rightEdge.name += " upright";
			if (isCorner)
				_rightEdge.name += " corner";
			if (isDropless)
				_rightEdge.name += " dropless";

			SetRotation(_rightEdge, rotation);
			SetPosition(_rightEdge, corner);
		}

		private enum Corner
		{
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight
		}

		private enum Rotation
		{
			None,
			Left,
			Right,
			About
		}

		private void SetRotation(GameObject edge, Rotation rotation)
		{
			switch (rotation)
			{
				case Rotation.Left:
					edge.transform.Rotate(0, 0, 90);
					break;
				case Rotation.Right:
					edge.transform.Rotate(0, 0, -90);
					break;
				case Rotation.About:
					edge.transform.Rotate(0, 0, 180);
					break;
				case Rotation.None:
				default:
					break;
			}
		}

		private void SetPosition(GameObject edge, Corner corner)
		{
			switch (corner)
			{
				case Corner.TopLeft:
					SetTopLeft(edge);
					break;
				case Corner.TopRight:
					SetTopRight(edge);
					break;
				case Corner.BottomLeft:
					SetBottomLeft(edge);
					break;
				case Corner.BottomRight:
					SetBottomRight(edge);
					break;
			}

			edge.transform.parent = transform;
		}

		private void SetTopRight(GameObject edge)
		{
			edge.transform.position = TopRight(_col);
		}

		private void SetBottomRight(GameObject edge)
		{
			edge.transform.position = BottomRight(_col);
		}

		private void SetTopLeft(GameObject edge)
		{
			edge.transform.position = TopLeft(_col);
		}

		private void SetBottomLeft(GameObject edge)
		{
			edge.transform.position = BottomLeft(_col);
		}

		private static Vector2 TopLeft(Collider2D col)
		{
			return new Vector2(col.bounds.min.x, col.bounds.max.y);
		}

		private static Vector2 TopRight(Collider2D col)
		{
			return new Vector2(col.bounds.max.x, col.bounds.max.y);
		}

		private static Vector2 BottomRight(Collider2D col)
		{
			return new Vector2(col.bounds.max.x, col.bounds.min.y);
		}

		private static Vector2 BottomLeft(Collider2D col)
		{
			return new Vector2(col.bounds.min.x, col.bounds.min.y);
		}
	}
}
