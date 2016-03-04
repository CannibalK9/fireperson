using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Fire
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Fire : MonoBehaviour
    {
        public bool createFire;

        void OnGUI()
        {
            createFire = EditorGUILayout.Toggle("Create fire", false);
        }

        void Update()
        {
            if (createFire)
                CreateFire();
        }

        public void CreateFire()
        {
            var fire = new GameObject();
            var light = fire.AddComponent<Light>();
            var halo = light.GetComponent("Halo");
            //var haloEnabledProperty = halo.GetType().GetProperty("enabled");
            //haloEnabledProperty.SetValue(halo, true, null);
            var woo = Instantiate(fire) as GameObject;
            woo.transform.parent = transform;
        }
    }
}
