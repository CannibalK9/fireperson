using UnityEngine;

namespace Assets.Scripts.Sprites
{
    public class MeshSortingOrder : MonoBehaviour
    {
        public string LayerName;
        public int Order;

        private SkinnedMeshRenderer _rend;
        void Awake()
        {
            _rend = GetComponent<SkinnedMeshRenderer>();
            _rend.sortingLayerName = LayerName;
            _rend.sortingOrder = Order;
        }

        public void Update()
        {
            if (_rend.sortingLayerName != LayerName)
                _rend.sortingLayerName = LayerName;
            if (_rend.sortingOrder != Order)
                _rend.sortingOrder = Order;
        }

        public void OnValidate()
        {
            _rend = GetComponent<SkinnedMeshRenderer>();
            _rend.sortingLayerName = LayerName;
            _rend.sortingOrder = Order;
        }
    }
}