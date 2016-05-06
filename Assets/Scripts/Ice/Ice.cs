using System.Collections.Generic;
using Assets.Scripts.Heat;
using UnityEngine;

namespace Assets.Scripts.Ice
{
	[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints = 2f;
		public float DefaultDistanceToLowerPoints = 0.1f;

		public bool GrowsBack;

		private PolygonCollider2D _polyCollider;
		private Mesh _mesh;
		private Vector2[] _initialPoints;
		private Dictionary<int, Vector2> _normalsBeforeMelt;

		public Vector2[] GetCurrentPoints()
		{
			return _polyCollider.points;
		}

		void Awake()
		{
			_polyCollider = GetComponent<PolygonCollider2D>();
			_mesh = GetComponent<MeshFilter>().mesh;
		}

		void Start()
		{
			_polyCollider.points = GetColliderPointsAtIntervals(ActualDistanceBetweenPoints);
			_initialPoints = _polyCollider.points;
			SetMeshFilterToPolyColliderPoints();
		}

		private Vector2[] GetColliderPointsAtIntervals(float interval)
		{
			var newPoints = new List<Vector2>();

			for (int i = 0; i < _polyCollider.points.Length; i++)
			{
				Vector2 start = _polyCollider.points[i];
				Vector2 end = i + 1 == _polyCollider.points.Length ? _polyCollider.points[0] : _polyCollider.points[i + 1];

				float distanceBetweenPoints = ActualDistanceBetweenPoints / Vector2.Distance(start, end);
				while (Vector2.Distance(start, end) > distanceBetweenPoints)
				{
					newPoints.Add(start);
					start = Vector2.MoveTowards(start, end, distanceBetweenPoints);
				}
			}
			return newPoints.ToArray();
		}

		private void SetMeshFilterToPolyColliderPoints()
		{
			_mesh.Clear();
			_mesh.vertices = GetNewMeshFilterVertices();
			_mesh.triangles = GetNewMeshFilterTriangles(_polyCollider.points);
			_mesh.RecalculateNormals();
		}

		private Vector3[] GetNewMeshFilterVertices()
		{
			var vertices = new Vector3[_polyCollider.GetTotalPointCount()];
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j] = new Vector3(
					_polyCollider.points[j].x,
					_polyCollider.points[j].y,
					0);
			}
			return vertices;
		}

		private int[] GetNewMeshFilterTriangles(Vector2[] points)
		{
			var tr = new Triangulator(points);
			return tr.Triangulate();
		}

		void Update()
		{
			if (GrowsBack)
			{
				_polyCollider.points = RaisePoints();
				SetMeshFilterToPolyColliderPoints();
			}
		}

		private Vector2[] RaisePoints()
		{
			Vector2[] newPoints = _polyCollider.points;
			for (int i = 0; i < _polyCollider.points.Length; i++)
			{
				Vector2 point = _polyCollider.points[i];
				Vector2 initialPoint = _initialPoints[i];

				if (point != initialPoint)
				{
					point = Vector2.MoveTowards(point, initialPoint, UnityEngine.Random.value / 100);
				}
				newPoints[i] = point;
			}
			return newPoints;
		}

		void Melt(HeatMessage message)
		{
            var newPoints = MovePointsInwards(message);
            for (int i = 0; i < _polyCollider.points.Length; i++)
            {
                if (_polyCollider.OverlapPoint(transform.TransformPoint(newPoints[i])) == false)
                    newPoints[i] = _polyCollider.points[i];
            }
            _polyCollider.points = newPoints;

            SetMeshFilterToPolyColliderPoints();
		}

		//A message arrives at the ice. A single raycasthit and the origin of the cast. The points of the ice that are within the distance to the origin, and that are not
		//on the wrong side of the normal, move away from the origin proportionally to the distance. If moving would break the polycollider then the point does not move

		private Vector2[] MovePointsInwards(HeatMessage message)
		{
			Vector2[] newPoints = _polyCollider.points;
			IEnumerable<int> allIndices = GetIndicesInRange(message.Origin, message.CastDistance);

            foreach (int i in allIndices)
            {
                int beforeIndex = i == 0 ? _polyCollider.points.Length - 1 : i - 1;
                int afterIndex = i == _polyCollider.points.Length - 1 ? 0 : i + 1;

                Vector2 point = _polyCollider.points[i];
                Vector2 beforePoint = _polyCollider.points[beforeIndex];
                Vector2 afterPoint = _polyCollider.points[afterIndex];

                Vector2 centre = Vector2.Lerp(beforePoint, afterPoint, 0.5f);

                Vector2 direction = _polyCollider.OverlapPoint(transform.TransformPoint(centre))
                    ? centre - point
                    : point - centre;

                Vector2 newPoint = Vector2.MoveTowards(point, direction.normalized*10, DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);

                newPoints[i] = newPoint;
            }
			return newPoints;
		}

		private IEnumerable<int> GetIndicesInRange(Vector2 origin, float distance)
		{
			var indices = new List<int>();

			for (int i = 0; i < _polyCollider.points.Length; i++)
			{
				Vector2 worldPoint = transform.TransformPoint(_polyCollider.points[i]);

				if (Vector2.Distance(worldPoint, origin) < distance)
					indices.Add(i);
			}

			return indices;
		}
	}
}
