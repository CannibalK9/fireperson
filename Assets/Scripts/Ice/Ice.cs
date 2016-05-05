using System.Collections.Generic;
using Assets.Scripts.Heat;
using UnityEngine;

namespace Assets.Scripts.Ice
{
	[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints = 0.2f;
		public float MinimumDistanceToTriggerPointsOnCollision = 0.4f;
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
			SetPolyColliderPoints();
			_initialPoints = _polyCollider.points;
			SetMeshFilterToPolyColliderPoints();
		}

		private void SetPolyColliderPoints()
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
			_polyCollider.points = newPoints.ToArray();
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
			_polyCollider.points = MovePointsAwayFromOrigin(message);
			SetMeshFilterToPolyColliderPoints();
		}

		//A message arrives at the ice. A single raycasthit and the origin of the cast. The points of the ice that are within the distance to the origin, and that are not
		//on the wrong side of the normal, move away from the origin proportionally to the distance. If moving would break the polycollider then the point does not move

		private Vector2[] MovePointsAwayFromOrigin(HeatMessage message)
		{
			Vector2[] newPoints = _polyCollider.points;

			List<int> allIndices = GetIndicesInRange(message.Origin, message.CastDistance);
            List<int> indices = GetIndicesWithNormalTowardsOrigin(allIndices, message.Origin);

			foreach (int i in indices)
			{
                Vector2 point = Vector2.MoveTowards(transform.TransformPoint(_polyCollider.points[i]), message.Origin, -DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);

				bool tooClose = false;
                //foreach (Vector2 p in _polyCollider.points)
                //{
                //    if (_polyCollider.points[i] != p && Vector2.Distance(transform.TransformPoint(p), point) < 0.03f) //find out how close is too close
                //    {
                //        tooClose = true;
                //        break;
                //    }
                //}
                if (tooClose == false)
                {
                    newPoints[i] = transform.InverseTransformPoint(point);
                }
			}
            _mesh.Clear();
            var vertices = new Vector3[newPoints.Length];
            for (int j = 0; j < vertices.Length; j++)
            {
                vertices[j] = new Vector3(
                    newPoints[j].x,
                    newPoints[j].y,
                    0);
            }
            _mesh.vertices = vertices;
            _mesh.triangles = GetNewMeshFilterTriangles(newPoints);
            _mesh.RecalculateNormals();
            foreach (int i in indices)
            {
                if (Vector2.Angle(_mesh.normals[i], _normalsBeforeMelt[i]) > 180)
                    newPoints[i] = _polyCollider.points[i];
            }

			return newPoints;
		}

		private List<int> GetIndicesInRange(Vector2 origin, float distance)
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

        private List<int> GetIndicesWithNormalTowardsOrigin(List<int> allIndices, Vector2 origin)
        {
            var indices = new List<int>();
            _normalsBeforeMelt = new Dictionary<int, Vector2>();

            foreach (int i in allIndices)
            {
                Vector2 vertex = _mesh.vertices[i];
                Vector2 direction = vertex - origin;
                Vector2 normal = _mesh.normals[i];

                if (Vector2.Angle(direction, normal) < 180)
                {
                    indices.Add(i);
                    _normalsBeforeMelt.Add(i, normal);
                }
            }
            return indices;
        }
	}
}
