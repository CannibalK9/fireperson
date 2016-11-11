using Assets.Scripts.Helpers;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public static class EdgeValidator
	{
		private static int _rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
		private static int _leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);

		public static bool CanJumpToHang(Collider2D col, Bounds bounds)
		{
			return IsEdgeUnblocked(col, bounds, false) && IsUprightAccessible(col, bounds) && IsSpaceBelowEdge(col, bounds) && IsHangingSpace(col);
		}

		public static bool CanClimbUpOrDown(Collider2D col, Bounds bounds)
		{
			return IsSpaceOffEdge(col, bounds) && IsSpaceAboveEdge(col, bounds);
		}

		public static bool CanHang(Collider2D col, Bounds bounds)
		{
			return IsSpaceBelowEdge(col, bounds);
		}

		public static bool CanJumpToOrFromEdge(Collider2D col, Bounds bounds)
		{
			return IsEdgeUnblocked(col, bounds, true, false) && IsSpaceAboveEdge(col, bounds) && IsSpaceOffEdge(col, bounds);
		}

		public static bool CanMantle(Collider2D col, Bounds bounds)
		{
			return IsUprightAccessible(col, bounds) && IsEdgeUnblocked(col, bounds, true) && IsSpaceAboveEdge(col, bounds);
		}

		private static bool IsSpaceAboveEdge(Collider2D col, Bounds bounds)
		{
			RaycastHit2D hit = Physics2D.Raycast((col.transform.gameObject.layer == _leftClimbLayer ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(0, 0.1f, 0), Vector2.up, bounds.size.y, Layers.Platforms);
			return hit == false;
		}

		private static bool IsSpaceBelowEdge(Collider2D col, Bounds bounds)
		{
			bool isLeft = col.transform.gameObject.layer == _leftClimbLayer;
			RaycastHit2D[] hits = Physics2D.BoxCastAll((isLeft ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(isLeft ? -bounds.extents.x : bounds.extents.x, 0),
				new Vector2(bounds.size.x - 0.2f, 0.1f), 0, col.IsCorner() ? OrientationHelper.GetDownwardVector(col.transform) : Vector3.down, bounds.size.y, Layers.Platforms);

			return hits.Any(hit => hit.collider.transform != col.transform.parent) == false;
		}

		private static bool IsSpaceOffEdge(Collider2D col, Bounds bounds)
		{
			bool isLeft = col.transform.gameObject.layer == _leftClimbLayer;
			RaycastHit2D[] hits = Physics2D.BoxCastAll((isLeft ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(isLeft ? -bounds.extents.x : bounds.extents.x, 0),
				new Vector2(bounds.size.x, 0.1f), 0, Vector2.up, bounds.size.y, Layers.Platforms);

			return ClimbCollision.IsCollisionInvalid(hits, col.transform) == false;
		}

		private static bool IsEdgeUnblocked(Collider2D originalHit, Bounds bounds, bool above, bool allowExceptions = true)
		{
            float offset = above ? bounds.extents.y : -bounds.extents.y;
            Vector2 origin = bounds.center;
            Vector2 target = originalHit.GetTopFace() + new Vector3(0, offset);
			Vector2 direction = target - origin;

			RaycastHit2D[] obstacleHits = Physics2D.BoxCastAll(origin, new Vector2(0.01f, bounds.size.y - 0.1f), 0, direction, Vector2.Distance(origin, target), Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

            foreach (var hit in obstacleHits)
            {
                if (allowExceptions)
                {
                    if (originalHit.transform.parent == hit.collider.transform
                    || ClimbCollision.IsCollisionInvalid(new RaycastHit2D[] { hit }, originalHit.transform) == false)
                        continue;
                    else
                        return false;
                }
                else if (originalHit.transform.parent == hit.collider.transform)
                    continue;
                else
                    return false;
            }
            return true;
        }

		private static bool IsHangingSpace(Collider2D originalHit)
		{
			return Physics2D.Raycast(originalHit.GetTopFace() + new Vector3(0, 0.1f), Vector2.up, ConstantVariables.FingerHoldSpace, Layers.Platforms) == false;
		}

		private static bool IsUprightAccessible(Collider2D col, Bounds bounds)
		{
			if (col.IsUpright() == false)
				return true;

			Vector2 playerPosition = col.bounds.center - bounds.center;

			float angleDirection = AngleDir(OrientationHelper.GetDownwardVector(col.transform), playerPosition);

			bool onRight = angleDirection > 0;

			return ((col.transform.gameObject.layer == LayerMask.NameToLayer(Layers.RightClimbSpot) && onRight)
				|| (col.transform.gameObject.layer == LayerMask.NameToLayer(Layers.LeftClimbSpot)) && !onRight);
		}

		public static float AngleDir(Vector2 line, Vector2 point)
		{
			return -line.x * point.y + line.y * point.x;
		}
	}
}
