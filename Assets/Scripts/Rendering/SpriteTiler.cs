using Assets.Scripts.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Rendering
{
	[ExecuteInEditMode]
	class SpriteTiler : MonoBehaviour
	{
		#if UNITY_EDITOR
		private float _gridX;
		private float _gridY;

		void Update()
		{
			if (transform.localScale.x < 1 || transform.localScale.y < 1 || (_gridX == transform.localScale.x && _gridY == transform.localScale.y))
				return;

			Quaternion rotation = transform.rotation;
			transform.rotation = new Quaternion();

			_gridX = transform.localScale.x;
			_gridY = transform.localScale.y;

			var children = new List<GameObject>();
			foreach (Transform c in transform.GetComponentsInChildren<Transform>())
			{
				if (c != transform)
				children.Add(c.gameObject);
			}
			children.ForEach(c => DestroyImmediate(c));

			var sprite = GetComponent<SpriteRenderer>();
			Vector2 spriteSize_wu = new Vector2(sprite.bounds.size.x / transform.localScale.x, sprite.bounds.size.y / transform.localScale.y);
			var scale = Vector3.one;

			if (_gridX != 0)
			{
				float width_wu = sprite.bounds.size.x / _gridX;
				scale.x = width_wu / spriteSize_wu.x;
				spriteSize_wu.x = width_wu;
			}

			if (_gridY != 0)
			{
				float height_wu = sprite.bounds.size.y / _gridY;
				scale.y = height_wu / spriteSize_wu.y;
				spriteSize_wu.y = height_wu;
			}

			var childPrefab = new GameObject();

			var childSprite = childPrefab.AddComponent<SpriteRenderer>();
			childPrefab.transform.position = transform.position;
			childPrefab.gameObject.layer = LayerMask.NameToLayer(Layers.Background);
			childSprite.sprite = sprite.sprite;
			childSprite.material = sprite.sharedMaterial;
			childSprite.sortingLayerID = sprite.sortingLayerID;
			childSprite.sortingOrder = sprite.sortingOrder;

			GameObject child;
			int h = Mathf.RoundToInt(sprite.bounds.size.y / spriteSize_wu.y);

			for (int i = 0; i < h; i++)
			{
				int w = Mathf.RoundToInt(sprite.bounds.size.x / spriteSize_wu.x);
				var offSet = new Vector3((spriteSize_wu.x / 2) * (w - 1), (spriteSize_wu.y / 2) * (h - 1), 0);
				for (int j = 0; j < w; j++) {
					child = Instantiate(childPrefab) as GameObject;
					child.transform.position = transform.position - (new Vector3(spriteSize_wu.x * j, spriteSize_wu.y * i, 0)) + offSet;
					child.transform.localScale = scale;
					child.transform.parent = transform;
				}
			}

			transform.rotation = rotation;
			DestroyImmediate(childPrefab);
			sprite.enabled = false; // Disable this SpriteRenderer and let the prefab children render themselves
		}
		#endif
	}
}
