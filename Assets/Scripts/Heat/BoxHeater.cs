using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class BoxHeater : HeatHandler
	{
		protected override void SetColliderSizes(float additionalRange)
		{
			float heatWidth = HeatMessage.HeatRange * 2 + _width;
			float heatLength = transform.lossyScale.y;

			_box.size = new Vector2(heatWidth, 1);

			if (_ps != null)
				_ps.transform.localScale = new Vector2(
					heatWidth * 0.2f,
					heatLength * 0.2f);
		}
	}
}

/*
	30	1.9
	20	1.6
	10	1.1	
	5	0.8
	4	0.7
	3	0.6
	1	0.35
*/