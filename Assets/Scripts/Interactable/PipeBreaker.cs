using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class PipeBreaker : MonoBehaviour
	{
		public FirePlace Fp1;
		public FirePlace Fp2;

		private bool _broken;

		void Update()
		{
			if (_broken == false && Vector2.Distance(Fp1.transform.position, Fp2.transform.position) < 1f)
			{
				Disconnect();
				_broken = true;
			}
		}

		public void Disconnect()
		{
			Fp1.Disconnect(Fp2);
			Fp2.Disconnect(Fp1);
		}
	}
}
