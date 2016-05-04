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
            float radius = _heater.BoxCollider.bounds.extents.x + _heater.HeatRayDistance;
            var hits = new List<RaycastHit2D>();
            for (int i = 0; i < 20; i++)
            {
                //make it spin
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, radius, GetMeltingMask());
                if (hit)
                    hits.Add(hit);
            }
            SendRaycastMessages(hits);
        }

        private LayerMask GetMeltingMask()
        {
            return 1 << LayerMask.NameToLayer("Melting");
        }

        private void SendRaycastMessages(List<RaycastHit2D> hits)
        {
            List<RaycastHit2D> uniqueHits = hits.Select(h => h.collider)
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.SendMessage("Melt", hit, SendMessageOptions.RequireReceiver);
            }
        }
    }
}