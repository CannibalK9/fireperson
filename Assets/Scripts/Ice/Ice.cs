#define DEBUG_CC2D_RAYS
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Heat;
using Assets.Scripts.Helpers;
using UnityEngine;
using Assets.Scripts.Player.Abilities;
using Destructible2D;

namespace Assets.Scripts.Ice
{
	public class Ice : MonoBehaviour
	{
		public float ActualDistanceBetweenPoints = 0.2f;

		public bool GrowsBack;
		public bool ExtraPoints;
		public Texture2D StampTex;
		public bool IsAnchored { get; set; }
		public float StampSize;

		//private D2dPolygonCollider _col;
		private EdgeCollider2D[] _polyCollider;
		private Mesh _mesh;
		private Vector2[] _initialPoints;
		private Transform[] _anchors;
		private MeshRenderer _meshRenderer;
		private MeshCollider _meshCollider;
		private bool _meltedThisFrame;

		void Awake()
		{
			_polyCollider = GetComponents<EdgeCollider2D>();
			//_col = GetComponent<D2dPolygonCollider>();
			//_mesh = GetComponentInChildren<MeshFilter>().mesh;
			//_meshRenderer = GetComponentInChildren<MeshRenderer>();
			//_meshCollider = GetComponentInChildren<MeshCollider>();
			_anchors = GetComponentsInChildren<Transform>().Where(t => t.tag == "anchor").ToArray();
			IsAnchored = true;
		}

		void Start()
		{
			if (_polyCollider == null)
				_polyCollider = GetComponents<EdgeCollider2D>();
			//if (ExtraPoints)
				//_polyCollider.points = GetColliderPointsAtIntervals(ActualDistanceBetweenPoints);
			//_initialPoints = _polyCollider.points;
			//SetMeshFilterToPolyColliderPoints();
		}

		//private Vector2[] GetColliderPointsAtIntervals(float interval)
		//{
		//	var newPoints = new List<Vector2>();

		//	for (int i = 0; i < _polyCollider.points.Length; i++)
		//	{
		//		Vector2 start = _polyCollider.points[i];
		//		Vector2 end = i + 1 == _polyCollider.points.Length ? _polyCollider.points[0] : _polyCollider.points[i + 1];

		//		float distanceBetweenPoints = interval / Vector2.Distance(start, end);
		//		do
		//		{
		//			newPoints.Add(start);
		//			start = Vector2.MoveTowards(start, end, distanceBetweenPoints);
		//		}
		//		while (Vector2.Distance(start, end) > distanceBetweenPoints);
		//	}
		//	return newPoints.ToArray();
		//}

		//private void SetMeshFilterToPolyColliderPoints()
		//{
		//	_mesh.Clear();
		//	_mesh.vertices = GetNewMeshFilterVertices();
		//	_mesh.triangles = GetNewMeshFilterTriangles(_polyCollider.points);
		//	_mesh.RecalculateNormals();
		//}

		//private Vector3[] GetNewMeshFilterVertices()
		//{
		//	var vertices = new Vector3[1];//_polyCollider.GetTotalPointCount()];
		//	for (int j = 0; j < vertices.Length; j++)
		//	{
		//		vertices[j] = new Vector3(
		//			_polyCollider.points[j].x,
		//			_polyCollider.points[j].y,
		//			0);
		//	}
		//	return vertices;
		//}

		private static int[] GetNewMeshFilterTriangles(Vector2[] points)
		{
			var tr = new Triangulator(points);
			return tr.Triangulate();
		}

		void Update()
		{
			foreach (Transform anchor in _anchors)
			{
				//if (_polyCollider.OverlapPoint(anchor.position) == false)
					//anchor.gameObject.SetActive(false);
			}

			//IsAnchored = _anchors.Any(j => j.gameObject.activeSelf);

			//_polyCollider.enabled = true;
			//_meshRenderer.enabled = true;

			//if (_polyCollider.bounds.size.x < 0.3 || _polyCollider.bounds.size.y < 0.3)
			{
				//_polyCollider.enabled = false;
				//_meshRenderer.enabled = false;
			}

			//if (GrowsBack && _meltedThisFrame == false)
			{
				//_polyCollider.points = RaisePoints();
				//SetMeshFilterToPolyColliderPoints();
			}

			//_meltedThisFrame = false;
		}

		//private Vector2[] RaisePoints()
		//{
		//	Vector2[] newPoints = _polyCollider.points;
		//	for (int i = 0; i < _polyCollider.points.Length; i++)
		//	{
		//		Vector2 point = _polyCollider.points[i];
		//		Vector2 initialPoint = _initialPoints[i];

		//		if (point != initialPoint)
		//		{
		//			point = Vector2.MoveTowards(point, initialPoint, Random.value / 100);
		//		}
		//		newPoints[i] = point;
		//	}
		//	return newPoints;
		//}

		void OnTriggerStay2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.Heat))
			{
				//_meltedThisFrame = true;
				Melt(col.gameObject.GetComponent<HeatHandler>().HeatMessage, col);
			}
		}

		private float timer;

		private void Melt(HeatMessage message, Collider2D col)
		{

			if (timer > 0.25f)
				timer = 0;


			//if (Random.value < message.Intensity / 100)
			if (timer == 0)
			{
				IEnumerable<Vector2> points = GetPointsInRange(col);
				foreach (var point in points)
				{
					D2dDestructible.StampAll(point, Vector2.one * StampSize, 0, StampTex, 1, 1 << LayerMask.NameToLayer(Layers.Ice));

				}


				//_polyCollider.points = MovePointsInwards(col);
				//_polyCollider.points = FlattenAngles();
				//SetMeshFilterToPolyColliderPoints();
			}
				timer += Time.deltaTime;
		}

		//A message arrives at the ice. A single raycasthit and the origin of the cast. The points of the ice that are within the distance to the origin, and that are not
		//on the wrong side of the normal, move away from the origin proportionally to the distance. If moving would break the polycollider then the point does not move

		//private Vector2[] MovePointsInwards(Collider2D col)
		//{
		//	float distanceToMove = 0.01f;
		//	Vector2[] newPoints = _polyCollider.points;
		//	IEnumerable<int> allIndices = new List<int>();// GetPointsInRange(col);

		//	foreach (int i in allIndices)
		//	{
		//		int beforeIndex = GetBeforeIndex(i);
		//		int afterIndex = GetAfterIndex(i);

		//		Vector2 point = transform.TransformPoint(_polyCollider.points[i]);
		//		Vector2 beforePoint = transform.TransformPoint(_polyCollider.points[beforeIndex]);
		//		Vector2 afterPoint = transform.TransformPoint(_polyCollider.points[afterIndex]);

		//		Vector2 offSet = beforePoint - afterPoint;
		//		Vector2 perpendicular = new Vector2(-offSet.y, offSet.x) / (offSet.magnitude * 100f);

		//		Vector2 newPoint = point + (perpendicular.normalized * (distanceToMove));
		//		if (_polyCollider.OverlapPoint(newPoint))
		//		{
		//			newPoints[i] = transform.InverseTransformPoint(newPoint);
		//		}
		//		else
		//		{
		//			Vector2 perpendicular2 = new Vector2(-offSet.y, offSet.x) / (offSet.magnitude * -100f);
		//			newPoint = point + (perpendicular2.normalized * (distanceToMove));
		//			if (_polyCollider.OverlapPoint(newPoint))
		//				newPoints[i] = transform.InverseTransformPoint(newPoint);
		//			else
  //                  {
  //                      // some complex splitter something _polyCollider.
		//				newPoints[i] = transform.InverseTransformPoint(Vector2.Lerp(beforePoint, afterPoint, 0.5f));
  //                  }
		//		}
		//	}
		//	return newPoints;
		//}

  //      private Vector2[] FlattenAngles()
		//{
		//	Vector2[] newPoints = _polyCollider.points;
		//	int currentIndex = 0;
		//	int count = 0;
		//	while (count < _polyCollider.points.Length)
		//	{
		//		newPoints[currentIndex] = AcuteAngleFlattened(
		//			newPoints[currentIndex],
		//			newPoints[GetBeforeIndex(currentIndex)],
		//			newPoints[GetAfterIndex(currentIndex)]);
		//		currentIndex = GetBeforeIndex(currentIndex);
		//		count++;
		//	}
		//	return newPoints;
		//}

		//private Vector2 AcuteAngleFlattened(Vector2 point, Vector2 beforePoint, Vector2 afterPoint)
		//{
		//	Vector2 centre = Vector2.Lerp(beforePoint, afterPoint, 0.5f);
		//	float angle = Angle(beforePoint, afterPoint, point);

		//	while (angle < 150 && _meshRenderer.enabled)
		//	{
		//		point = Vector2.MoveTowards(point, centre, 100f);
		//		angle = Angle(beforePoint, afterPoint, point);
		//	}
		//	return point;
		//}

		//private static float Angle(Vector2 beforePoint, Vector2 afterPoint, Vector2 point)
		//{
		//	return Vector2.Angle(beforePoint - point, afterPoint - point);
		//}

		//private int GetBeforeIndex(int currentIndex)
		//{
		//	return currentIndex == 0 ? _polyCollider.points.Length - 1 : currentIndex - 1;			
		//}

		//private int GetAfterIndex(int currentIndex)
		//{
		//	return currentIndex == _polyCollider.points.Length - 1 ? 0 : currentIndex + 1;
		//}

		private IEnumerable<Vector2> GetPointsInRange(Collider2D col)
		{
			var indices = new List<Vector2>();
				_polyCollider = GetComponents<EdgeCollider2D>();

			foreach (var edge in _polyCollider)
			{

				for (int i = 0; i < edge.points.Length; i++)
				{
					Vector3 worldPoint = transform.TransformPoint(edge.points[i]);

					if (col.OverlapPoint(worldPoint))
					{
						if (col.gameObject.GetComponent<Tether>() == null)
						{
							RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, worldPoint - col.bounds.center, 20f, Layers.Platforms);
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

		void OnTriggerEnter(Collider col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.Background))
			{
				GrowsBack = false;
			}
		}

		[System.Diagnostics.Conditional("DEBUG_CC2D_RAYS")]
		void DrawRay(Vector3 start, Vector3 dir, Color color)
		{
			Debug.DrawRay(start, dir, color);
		}
	}
}
