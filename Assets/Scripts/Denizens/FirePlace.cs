using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Denizens
{
    public class Fireplace : MonoBehaviour
    {
        public bool IsLit { get; set; }
        public bool IsAccessible;
        public bool IsHeatSource;

        public Fireplace Left;
        public Fireplace Right;
        public Fireplace Up;
        public Fireplace Down;

        private Vector2 origin;
        private Vector2 rotation;
        private RaycastHit2D hit;

        void Update()
        {
            if (IsLit == false || IsHeatSource == false)
                return;

            LayerMask mask = 1 << LayerMask.NameToLayer("Denizens");

            origin = transform.position;
            rotation = new Vector2(transform.rotation.x, transform.rotation.y);

            hit = Physics2D.Raycast(origin, Vector2.left + rotation, 10f, mask);
            if (hit)
                SendRaycastMessage(hit, DirectionTravelling.Right);

            hit = Physics2D.Raycast(origin, Vector2.right + rotation, 10f, mask);
            if (hit)
                SendRaycastMessage(hit, DirectionTravelling.Left);
        }

        private void SendRaycastMessage(RaycastHit2D hit, DirectionTravelling direction)
        {
            hit.collider.SendMessage("MoveToFireplace", direction, SendMessageOptions.RequireReceiver);
        }

        public List<Fireplace> GetConnectedFireplaces()
        {
            return new List<Fireplace> { Left, Right, Up, Down };
        }
    }
}
