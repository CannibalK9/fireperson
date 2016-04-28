using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
    public class FirePlace : MonoBehaviour
    {
        public bool IsLit;
        public bool IsAccessible;
        public bool IsHeatSource;

        public FirePlace Left;
        public FirePlace Right;
        public FirePlace Up;
        public FirePlace Down;

        private Vector2 _origin;
        private Vector2 _rotation;
        private RaycastHit2D _hit;

        void Update()
        {
            if (IsLit == false || IsHeatSource == false)
                return;

            LayerMask mask = 1 << LayerMask.NameToLayer("Denizens") | 1 << LayerMask.NameToLayer("Static Environment");

            _origin = transform.position;
            _rotation = new Vector2(transform.rotation.x, transform.rotation.y);

            _hit = Physics2D.Raycast(_origin, Vector2.left + _rotation, 10f, mask);
            if (_hit)
                SendRaycastMessage(_hit, DirectionTravelling.Right);

            _hit = Physics2D.Raycast(_origin, Vector2.right + _rotation, 10f, mask);
            if (_hit)
                SendRaycastMessage(_hit, DirectionTravelling.Left);
        }

        private void SendRaycastMessage(RaycastHit2D hit, DirectionTravelling direction)
        {
            hit.collider.SendMessage("MoveToFireplace", direction, SendMessageOptions.DontRequireReceiver);
        }

        public List<FirePlace> GetConnectedFireplaces()
        {
            return new List<FirePlace> { Left, Right, Up, Down };
        }
    }
}
