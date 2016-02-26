using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace fireperson.Assets.Scripts
{
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints;
		public float MinimumDistanceToTriggerPointsOnCollision;
		public float DistanceToLowerPoints;
	
		public float RelativeCentre;
		public float RelativeDistanceAboveParent;
		public float RelativeSize;
	
		private PolygonCollider2D _polyCollider;

		void Start()
		{
			_polyCollider = GetComponent<PolygonCollider2D>();
	
			SetPositionToCentreOfAndDistanceAboveParent();
			SetRelativeSize();
			SetPolyColliderPoints();
		}
	
		private void SetPositionToCentreOfAndDistanceAboveParent()
		{
			gameObject.transform.position = new Vector2(
				gameObject.transform.parent.transform.position.x + RelativeCentre,
				gameObject.transform.parent.transform.position.y + RelativeDistanceAboveParent);
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

		void OnTriggerEnter2D(Collider2D coll)
		{
			OnCollisionLowerPointsWithinDistance(coll);
		}

		private void OnCollisionLowerPointsWithinDistance(Collider2D coll)
		{
			List<Vector2> nearestPoints = GetNearestPointsToCentre(coll);	
			nearestPoints.ForEach(point => point.y -= DistanceToLowerPoints);
		}
	
		private List<Vector2> GetNearestPointsToCentre(Collider2D coll)
		{
			var pcCentre = new Vector2(coll.bounds.center.x, coll.bounds.center.y);

			return (from point in _polyCollider.points
				let worldPoint = coll.transform.TransformPoint(point)
				let distance = Vector2.Distance(worldPoint, pcCentre)
				where distance < MinimumDistanceToTriggerPointsOnCollision
				select point).ToList();
		}
	}
}
