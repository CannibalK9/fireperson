using Assets.Scripts.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class HeatHandler
	{
		private readonly IVariableHeater _heater;

		public HeatHandler(IVariableHeater heater)
		{
			_heater = heater;
		}

		public void OneCircleHeat()
		{
			CastMeltingCircle(_heater.BoxCollider.bounds.center);
		}

		public void TwoCirclesHeat()
		{
			CastMeltingCircle(new Vector2(
				_heater.BoxCollider.bounds.center.x,
				_heater.BoxCollider.bounds.min.y + _heater.BoxCollider.bounds.extents.x));

			CastMeltingCircle(new Vector2(
				_heater.BoxCollider.bounds.center.x,
				_heater.BoxCollider.bounds.max.y - _heater.BoxCollider.bounds.extents.x));
		}

		private void CastMeltingCircle(Vector2 origin)
		{
			const int numberOfCasts = 20;

			float radius = _heater.BoxCollider.bounds.extents.x + _heater.HeatRayDistance;
			var hits = new List<RaycastHit2D>();
			for (int i = 0; i < numberOfCasts; i++)
			{
				Vector2 direction = Rotate(Vector2.up, i * (360 / numberOfCasts));
				RaycastHit2D hit = Physics2D.Raycast(origin, direction, radius, GetMeltingMask());
				Debug.DrawRay(origin, direction * radius, Color.green);
				if (hit)
					hits.Add(hit);
			}
			SendRaycastMessages(hits, origin, radius);
		}

		public static Vector2 Rotate(Vector2 v, float degrees)
		{
			return Quaternion.Euler(0, 0, degrees) * v;
		}

		private LayerMask GetMeltingMask()
		{
			return 1 << LayerMask.NameToLayer(Layers.Ice);
		}

		private void SendRaycastMessages(IEnumerable<RaycastHit2D> hits, Vector2 origin, float castDistance)
		{
			IEnumerable<RaycastHit2D> uniqueHits = hits.GroupBy(hit => hit.collider).Select(h => h.First()).ToList();
			foreach (RaycastHit2D hit in uniqueHits)
			{
				hit.collider.SendMessage("Melt", new HeatMessage { Hit = hit, Origin = origin, CastDistance = castDistance}, SendMessageOptions.RequireReceiver);
			}
		}
	}
}