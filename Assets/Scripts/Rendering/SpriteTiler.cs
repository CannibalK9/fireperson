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
		[Range(1, 50)]
		public int ScaleX = 1;
		[Range(1, 50)]
		public int ScaleY = 1;

		public float PositionX;
		public float PositionY;

		private int _scaleX;
		private int _scaleY;

		private float _positionX;
		private float _positionY;

		void Awake()
		{
			ScaleX = Mathf.RoundToInt(transform.localScale.x);
			ScaleY = Mathf.RoundToInt(transform.localScale.y);

			PositionX = (float)Math.Round(transform.localPosition.x, 2);
			PositionY = (float)Math.Round(transform.localPosition.y, 2);

			_scaleX = ScaleX;
			_scaleY = ScaleY;

			_positionX = PositionX;
			_positionY = PositionY;
		}

		void Update()
		{
			float xRound = (float)Math.Round(transform.localPosition.x, 2);
			float yRound = (float)Math.Round(transform.localPosition.y, 2);
			float rotRound = (float)Math.Round(transform.localRotation.eulerAngles.z, 2);

			if (rotRound % 90 == 0 && (_positionX != xRound || _positionY != yRound))
			{
				PositionX = xRound;
				PositionY = yRound;
			}

			if (rotRound % 90 == 0 && (_positionX != PositionX || _positionY != PositionY))
			{
				float roundedX = _positionX < PositionX
					? Mathf.Ceil(PositionX * 4)
					: Mathf.Floor(PositionX * 4);
				PositionX = roundedX / 4;

				float roundedY = _positionY < PositionY
					? Mathf.Ceil(PositionY * 4)
					: Mathf.Floor(PositionY * 4);
				PositionY = roundedY / 4;

				transform.localPosition = new Vector2(PositionX, PositionY);
			}

			_positionX = PositionX;
			_positionY = PositionY;

			if (_scaleX != ScaleX || _scaleY != ScaleY)
			{
				_scaleX = ScaleX;
				_scaleY = ScaleY;

				transform.localScale = new Vector3(ScaleX, ScaleY, 1);

				Quaternion rotation = transform.rotation;
				transform.rotation = Quaternion.Euler(Vector3.zero);

				var children = new List<GameObject>();
				foreach (Transform c in transform.GetComponentsInChildren<Transform>())
				{
					if (c != transform && c.GetComponent<SpriteRenderer>() != null)
						children.Add(c.gameObject);
				}
				children.ForEach(c => DestroyImmediate(c));

				var sprite = GetComponent<SpriteRenderer>();
				Vector2 spriteSize_wu = new Vector2(sprite.bounds.size.x / ScaleX, sprite.bounds.size.y / ScaleY);
				var scale = Vector3.one;

				float width_wu = sprite.bounds.size.x / ScaleX;
				scale.x = width_wu / spriteSize_wu.x;
				spriteSize_wu.x = width_wu;

				float height_wu = sprite.bounds.size.y / ScaleY;
				scale.y = height_wu / spriteSize_wu.y;
				spriteSize_wu.y = height_wu;

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
					for (int j = 0; j < w; j++)
					{
						child = Instantiate(childPrefab) as GameObject;
						child.transform.position = transform.position - (new Vector3(spriteSize_wu.x * j, spriteSize_wu.y * i, 0)) + offSet;
						child.transform.localScale = scale;
						child.transform.localRotation = new Quaternion();
						child.transform.parent = transform;
					}
				}

				transform.rotation = rotation;
				DestroyImmediate(childPrefab);
				sprite.enabled = false;
			}
		}
		#endif
	}
}
