using UnityEngine;

namespace Assets.Scripts.Sprites
{
    public class ClimbableEdges : MonoBehaviour
    {
        public bool LeftEdge;
        public bool RightEdge;

        public GameObject LeftEdgeObject;
        public GameObject RightEdgeObject;


        void Awake()
        {
            if (LeftEdge)
            {
                GameObject leftEdge = Instantiate(LeftEdgeObject);

                leftEdge.transform.parent = transform;
                leftEdge.transform.localRotation = new Quaternion();
                leftEdge.transform.localPosition = new Vector2(-0.5f, 0);
            }

            if (RightEdge)
            {
                GameObject rightEdge = Instantiate(RightEdgeObject);

                rightEdge.transform.parent = transform;
                rightEdge.transform.localRotation = new Quaternion();
                rightEdge.transform.localPosition = new Vector2(0.5f, 0);
            }
        }
    }
}
