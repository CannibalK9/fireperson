using Assets.Scripts.Interactable;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
	public static class ClimbCollision
	{
		public static bool IsCollisionInvalid(RaycastHit2D[] hits, Transform t)
		{
			ClimbableEdges climbableEdges = t.parent.GetComponent<ClimbableEdges>();

			if (climbableEdges != null)
				return hits.Any(hit =>
				hit.collider.transform != t.parent
				&& hit.collider != climbableEdges.LeftException
				&& hit.collider != climbableEdges.OtherException
				&& hit.collider != climbableEdges.RightException);
			else
				return hits.Any(hit =>
					hit.collider.transform != t);
		}
	}
}
