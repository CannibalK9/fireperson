using UnityEngine;

namespace Assets.Scripts.CameraHandler
{
	public class SmoothCamera2D : MonoBehaviour
	{
		public float DampTime = 0.15f;
		public Transform Target;

		private Vector3 _velocity = Vector3.zero;

		void Update()
		{
            if (Target != null)
			    transform.position = Vector3.SmoothDamp(transform.position, new Vector3(Target.position.x, Target.position.y, -12f), ref _velocity, DampTime);
		}
	}
}