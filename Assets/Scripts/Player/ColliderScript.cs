using UnityEngine;

namespace Assets.Scripts.Player
{
    public class ColliderScript : MonoBehaviour
    {
        public PlayerController Controller;

        public void Spotted()
        {
            Controller.Spotted();
        }
    }
}
