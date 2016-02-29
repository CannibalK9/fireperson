using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RepeatSpriteBoundary : MonoBehaviour
{
    SpriteRenderer _sprite;
    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        int spriteCount = transform.localScale.x > transform.localScale.y ? (int)transform.localScale.x : (int)transform.localScale.y;

        Debug.Log(
            _sprite.bounds.ToString());

        float cosine = Mathf.Cos(transform.rotation.z);
        Vector2 startPosition = new Vector2(
            _sprite.bounds.min.x,
            _sprite.bounds.min.y + cosine * transform.lossyScale.y/2);

        Vector2 shiftValues = new Vector2(
            cosine * transform.lossyScale.x / spriteCount,
            Mathf.Sin(transform.rotation.z) * transform.lossyScale.y );

        GameObject childPrefab = new GameObject();

        SpriteRenderer childSprite = childPrefab.AddComponent<SpriteRenderer>();
        childSprite.sprite = _sprite.sprite;
        childSprite.sortingLayerID = _sprite.sortingLayerID;
        childSprite.sortingOrder = _sprite.sortingOrder;

        GameObject child;

        if (true)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                child = Instantiate(childPrefab) as GameObject;
                child.transform.parent = transform;
                child.transform.position = startPosition + i * shiftValues;
                child.transform.localScale = new Vector2(1/ transform.localScale.x, 1);
                child.transform.localRotation = new Quaternion();
            }
        }
        else
        {
            for (int i = 0; i < spriteCount; i++)
            {
                child = Instantiate(childPrefab) as GameObject;
                float offset = _sprite.bounds.size.x / 2 - 0.5f;
                child.transform.parent = transform;
            }
        }

        Destroy(childPrefab);
        _sprite.enabled = false;
    }
}