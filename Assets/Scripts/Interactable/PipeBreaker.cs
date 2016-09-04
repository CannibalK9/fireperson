using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class PipeBreaker : MonoBehaviour
	{
		public FirePlace Fp1;
		public FirePlace Fp2;

		public void Disconnect()
		{
			Fp1.IsAccessible = true;
			Fp2.IsAccessible = true;

			FirePlace f1 = Fp1.GetConnectedFireplaces().Single(f => f == Fp2);
			f1 = null;

			FirePlace f2 = Fp2.GetConnectedFireplaces().Single(f => f == Fp1);
			f2 = null;
		}
	}
}
