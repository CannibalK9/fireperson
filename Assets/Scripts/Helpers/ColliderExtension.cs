using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public static class ColliderExtension
	{
		public static Vector2 GetPoint(this Collider2D col, ColliderPoint point)
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

		public static Vector2 GetTopRight(this Collider2D col)
		{
			return col.bounds.max;
		}

		public static Vector2 GetTopLeft(this Collider2D col)
		{
			return new Vector2(
				col.bounds.min.x,
				col.bounds.max.y);
		}

		public static Vector2 GetBottomRight(this Collider2D col)
		{
			return new Vector2(
				col.bounds.max.x,
				col.bounds.min.y);
		}

		public static Vector2 GetBottomLeft(this Collider2D col)
		{
			return col.bounds.min;
		}

		public static Vector2 GetBottomCenter(this Collider2D col)
		{
			return new Vector2(
				col.bounds.center.x,
				col.bounds.min.y);
		}

		public static Vector2 GetRightFace(this Collider2D col)
		{
			return new Vector2(
				col.bounds.max.x,
				col.bounds.center.y);
		}

		public static Vector2 GetLeftFace(this Collider2D col)
		{
			return new Vector2(
				col.bounds.min.x,
				col.bounds.center.y);
		}

		public static Vector2 GetTopFace(this Collider2D col)
		{
			return new Vector2(
				col.bounds.center.x,
				col.bounds.max.y);
		}

		public static bool CanClimbDown(this Collider2D col)
		{
			return col.name.Contains("less") == false;
		}

		public static bool CanCross(this Collider2D col)
		{
			return col.name.Contains("jumpless") == false;
		}

		public static bool IsCorner(this Collider2D col)
		{
			return col.name.Contains("corner");
		}

		public static bool IsUpright(this Collider2D col)
		{
			return col.name.Contains("upright");
		}
	}
}
