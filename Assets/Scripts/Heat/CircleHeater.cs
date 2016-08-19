using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class CircleHeater : HeatHandler
	{
		protected override void SetColliderSizes(float additionalRange)
		{
			float newScale = (_defaultWidth + additionalRange) / _defaultWidth;
			transform.localScale = _defaultScale * newScale;

			//_ps.transform.localScale = _ps.transform.localScale * newScale;
		}
	}
}
