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
            RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, radius, Vector2.up, 0.001f, GetMeltingMask());
            SendRaycastMessages(hits, new Vector4(origin.x, origin.y, radius, 0));
        }

        private LayerMask GetMeltingMask()
        {
            return 1 << LayerMask.NameToLayer("Melting");
        }

        private void SendRaycastMessages(RaycastHit2D[] hits, Vector4 value)
        {
            foreach (RaycastHit2D hit in hits)
            {
                hit.collider.SendMessage("Melt", value, SendMessageOptions.RequireReceiver);
            }
        }
    }
}