using UnityEngine;

namespace Assets.Scripts.Sprites
{
	public class StiltDestruction : MonoBehaviour
	{
		public GameObject AttachedBuilding;

		public void DestroyStilt()
		{
			if (AttachedBuilding != null)
			{
				Joint2D[] joints = AttachedBuilding.GetComponents<Joint2D>();
				foreach (Joint2D joint in joints)
				{
					joint.enabled = false;
				}
			}
			gameObject.GetComponent<HingeJoint2D>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
			gameObject.layer = LayerMask.NameToLayer("Background");
		}
	}
}
