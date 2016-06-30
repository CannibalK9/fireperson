using Assets.Scripts.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class HeatHandler
	{
		private readonly IVariableHeater _heater;
		private readonly GameObject _steam;
        private readonly GameObject _emberLight;
        private readonly GameObject _emberStrong;

        public HeatHandler(IVariableHeater heater)
		{
			_heater = heater;
			_steam = (GameObject)Resources.Load("particles/steam");
			_emberLight = (GameObject)Resources.Load("particles/emberLight");
			_emberStrong = (GameObject)Resources.Load("particles/emberStrong");
        }

        public void OneCircleHeat(EmberEffect effect)
		{
			CastMeltingCircle(_heater.Collider.bounds.center, effect);
		}

		public void TwoCirclesHeat(EmberEffect effect)
		{
			CastMeltingCircle(new Vector2(
				_heater.Collider.bounds.center.x,
				_heater.Collider.bounds.min.y + _heater.Collider.bounds.extents.x), effect);

			CastMeltingCircle(new Vector2(
				_heater.Collider.bounds.center.x,
				_heater.Collider.bounds.max.y - _heater.Collider.bounds.extents.x), effect);
		}

		private void CastMeltingCircle(Vector2 origin, EmberEffect effect)
		{
			const int numberOfCasts = 20;

			float radius = _heater.Collider.bounds.extents.x + _heater.HeatRayDistance / 10;
			var hits = new List<RaycastHit2D>();
			for (int i = 0; i < numberOfCasts; i++)
			{
				Vector2 direction = Rotate(Vector2.up, i * (360 / numberOfCasts));
				RaycastHit2D hit = Physics2D.Raycast(origin, direction, radius, GetMeltingMask());
				Debug.DrawRay(origin, direction * radius, Color.green);

                if (effect != EmberEffect.None)
                {
                    Vector2 point = origin + direction * radius;
                    if (_heater.Collider.OverlapPoint(point) == false)
                    {
                        GameObject particle = effect == EmberEffect.Light ? _emberLight : _emberStrong;
                        Object.Instantiate(particle, point, Quaternion.Euler(direction));
                    }
                }

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
			return 1 << LayerMask.NameToLayer(Layers.Ice) | 1 << LayerMask.NameToLayer(Layers.BackgroundIce);
		}

		private void SendRaycastMessages(List<RaycastHit2D> hits, Vector2 origin, float castDistance)
		{
			foreach (RaycastHit2D hit in hits)
			{
                //icy embers or something go here
                //if (Physics2D.CircleCast(hit.point, 0.01f, Vector2.up, 0.01f, 1 << LayerMask.NameToLayer(Layers.Steam)) == false)
				    //Object.Instantiate(_steam, new Vector3(hit.point.x, hit.point.y, -10), _steam.transform.rotation);
			}

			IEnumerable<RaycastHit2D> uniqueHits = hits.GroupBy(hit => hit.collider).Select(h => h.First()).ToList();
			foreach (RaycastHit2D hit in uniqueHits)
			{
				hit.collider.SendMessage("Melt", new HeatMessage
                {
                    Hit = hit,
                    Origin = origin,
                    CastDistance = castDistance,
                    DistanceToMove = _heater.HeatIntensity / 10
                },
                SendMessageOptions.RequireReceiver);
			}
		}
	}
}