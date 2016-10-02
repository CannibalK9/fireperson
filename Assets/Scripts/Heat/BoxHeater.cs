using UnityEngine;

namespace Assets.Scripts.Heat
{
	public class BoxHeater : HeatHandler
	{
		protected override void SetColliderSizes(float additionalRange)
		{
			float heatWidth = HeatMessage.HeatRange * 2;
			float heatLength = transform.lossyScale.y;

			float psLength = 0.55f - (0.2f * ((Mathf.Pow(2f, heatLength) - 1f) / Mathf.Pow(2f, heatLength - 1f)));

			_box.size = new Vector2(heatWidth, 1);

			if (_ps != null)
				_ps.transform.localScale = new Vector2(
					heatWidth * 0.2f + 0.3f,
					psLength);
		}
	}
}

/*
	0	0
	0.5	0.444
	1	0.5		0.3+0.2
	2	0.6666	0.3+0.3666
	3	0.9		0.3+0.6
	4	1.05
*/


/*
	10	0.11
	4	0.175	0.55 - 0.375=0.55 - 0.2*1.875	15/8
	3	0.2		0.55 - 0.35 =0.55 - 0.2*1.75	7/4
	2	0.25	0.55 - 0.3 = 0.55 - 0.2*1.5		3/2		2^n - 1/2^(n-1)
	1	0.35	0.55 - 0.2 = 0.55 - 0.2*1		1/1
	0	0
*/