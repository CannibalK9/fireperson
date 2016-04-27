using UnityEngine;

namespace Assets.Scripts.Movement
{
	public static class ColliderExtension
	{
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

		public static bool CanClimbDown(this Collider2D col)
		{
			return col.name.Contains("less") == false;
		}

		public static bool CanCross(this Collider2D col)
		{
			return col.name.Contains("jumpless") == false;
		}

		public static bool CanSwing(this Collider2D col)
		{
			return col.name.Contains("corner") == false;
		}
	}
}
