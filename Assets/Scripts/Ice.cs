using UnityEngine;
using System;

public class Ice : MonoBehaviour
{
    public float ActualDistanceBetweenPoints;
    public float RelativeCentre;
    public float RelativeDistanceAboveParent;
    public float RelativeSize;

	void Start()
    {
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
        var polyCollider = GetComponent<PolygonCollider2D>();

        float boundaryWidth = polyCollider.bounds.size.x;
        float relativeDistanceBetweenPoints = ActualDistanceBetweenPoints / boundaryWidth;
        int pointCount = Convert.ToInt32(1 / relativeDistanceBetweenPoints) + 3;

        polyCollider.points = GetPointsEvenlySpacedOnNorthOfRectangle(relativeDistanceBetweenPoints, pointCount);
    }

    private Vector2[] GetPointsEvenlySpacedOnNorthOfRectangle(float northSideIncrement, int pointCount)
    {
        float max = 0.5f;
        float min = -0.5f;
        Vector2[] points = new Vector2[pointCount];

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
        OnCollisionLowerNearestPoint(coll);
    }

    private void OnCollisionLowerNearestPoint(Collider2D coll)
    {
        if (coll.gameObject.tag == "Player")
        {
            Debug.Log("touched!");
        }
    }
}
