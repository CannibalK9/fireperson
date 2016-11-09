﻿using Assets.Scripts.Helpers;
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

			if (_orientation == Orientation.UprightAntiClockwise)
			{
                bool createLeftEdge = IsRightCorner == false || (IsRightCorner && IsRightCornerInverted == false);
                bool createRightEdge = IsRightCorner == false || (IsRightCorner && IsRightCornerInverted);
                CreateRightUpright(createLeftEdge, createRightEdge);
			}
            else if (_orientation == Orientation.UprightClockwise)
            {
                bool createLeftEdge = IsLeftCorner == false || (IsLeftCorner && IsLeftCornerInverted);
                bool createRightEdge = IsLeftCorner == false || (IsLeftCorner && IsLeftCornerInverted == false);
                CreateLeftUpright(createLeftEdge, createRightEdge);
            }
			else
			{
				if (IsLeftCorner == false)
				{
					CreateLeftEdge();
				}

				if (IsRightCorner == false)
				{
					CreateRightEdge();
				}
			}
			transform.rotation = currentRotation;
			transform.parent = currentParent;
		}

        private void CreateLeft(bool isUpright)
        {
            _leftEdge = Instantiate(_leftEdgeObject);

            switch (_orientation)
            {
                case Orientation.Flat:
                case Orientation.LeftTilt:
                case Orientation.RightTilt:
                case Orientation.UprightClockwise:
                    if (IsLeftCorner)
                        _leftEdge.name += " corner";
                    if (IsLeftDropless)
                        _leftEdge.name += " dropless";
                    break;
                case Orientation.UpsideDown:
                case Orientation.UpsideDownLeftTilt:
                case Orientation.UpsideDownRightTilt:
                case Orientation.UprightAntiClockwise:
                    if (IsRightCorner)
                        _leftEdge.name += " corner";
                    if (IsRightDropless)
                        _leftEdge.name += " dropless";
                    break;
            }

            if (isUpright)
            {
                _leftEdge.name += " upright";
                //rotate accordingly
            }
        }

        private void CreateRight(bool isUpright)
        {

        }

		private void CreateLeftUpright(bool createLeftEdge, bool createRightEdge)
		{
            if (LeftEdge)
            {
                InstantiateUprightObjects(createLeftEdge, createRightEdge);

                if (_leftEdge != null)
                {
                    _leftEdge.transform.position = UpsideDownRight(_col);
                    var col = _leftEdge.GetComponent<BoxCollider2D>();
                    col.offset = new Vector2(-1 + col.offset.x, -1 - col.offset.y);
                    _leftEdge.transform.parent = transform;
                }

                if (_rightEdge != null)
                {
                    _rightEdge.transform.position = FlatLeft(_col);
                    var col2 = _rightEdge.GetComponent<BoxCollider2D>();
                    col2.offset = new Vector2(-col2.offset.x, col2.offset.y);
                    _rightEdge.transform.parent = transform;
                }
            }
		}

		private void CreateRightUpright(bool createLeftEdge, bool createRightEdge)
		{
            if (RightEdge)
            {
                InstantiateUprightObjects(createLeftEdge, createRightEdge);

                if (_leftEdge != null)
                {
                    _leftEdge.transform.position = FlatRight(_col);
                    var col = _leftEdge.GetComponent<BoxCollider2D>();
                    col.offset = new Vector2(-col.offset.x, col.offset.y);
                    _leftEdge.transform.parent = transform;
                }

                if (_rightEdge != null)
                {
                    _rightEdge.transform.position = UpsideDownLeft(_col);
                    var col2 = _rightEdge.GetComponent<BoxCollider2D>();
                    col2.offset = new Vector2(1 + col2.offset.x, -1 - col2.offset.y);
                    _rightEdge.transform.parent = transform;
                }
            }
		}

		private void InstantiateUprightObjects(bool createLeftEdge, bool createRightEdge)
		{
            if (createLeftEdge)
            {
			    _leftEdge = Instantiate(_leftEdgeObject);
                _leftEdge.name += " upright";
                if (IsLeftDropless)
                    _leftEdge.name += " dropless";
                if (createLeftEdge != createRightEdge)
                    _leftEdge.name += " corner";
            }

            if (createRightEdge)
            {
                _rightEdge = Instantiate(_rightEdgeObject);
                _rightEdge.name += " upright";
                if (IsRightDropless)
                    _rightEdge.name += " dropless";
                if (createLeftEdge != createRightEdge)
                    _rightEdge.name += " corner";
            }
		}

		private void CreateLeftEdge()
		{
			if (LeftEdge && _leftEdge == null)
			{
				_leftEdge = _orientation == Orientation.UpsideDown || IsLeftCornerInverted
					? Instantiate(_rightEdgeObject)
					: Instantiate(_leftEdgeObject);

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
