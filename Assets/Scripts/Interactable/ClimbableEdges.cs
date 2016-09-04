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

		public GameObject LeftEdgeObject;
		public GameObject RightEdgeObject;

		public Collider2D Exception;

		private GameObject _leftEdge;
		private GameObject _rightEdge;

		private bool _wasLeft;
		private bool _wasRight;

		private BoxCollider2D _col;
		private readonly float _slopeLimit = ConstantVariables.DefaultPlayerSlopeLimit;
		private Orientation _orientation;

		void Awake()
		{
			_col = gameObject.GetComponent<BoxCollider2D>();
			_wasLeft = LeftEdge;
			_wasRight = RightEdge;
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

			if (_orientation == Orientation.Upright)
			{
				if (IsLeftCorner == false && rotation > 180)
					CreateLeftEdge();
				else if (IsRightCorner == false && rotation < 180)
					CreateRightEdge();
			}

			if (IsLeftCorner)
			{
				if ((_orientation == Orientation.Flat && IsLeftCornerInverted == false)
					|| (_orientation == Orientation.UpsideDown && IsLeftCornerInverted))
					CreateLeftEdge();
			}
			else if (_orientation != Orientation.Upright)
				CreateLeftEdge();

			if (IsRightCorner)
			{
				if ((_orientation == Orientation.Flat && IsRightCornerInverted == false)
					|| (_orientation == Orientation.UpsideDown && IsRightCornerInverted))
					CreateRightEdge();
			}
			else if (_orientation != Orientation.Upright)
				CreateRightEdge();

			transform.rotation = currentRotation;
			transform.parent = currentParent;
		}

		private void CreateLeftEdge()
		{
			if (LeftEdge && _leftEdge == null)
			{
				_leftEdge = _orientation == Orientation.UpsideDown || IsLeftCornerInverted
					? Instantiate(RightEdgeObject)
					: Instantiate(LeftEdgeObject);

				if (IsLeftCorner)
				{
					_leftEdge.name += " corner";
					if (IsLeftCornerInverted)
						_leftEdge.name += " inv";
				}
				else if (_orientation == Orientation.Upright)
					_leftEdge.name += " upright corner";
				if (IsLeftDropless)
					_leftEdge.name += " dropless";

				_leftEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownRight(_col)
					: FlatLeft(_col);

				var col = _leftEdge.GetComponent<BoxCollider2D>();
				col.size = new Vector2(col.size.x, _col.bounds.size.y);
				col.offset = new Vector2(col.offset.x, _orientation == Orientation.UpsideDown ? -1 + col.size.y / 2 : -col.size.y / 2);
				if (_orientation == Orientation.UpsideDown)
					col.offset = new Vector2(-1 - col.offset.x, col.offset.y);

				_leftEdge.transform.parent = transform;
			}
		}

		private void CreateRightEdge()
		{
			if (RightEdge && _rightEdge == null)
			{
				_rightEdge = _orientation == Orientation.UpsideDown || IsRightCornerInverted
					? Instantiate(LeftEdgeObject)
					: Instantiate(RightEdgeObject);

				if (IsRightCorner)
				{
					_rightEdge.name += " corner";
					if (IsRightCornerInverted)
						_rightEdge.name += " inv";
				}
				else if (_orientation == Orientation.Upright)
					_rightEdge.name += " upright corner";
				if (IsRightDropless)
					_rightEdge.name += " dropless";

				_rightEdge.transform.position = _orientation == Orientation.UpsideDown
					? UpsideDownLeft(_col)
					: FlatRight(_col);

				var col = _rightEdge.GetComponent<BoxCollider2D>();
				col.size = new Vector2(col.size.x, _col.bounds.size.y);
				col.offset = new Vector2(col.offset.x, _orientation == Orientation.UpsideDown ? -1 + col.size.y / 2 : -col.size.y / 2);
				if (_orientation == Orientation.UpsideDown)
					col.offset = new Vector2(1 - col.offset.x, col.offset.y);
				_rightEdge.transform.parent = transform;
			}
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
