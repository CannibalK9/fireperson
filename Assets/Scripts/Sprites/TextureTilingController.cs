using UnityEngine;

namespace Assets.Scripts.Sprites
{
    [ExecuteInEditMode]
    public class TextureTilingController : MonoBehaviour
    {

        // Give us the texture so that we can scale proportianally the width according to the height variable below
        // We will grab it from the meshRenderer
        public Texture texture;
        public float textureToMeshZ = 2f; // Use this to contrain texture to a certain size

        Vector3 prevScale = Vector3.one;
        float prevTextureToMeshZ = -1f;

        void Start()
        {
            prevScale = gameObject.transform.lossyScale;
            prevTextureToMeshZ = this.textureToMeshZ;

            UpdateTiling();
        }

        void Update()
        {
            if (gameObject.transform.lossyScale != prevScale || !Mathf.Approximately(this.textureToMeshZ, prevTextureToMeshZ))
                UpdateTiling();

            prevScale = gameObject.transform.lossyScale;
            prevTextureToMeshZ = textureToMeshZ;
        }

        [ContextMenu("UpdateTiling")]
        void UpdateTiling()
        {
            // A Unity plane is 10 units x 10 units
            float planeSizeX = 10f;
            float planeSizeZ = 10f;

            // Figure out texture-to-mesh width based on user set texture-to-mesh height
            float textureToMeshX = ((float)texture.width / texture.height) * textureToMeshZ;

            GetComponent<Renderer>().sharedMaterial.mainTextureScale
                = new Vector2(
                    planeSizeX * gameObject.transform.lossyScale.x / textureToMeshX,
                    planeSizeZ * gameObject.transform.lossyScale.y / textureToMeshZ);
        }
    }
}