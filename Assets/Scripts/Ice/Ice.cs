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
			_mesh.triangles = GetNewMeshFilterTriangles();
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

		private int[] GetNewMeshFilterTriangles()
		{
			var tr = new Triangulator(_polyCollider.points);
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

            int a = _polyCollider.shapeCount;

			int nearestIndex = GetNearestIndex(message.Origin);
			Vector2 nearestWorldPoint = transform.TransformPoint(_polyCollider.points[nearestIndex]);
			int numberOfPointsToMove = Mathf.RoundToInt(message.CastDistance / Vector2.Distance(nearestWorldPoint, message.Origin)); //may need recalibrating

			foreach (int i in GetIndecesToMove(nearestIndex, numberOfPointsToMove))
			{
				int index = i;
				if (i < 0)
					index = _polyCollider.points.Length + i;
				else if (i >= _polyCollider.points.Length)
					index = i - _polyCollider.points.Length;

                Vector2 point = Vector2.MoveTowards(transform.TransformPoint(_polyCollider.points[index]), message.Origin, -DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);

				bool tooClose = false;
                foreach (Vector2 p in _polyCollider.points)
                {
                    if (_polyCollider.points[index] != p && Vector2.Distance(transform.TransformPoint(p), point) < 0.03f) //find out how close is too close
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose == false)
                {
                    newPoints[index] = transform.InverseTransformPoint(point);
                }
			}

			return newPoints;
		}

		private int GetNearestIndex(Vector2 origin)
		{
			Vector2 nearestPoint = _polyCollider.points[0];
			int nearestIndex = 0;

			for (int i = 1; i < _polyCollider.points.Length; i++)
			{
				Vector2 point = _polyCollider.points[i];
				Vector2 worldPoint = transform.TransformPoint(point);

				if (Vector2.Distance(worldPoint, origin) < Vector2.Distance(transform.TransformPoint(nearestPoint), origin))
				{
					nearestPoint = point;
					nearestIndex = i;
				}
			}

			return nearestIndex;
		}

		private IEnumerable<int> GetIndecesToMove(int nearestIndex, int numberOfPointsToMove)
		{
			var indexes = new List<int>();
			for (int i = 0; i <= numberOfPointsToMove; i++)
			{
				indexes.Add(nearestIndex + i);
				indexes.Add(nearestIndex - i);
			}
			return indexes;
		}
	}
}
