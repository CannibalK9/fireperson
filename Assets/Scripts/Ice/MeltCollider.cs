using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Abilities;
using Destructible2D;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Ice
{
	public class MeltCollider : MonoBehaviour
	{
		private EdgeCollider2D[] _edges;
		private Texture2D _stampTex;
		public float StampSize = 0.3f;
		private float _timer;
		private GameObject _steam;
		private float _nextMelt = 1;

		void Awake()
		{
			_steam = (GameObject)Resources.Load("particles/melting");
			_stampTex = (Texture2D)Resources.Load("terrain/Ice/tinystamp");
			//Mesh mesh = new Mesh();
			//SetMeshFilterToPolyColliderPoints(mesh);
			//var tex = new Texture2D(mesh.bounds.size.x, mesh.bounds.size.y);
			//mesh.
			//var d2d = GetComponent<D2dDestructible>();
			//var d2dEdge = GetComponent<D2dEdgeCollider>();
			//var tex = new Texture2D(1,1);
			//d2d.MainTex = tex;
			//d2d.ResetAlpha();
			//d2dEdge.Regenerate();
		}

		private void BuildTexture()
		{
			Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);

			// fill texture with random color
			for (int x = 0; x < texture.width; x++)
			{
				for (int y = 0; y < texture.height; y++)
				{
					texture.SetPixel(x, y, Random.Range(0, 2) == 0 ? Color.red : Color.blue);
				}
			}
			texture.Apply();
		}

		private void SetMeshFilterToPolyColliderPoints(Mesh mesh)
		{
			Vector2[] points = GetComponent<EdgeCollider2D>().points;
			mesh.Clear();
			mesh.vertices = GetNewMeshFilterVertices(points);
			mesh.triangles = GetNewMeshFilterTriangles(points);
			mesh.RecalculateNormals();
		}

		private Vector3[] GetNewMeshFilterVertices(Vector2[] points)
		{
			var vertices = new Vector3[points.Length];
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j] = new Vector3(
					points[j].x,
					points[j].y,
					0);
			}
			return vertices;
		}

		private static int[] GetNewMeshFilterTriangles(Vector2[] points)
		{
			var tr = new Triangulator(points);
			return tr.Triangulate();
		}

		void Update()
		{
			if (_timer > 0)
				_timer += Time.deltaTime;

			if (_timer > _nextMelt)
				_timer = 0;
		}

		void OnTriggerStay2D(Collider2D col)
		{
			if (_timer == 0 && col.gameObject.layer == LayerMask.NameToLayer(Layers.Heat))
			{
				_timer += Time.deltaTime;
				_nextMelt = 1 - (col.gameObject.GetComponent<HeatHandler>().HeatMessage.Intensity / 100);
				OnMelt(col);
			}
		}

		private IEnumerable<Vector2> GetPointsInRange(Collider2D col)
		{
			var indices = new List<Vector2>();
			_edges = GetComponents<EdgeCollider2D>();

			foreach (var edge in _edges)
			{
				for (int i = 0; i < edge.points.Length; i++)
				{
					Vector3 worldPoint = transform.TransformPoint(edge.points[i]);

					if (col.OverlapPoint(worldPoint))
					{
						if (col.gameObject.GetComponent<Tether>() == null)
						{
							RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, worldPoint - col.bounds.center, 20f, Layers.Platforms | 1 << LayerMask.NameToLayer(Layers.BackgroundIce));
							if (Vector2.Distance(worldPoint, hit.point) < 0.1f)
								indices.Add(worldPoint);
						}
						else
							indices.Add(worldPoint);
					}
				}
			}
			return indices;
		}

		private void OnMelt(Collider2D col)
		{
			IEnumerable<Vector2> points = GetPointsInRange(col);
			foreach (var point in points)
			{
				Instantiate(_steam, point, new Quaternion());
				D2dDestructible.StampAll(point, Vector2.one, 0, _stampTex, 1, 1 << LayerMask.NameToLayer(Layers.Ice) | 1 << LayerMask.NameToLayer(Layers.BackgroundIce));
			}
		}
	}
}
