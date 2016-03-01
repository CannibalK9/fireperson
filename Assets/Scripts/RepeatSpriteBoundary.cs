using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RepeatSpriteBoundary : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float width = transform.localScale.x;
        float height = transform.localScale.y;

        int spriteCount = width > height
            ? (int)width
            : (int)height;

        GameObject childPrefab = new GameObject();

        SpriteRenderer childSprite = childPrefab.AddComponent<SpriteRenderer>();
        childSprite.sprite = sprite.sprite;
        childSprite.sortingLayerID = sprite.sortingLayerID;
        childSprite.sortingOrder = sprite.sortingOrder;

        GameObject child;
        GameObject middleChild = new GameObject();

        for (int i = 0; i < spriteCount; i++)
        {
            child = Instantiate(childPrefab) as GameObject;
            child.transform.parent = middleChild.transform;
            float iFloat = (float)Convert.ToDouble(i);
            if (width > height)
            {
                child.transform.position = new Vector2(
                    child.transform.position.x + (2f * iFloat - width + 1f) / (2f * width),
                    child.transform.position.y);

                child.transform.localScale = new Vector2(1 / width, 1);
            }
            else
            {
                child.transform.position = new Vector2(
                    child.transform.position.x,
                    child.transform.position.y + (2f * iFloat - height + 1f) / (2f * height));

                child.transform.localScale = new Vector2(1, 1 / height);
            }
        }
        middleChild.transform.parent = transform;
        middleChild.transform.localRotation = new Quaternion();
        middleChild.transform.localScale = new Vector2(1, 1);
        middleChild.transform.localPosition = new Vector2(0, 0);

        Destroy(childPrefab);
        sprite.enabled = false;
    }
}