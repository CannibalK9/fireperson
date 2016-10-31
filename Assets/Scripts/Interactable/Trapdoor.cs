﻿using Assets.Scripts.Helpers;
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
		public bool LeftPlatformLeftEdge;
		public bool RightPlatformRightEdge;
		public bool IsLeftOnCorner;
		public bool IsRightOnCorner;

		public bool RemainsSolid;
		public float Mass;
		private Transform _hinge;
		private Collider2D _col;

		void Awake()
		{
			_ice = GetComponentsInChildren<Ice.Ice>();
			_hinge = GetComponentsInChildren<Transform>().SingleOrDefault(t => t.name == "hinge");
			_col = GetComponent<Collider2D>();
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

			if (_hinge != null)
			{
				var h = gameObject.AddComponent<HingeJoint2D>();
				h.anchor = _hinge.localPosition;
				Rigidbody2D rbody = null;
				Transform t = transform.parent;
				while (t != null && rbody == null)
				{
					rbody = t.GetComponent<Rigidbody2D>();
					t = t.parent;
				}
				if (rbody != null)
					h.connectedBody = rbody;
			}
		}

		private void AddEdges()
		{
			if (LeftPlatform != null)
				AddEdge(LeftPlatform.GetComponent<ClimbableEdges>(), LeftPlatformLeftEdge);
			if (RightPlatform != null)
				AddEdge(RightPlatform.GetComponent<ClimbableEdges>(), RightPlatformRightEdge == false);
		}

		private void AddEdge(ClimbableEdges edges, bool leftEdge)
		{
			if (leftEdge)
			{
				edges.LeftEdge = true;
				if (IsLeftOnCorner)
					edges.IsLeftCorner = false;
			}
			else
			{
				edges.RightEdge = true;
				if (IsRightOnCorner)
					edges.IsRightCorner = false;
			}
			edges.Reset();
		}
	}
}
