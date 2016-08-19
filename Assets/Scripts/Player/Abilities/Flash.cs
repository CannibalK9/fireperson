using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class Flash : MonoBehaviour
	{
		private CircleCollider2D _col;
		private float _flashDuration = 0f;

		void Awake()
		{
			_col = GetComponent<CircleCollider2D>();
			_col.radius = ConstantVariables.FlashRadius;
		}

		void Update()
		{
			_col.enabled = _flashDuration > 0;
			_flashDuration -= Time.deltaTime;
		}

		public void OnFlash()
		{
			_flashDuration = 0.5f;
		}
	}
}
