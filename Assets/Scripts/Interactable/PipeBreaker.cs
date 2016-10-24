using Assets.Scripts.Helpers;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
	public class PipeBreaker : MonoBehaviour
	{
		private FirePlace _fireplace;
		private CircleCollider2D _thisCol;

		void Awake()
		{
			_thisCol = GetComponent<CircleCollider2D>();
			_fireplace = GetComponent<FirePlace>();
			_fireplace.IsAccessible = true;
		}

		void OnTriggerStay2D(Collider2D col)
		{
			if (col.gameObject.layer == LayerMask.NameToLayer(Layers.PlSpot))
			{
				FirePlace connectedFireplace = col.GetComponent<FirePlace>();

				if (_fireplace.IsAccessible && connectedFireplace.IsAccessible && Vector2.Distance(col.bounds.center, _thisCol.bounds.center) < 0.1f)
				{
					_fireplace.Connect(connectedFireplace);
				}
				else if (_fireplace.IsAccessible == false && _fireplace.GetConnectedFireplaces().Contains(connectedFireplace) && Vector2.Distance(col.bounds.center, _thisCol.bounds.center) > 0.1f)
				{
					_fireplace.Disconnect(connectedFireplace);
				}
			}
		}
	}
}
