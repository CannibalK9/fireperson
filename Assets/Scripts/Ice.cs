using System;
using UnityEngine;

namespace fireperson.Assets.Scripts
{
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints = 0.5f;
        public float MinimumDistanceToTriggerPointsOnCollision = 0.85f;
		public float DistanceToLowerPoints = 0.1f;
	
		public float RelativeCentre;
		public float RelativeDistanceAboveParent;
		public float RelativeSize;
	
		private PolygonCollider2D _polyCollider;
        private Mesh _mesh;


        void Start()
		{
			_polyCollider = GetComponent<PolygonCollider2D>();
            _mesh = GetComponent<MeshFilter>().mesh;

            SetPositionToCentreAboveInFrontOfParent();
			SetRelativeSize();
			SetPolyColliderPoints();
            SetMeshFilterToPolyColliderPoints();
		}
	
		private void SetPositionToCentreAboveInFrontOfParent()
		{
			gameObject.transform.position = new Vector3(
				gameObject.transform.parent.transform.position.x + RelativeCentre,
				gameObject.transform.parent.transform.position.y + RelativeDistanceAboveParent,
                -0.1f);
		}

		private void SetRelativeSize()
		{
			gameObject.transform.localScale = new Vector2(RelativeSize, 1);
		}

		private void SetPolyColliderPoints()
		{
			float boundaryWidth = _polyCollider.bounds.size.x;
			float relativeDistanceBetweenPoints = ActualDistanceBetweenPoints / boundaryWidth;
			int pointCount = Convert.ToInt32(1 / relativeDistanceBetweenPoints) + 3;

			_polyCollider.points = GetPointsEvenlySpacedOnNorthOfRectangle(relativeDistanceBetweenPoints, pointCount);
		}

		private Vector2[] GetPointsEvenlySpacedOnNorthOfRectangle(float northSideIncrement, int pointCount)
		{
			const float max = 0.5f;
			const float min = -0.5f;
			var points = new Vector2[pointCount];

			for (int i = 0; i < pointCount - 3; i++)
			{
				points[i] = new Vector2(min + northSideIncrement * i, max);
			}

			points[pointCount - 3] = new Vector2(max, max);
			points[pointCount - 2] = new Vector2(max, min);
			points[pointCount - 1] = new Vector2(min, min);

			return points;
		}

		void Update()
		{
		}

        void OnTriggerStay2D(Collider2D coll)
        {
            OnCollisionLowerPointsWithinDistance(coll);
            SetMeshFilterToPolyColliderPoints();
        }

        private void OnCollisionLowerPointsWithinDistance(Collider2D coll)
		{
			Vector2[] newPoints = LowerNearestPointsToCentre(coll);
            _polyCollider.points = newPoints;
        }

        private Vector2[] LowerNearestPointsToCentre(Collider2D coll)
        {
            var pcCentre = new Vector2(coll.transform.position.x, coll.transform.position.y);
            Vector2[] newPoints = _polyCollider.points;

            for (int i = 0; i < _polyCollider.points.Length; i++)
            {
                Vector2 point = _polyCollider.points[i];
                if (point.y <= 0)
                    continue;

                Vector2 worldPoint = transform.TransformPoint(point);
                float distance = Vector2.Distance(worldPoint, pcCentre);
                if (distance < MinimumDistanceToTriggerPointsOnCollision)
                {
                    newPoints[i] = new Vector2(point.x, point.y - DistanceToLowerPoints);
                }
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
            Vector3[] vertices = new Vector3[_polyCollider.GetTotalPointCount()];
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
            Triangulator tr = new Triangulator(_polyCollider.points);
            return tr.Triangulate();
        }
	}
}
