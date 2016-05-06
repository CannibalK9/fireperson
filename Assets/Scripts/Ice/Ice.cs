using System.Collections.Generic;
using Assets.Scripts.Heat;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Ice
{
	[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints = 2f;
		public float DefaultDistanceToLowerPoints = 0.1f;

		public bool GrowsBack;
        public bool AnyJointEnabled { get; set; }

		private PolygonCollider2D _polyCollider;
		private Mesh _mesh;
		private Vector2[] _initialPoints;
		private Vector2[] _newPoints;
		private Dictionary<int, Vector2> _normalsBeforeMelt;
        private Joint2D[] _joints;

		void Awake()
		{
			_polyCollider = GetComponent<PolygonCollider2D>();
			_mesh = GetComponent<MeshFilter>().mesh;
            _joints = GetComponents<FixedJoint2D>();
            AnyJointEnabled = true;
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
            foreach (FixedJoint2D joint in _joints)
            {
                if (_polyCollider.OverlapPoint(transform.TransformPoint(joint.anchor)) == false)
                    joint.enabled = false;
            }

            if (_joints.Length > 0 && _joints.Where(j => j.enabled == true).Any() == false)
                AnyJointEnabled = false;

			if (_polyCollider.bounds.size.x < 0.5 || _polyCollider.bounds.size.y < 0.5)
			{
				_polyCollider.enabled = false;
			}
			else
			{
				_polyCollider.enabled = true;
			}

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
			_polyCollider.points = MovePointsInwards(message);
			SetMeshFilterToPolyColliderPoints();
		}

		//A message arrives at the ice. A single raycasthit and the origin of the cast. The points of the ice that are within the distance to the origin, and that are not
		//on the wrong side of the normal, move away from the origin proportionally to the distance. If moving would break the polycollider then the point does not move

		private Vector2[] MovePointsInwards(HeatMessage message)
		{
			_newPoints = _polyCollider.points;
			List<int> allIndices = GetIndicesInRange(message.Origin, message.CastDistance);

			foreach (int i in allIndices)
			{
				int beforeIndex = GetBeforeIndex(i);
				int afterIndex = GetAfterIndex(i);

				Vector2 point = _polyCollider.points[i];
				Vector2 beforePoint = _polyCollider.points[beforeIndex];
				Vector2 afterPoint = _polyCollider.points[afterIndex];

				Vector2 centre = Vector2.Lerp(beforePoint, afterPoint, 0.5f);

				Vector2 direction = _polyCollider.OverlapPoint(transform.TransformPoint(centre))
					? centre - point
					: point - centre;

				Vector2 newPoint = Vector2.MoveTowards(point, direction.normalized*10, DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);

				_newPoints[i] = newPoint;
			}

            for (int i = 0; i < _polyCollider.points.Length; i++)
            {
                if (_polyCollider.OverlapPoint(transform.TransformPoint(_newPoints[i])) == false)
                {
                    allIndices.Remove(i);
                    _newPoints[i] = _polyCollider.points[i];
                }
            }

			foreach (int i in allIndices)
			{
				int currentIndex = i;
                int count = 0;
				while (AcuteAngleFlattened(currentIndex, GetBeforeIndex(currentIndex), GetAfterIndex(currentIndex)) && count < _polyCollider.points.Length)
				{
					currentIndex = GetBeforeIndex(currentIndex);
                    count++;
				}

				currentIndex = GetAfterIndex(i);
                count = 0;
                while (AcuteAngleFlattened(currentIndex, GetBeforeIndex(currentIndex), GetAfterIndex(currentIndex)) && count < _polyCollider.points.Length)
				{
					currentIndex = GetAfterIndex(currentIndex);
                    count++;
				}
			}

			return _newPoints;
		}

		private bool AcuteAngleFlattened(int currentIndex, int beforeIndex, int afterIndex)
		{
            Vector2 point = _newPoints[currentIndex];
            Vector2 beforePoint = _newPoints[beforeIndex];
            Vector2 afterPoint = _newPoints[afterIndex];
			Vector2 centre = Vector2.Lerp(beforePoint, afterPoint, 0.5f);

			if (Vector2.Angle(beforePoint - point, afterPoint - point) < 90)
			{
				Vector2 direction = centre - point;
				_newPoints[currentIndex] = Vector2.MoveTowards(_newPoints[currentIndex], direction.normalized * 10, DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);
				return true;
			}
			return false;
		}

		private int GetBeforeIndex(int currentIndex)
		{
			return currentIndex == 0 ? _polyCollider.points.Length - 1 : currentIndex - 1;			
		}

		private int GetAfterIndex(int currentIndex)
		{
			return currentIndex == _polyCollider.points.Length - 1 ? 0 : currentIndex + 1;
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
	}
}
