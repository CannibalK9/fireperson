using Assets.Scripts.Interactable;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class CircleHeaterUnlit : HeatHandler
	{
		private CircleCollider2D _col;

		void Awake()
		{
			_fireplace = GetComponentInParent<FirePlace>();
			_col = GetComponent<CircleCollider2D>();
			EnableCollider(_fireplace == null);
		}

		protected override void SetColliderSizes(float range)
		{
			transform.localScale = Vector3.one * (HeatMessage.HeatRange);
		}

		protected override void EnableCollider(bool enable)
		{
			_col.enabled = enable;
		}
	}
}
