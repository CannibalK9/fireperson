using UnityEngine;

namespace Assets.Scripts.Rendering
{
	public class FlipWithParent : MonoBehaviour
	{
		void Update()
		{
			if (transform.lossyScale.x < 0)
				transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
		}
	}
}
