using UnityEngine;

namespace Assets.Scripts.Heat
{
    public class HeatHandler
    {
        private IVariableHeater _heater;

        public HeatHandler(IVariableHeater heater)
        {
            _heater = heater;
        }

        public void Heat()
        {
            Vector2 origin = new Vector2(
                _heater.BoxCollider.bounds.center.x,
                _heater.BoxCollider.bounds.min.y + _heater.BoxCollider.bounds.extents.x);

            float radius = _heater.BoxCollider.bounds.extents.x + _heater.HeatRayDistance;
            float distance = _heater.BoxCollider.bounds.size.y - 2 * _heater.BoxCollider.bounds.extents.x;
            LayerMask mask = 1 << LayerMask.NameToLayer("Melting");

            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, radius, Vector2.up, distance, mask);

            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.SendMessage("Melt", hit.point, SendMessageOptions.RequireReceiver);
            }
        }
    }
}