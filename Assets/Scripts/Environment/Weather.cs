using UnityEngine;

namespace Assets.Scripts.Environment
{
    public class Weather : MonoBehaviour
    {
		public float WindPower;

		void Update()
		{
			WindPower = Random.Range(-5, 5);
		}
    }
}
