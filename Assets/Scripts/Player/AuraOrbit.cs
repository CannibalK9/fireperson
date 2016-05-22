using UnityEngine;

namespace Assets.Scripts.Player
{
    public class AuraOrbit : MonoBehaviour
    {
        void Update()
        {
            transform.RotateAround(transform.parent.position + new Vector3(5f, 5f, 0f), transform.parent.position, 1000 * Time.deltaTime);
        }
    }
}
