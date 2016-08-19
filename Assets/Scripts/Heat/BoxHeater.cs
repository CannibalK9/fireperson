using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class BoxHeater : HeatHandler
	{
		protected override void SetColliderSizes(float additionalRange)
		{
			float newScale = (_defaultWidth + additionalRange) / _defaultWidth;
			transform.localScale = new Vector3(
				_defaultScale.x * newScale,
				_defaultScale.y,
				_defaultScale.z);
		}
	}
}
