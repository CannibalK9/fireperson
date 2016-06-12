using UnityEngine;

namespace Assets.Scripts.Rendering
{
    public class ParticleSystemAutoDestroy : MonoBehaviour
    {
        private ParticleSystem ps;

        public void Awake()
        {
            ps = GetComponent<ParticleSystem>();
        }

        public void Update()
        {
            if (ps && ps.IsAlive() == false)
            {
                Destroy(gameObject);
            }
        }
    }
}
