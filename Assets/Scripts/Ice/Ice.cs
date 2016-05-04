using System;
using System.Collections.Generic;
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

		void Melt(Vector4 hitBox)
		{
			OnCollisionLowerPointsWithinBox(hitBox);
			SetMeshFilterToPolyColliderPoints();
		}

		private void OnCollisionLowerPointsWithinBox(Vector4 hitBox)
		{
			_polyCollider.points = MovePointsToCentre(hitBox);
        }

        private Vector2[] MovePointsToCentre(Vector4 hitBox)
        {
            Vector2[] newPoints = _polyCollider.points;
            Vector2 centrePoint = _polyCollider.bounds.center;

            for (int i = 0; i < _polyCollider.points.Length; i++)
            {
                Vector2 point = _polyCollider.points[i];
                Vector2 worldPoint = transform.TransformPoint(point);

                if (hitBox.w == 0)
                {
                    float distance = Vector2.Distance(worldPoint, hitBox);
                    if (distance >= hitBox.z)
                        continue;
                }
                else
                {
                    var difference = new Vector2(worldPoint.x - hitBox.x, worldPoint.y - hitBox.y);
                    if (Mathf.Abs(difference.x) >= hitBox.z || Mathf.Abs(difference.y) >= hitBox.w)
                        continue;
                }
                point = Vector2.MoveTowards(point, centrePoint, DefaultDistanceToLowerPoints);// - UnityEngine.Random.value / 10);
                newPoints[i] = point;
            }
            return newPoints;
        }

        private Vector2[] GetPointsLoweredByBox(Vector4 hitBox)
		{
			Vector2[] newPoints = _polyCollider.points;

			for (int i = 0; i < _polyCollider.points.Length; i++)
			{
				Vector2 point = _polyCollider.points[i];
				if (point.y <= -0.3f)
				{
					continue;
				}

				Vector2 worldPoint = transform.TransformPoint(point);

				if (hitBox.w == 0)
				{
					float distance = Vector2.Distance(worldPoint, hitBox);
					if (distance >= hitBox.z)
						continue;
				}
				else
				{
					var difference = new Vector2(worldPoint.x - hitBox.x, worldPoint.y - hitBox.y);
					if (Mathf.Abs(difference.x) >= hitBox.z || Mathf.Abs(difference.y) >= hitBox.w)
						continue;
				}
				newPoints[i] = new Vector2(point.x, point.y - DefaultDistanceToLowerPoints - UnityEngine.Random.value / 10);
			}
			return newPoints;
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
	}
}
