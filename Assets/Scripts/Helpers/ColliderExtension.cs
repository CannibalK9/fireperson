using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public static class ColliderExtension
	{
		public static Vector3 GetPoint(this Collider2D col, ColliderPoint point)
		{
			switch (point)
			{
				case ColliderPoint.Centre:
					return col.transform.position;
				case ColliderPoint.TopLeft:
					return col.GetTopLeft();
				case ColliderPoint.TopRight:
					return col.GetTopRight();
				case ColliderPoint.BottomLeft:
					return col.GetBottomLeft();
				case ColliderPoint.BottomRight:
					return col.GetBottomRight();
				case ColliderPoint.TopFace:
					return col.GetTopFace();
				case ColliderPoint.LeftFace:
					return col.GetLeftFace();
				case ColliderPoint.RightFace:
					return col.GetRightFace();
				case ColliderPoint.BottomFace:
					return col.GetBottomCenter();
				default:
					return col.transform.position;
			}
		}

		public static Vector3 GetTopRight(this Collider2D col)
		{
			return col.bounds.max;
		}

		public static Vector3 GetTopLeft(this Collider2D col)
		{
			return new Vector3(
				col.bounds.min.x,
				col.bounds.max.y);
		}

		public static Vector3 GetBottomRight(this Collider2D col)
		{
			return new Vector3(
				col.bounds.max.x,
				col.bounds.min.y);
		}

		public static Vector3 GetBottomLeft(this Collider2D col)
		{
			return col.bounds.min;
		}

		public static Vector3 GetBottomCenter(this Collider2D col)
		{
			return new Vector3(
				col.bounds.center.x,
				col.bounds.min.y);
		}

		public static Vector3 GetRightFace(this Collider2D col)
		{
			return new Vector3(
				col.bounds.max.x,
				col.bounds.center.y);
		}

		public static Vector3 GetLeftFace(this Collider2D col)
		{
			return new Vector3(
				col.bounds.min.x,
				col.bounds.center.y);
		}

		public static Vector3 GetTopFace(this Collider2D col)
		{
			return new Vector3(
				col.bounds.center.x,
				col.bounds.max.y);
		}

		public static bool CanClimbDown(this Collider2D col)
		{
			return col.name.Contains("dropless") == false;
		}

		public static bool IsCorner(this Collider2D col)
		{
			return col.name.Contains("corner");
		}

		public static bool IsUpright(this Collider2D col)
		{
			return col.name.Contains("upright");
		}

        public static bool IsCorner(this Transform trans)
        {
            return trans.name.Contains("corner");
        }

        public static bool IsUpright(this Transform trans)
        {
            return trans.name.Contains("upright");
        }
    }
}
