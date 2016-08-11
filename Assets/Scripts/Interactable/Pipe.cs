using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class Pipe : FirePlace
	{
		void OnDrawGizmos()
		{
			var col = transform.parent.GetComponentInChildren<CapsuleCollider>();
			col.height = 1 + (col.radius / 3) * 2;
		}
	}
}
