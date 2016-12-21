using Assets.Scripts.Helpers;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player.Climbing
{
	public static class EdgeValidator
	{
		private static int _rightClimbLayer = LayerMask.NameToLayer(Layers.RightClimbSpot);
		private static int _leftClimbLayer = LayerMask.NameToLayer(Layers.LeftClimbSpot);

		public static bool CanJumpToHang(Collider2D col, Vector2 playerCentre, BoxCollider2D playerCol)
		{
			return CanHang(col, playerCol) && IsEdgeUnblocked(col, playerCentre, playerCol, false) && IsEdgeLip(col) == false && IsUprightAccessible(col, playerCentre, playerCol) && IsHangingSpace(col);
		}

		public static bool CanClimbUpOrDown(Collider2D col, BoxCollider2D playerCol)
		{
			return IsSpaceOffEdge(col, playerCol) && IsSpaceAboveEdge(col, playerCol);
		}

		public static bool CanHang(Collider2D col, BoxCollider2D playerCol)
		{
			return IsSpaceBelowEdge(col, playerCol);
		}

		public static bool CanJumpToOrFromEdge(Collider2D col, Vector2 playerCentre, BoxCollider2D playerCol)
		{
			return IsEdgeUnblocked(col, playerCentre, playerCol, true, false) && IsEdgeLip(col) == false;
		}

		public static bool CanMantle(Collider2D col, Vector2 playerCentre, BoxCollider2D playerCol)
		{
			return IsUprightAccessible(col, playerCentre, playerCol) && IsEdgeUnblocked(col, playerCentre, playerCol, true);
		}

		private static bool IsEdgeLip(Collider2D col)
		{
			bool isLeft = col.transform.gameObject.layer == _leftClimbLayer;
			RaycastHit2D hit = Physics2D.Raycast((isLeft ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(isLeft ? -0.3f : 0.3f, 0),
				OrientationHelper.GetDownwardVector(col.transform), ConstantVariables.MaxLipHeight, Layers.Platforms);

			return hit;
		}

		private static bool IsSpaceAboveEdge(Collider2D col, BoxCollider2D playerCol)
		{
			RaycastHit2D hit = Physics2D.Raycast((col.transform.gameObject.layer == _leftClimbLayer ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(0, 0.1f, 0), Vector2.up, playerCol.size.y, Layers.Platforms);
			return hit == false;
		}

		private static bool IsSpaceBelowEdge(Collider2D col, BoxCollider2D playerCol)
		{
			bool isLeft = col.transform.gameObject.layer == _leftClimbLayer;
			RaycastHit2D[] hits = Physics2D.BoxCastAll((isLeft ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(isLeft ? -playerCol.size.x / 2 : playerCol.size.x / 2, 0),
				new Vector2(playerCol.size.x - 0.2f, 0.1f), 0, col.IsCorner() ? OrientationHelper.GetDownwardVector(col.transform) : Vector3.down, playerCol.size.y, Layers.Platforms);

			return hits.Any(hit => hit.collider.transform != col.transform.parent) == false;
		}

		private static bool IsSpaceOffEdge(Collider2D col, BoxCollider2D playerCol)
		{
			bool isLeft = col.transform.gameObject.layer == _leftClimbLayer;
			RaycastHit2D[] hits = Physics2D.BoxCastAll((isLeft ? col.GetTopLeft() : col.GetTopRight()) + new Vector3(isLeft ? -playerCol.size.x / 2 : playerCol.size.x / 2, 0),
				new Vector2(playerCol.size.x, 0.1f), 0, Vector2.up, playerCol.size.y, Layers.Platforms);

			return ClimbCollision.IsCollisionInvalid(hits, col.transform) == false;
		}

		private static bool IsEdgeUnblocked(Collider2D originalHit, Vector2 playerCentre, BoxCollider2D playerCol, bool above, bool allowExceptions = true)
		{
            Vector2 origin = playerCentre;
			Vector2 target = originalHit.bounds.center;
			Vector2 direction = target - origin;

			RaycastHit2D obstacleHit = Physics2D.Raycast(origin, direction, Vector2.Distance(target, origin), Layers.Platforms);
			Debug.DrawRay(origin, direction, Color.red);

			if (originalHit.transform.parent != obstacleHit.collider.transform && Vector2.Distance(obstacleHit.point, target) > 0.2f)
				return false;

			float yOffset = above ? playerCol.size.y /2 : -playerCol.size.y / 2;
            target = originalHit.GetTopFace() + new Vector3(0, yOffset);
			direction = target - origin;
			float angle = above || originalHit.IsUpright() == false ? 0 : originalHit.transform.rotation.eulerAngles.z;

			RaycastHit2D[] obstacleHits = Physics2D.BoxCastAll(origin, new Vector2(0.01f, playerCol.size.y - 0.1f), angle, direction, Vector2.Distance(origin, target), Layers.Platforms);

            foreach (var hit in obstacleHits)
            {
                if (allowExceptions)
                {
                    if (originalHit.transform.parent == hit.collider.transform
						|| Vector2.Distance(originalHit.transform.position, hit.point) < 1
						|| ClimbCollision.IsCollisionInvalid(new RaycastHit2D[] { hit }, originalHit.transform) == false)
							continue;
                    else
                        return false;
                }
                else if (originalHit.transform.parent == hit.collider.transform
						|| Vector2.Distance(originalHit.transform.position, hit.point) < 1)
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

		private static bool IsUprightAccessible(Collider2D col, Vector3 playerCentre, BoxCollider2D playerCol)
		{
			if (col.IsUpright() == false)
				return true;

			Vector2 playerPosition = col.bounds.center - playerCentre;

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
