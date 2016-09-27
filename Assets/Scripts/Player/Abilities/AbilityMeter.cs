using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class AbilityMeter
	{
		private Vector2 _currentPoint;
		private bool _pointsChanging;
		private float _scale = 1f;

		void Awake()
		{
			_currentPoint = _straight[0];
		}

		void Update()
		{
			//click on UI, start changing points - current point is highlighted
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				//direction moving dictates where points go
				var clickPoint = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
					_pointsChanging = true;


				//if moving to valid point, check if points available and ignore, light or remove point
				//set current point accordingly

				
		
			}
		}

		private List<Vector2> _straight = new List<Vector2>
		{
			Vector2.zero,
			Vector2.up,
			new Vector2(0,2),
			new Vector2(0,3),
			new Vector2(0,4),
			new Vector2(0,5),
			new Vector2(0,6),
			new Vector2(0,7),
			new Vector2(0,8),
			new Vector2(0,9),
			new Vector2(0,10),
		};

		private List<Vector2> _firstCurve = new List<Vector2>
		{
			new Vector2(-56/64,10/64),
			new Vector2(-112/64,28/64),
			new Vector2(-160/64,54/64),
			new Vector2(-3,100/64),
			new Vector2(-160/64,138/64),
			new Vector2(-112/64,164/64),
			new Vector2(-56/64,182/64),
		};

		private List<Vector2> _secondCurve = new List<Vector2>
		{
			new Vector2(56/64,202/64),
			new Vector2(112/64,220/64),
			new Vector2(160/64,246/64),
			new Vector2(3,292/64),
			new Vector2(160/64,330/64),
			new Vector2(112/64,356/64),
			new Vector2(56/64,374/64),
		};

		private List<Vector2> _thirdCurve = new List<Vector2>
		{
			new Vector2(-56/64,394/64),
			new Vector2(-112/64,412/64),
			new Vector2(-160/64,438/64),
			new Vector2(-3,484/64),
			new Vector2(-160/64,522/64),
			new Vector2(-112/64,548/64),
			new Vector2(-56/64,566/64),
		};
	}
}
