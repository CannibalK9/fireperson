using Assets.Scripts.Player.Config;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class AbilityMeter
	{
		private int _currentPoint;
		private bool _pointsChanging;
		private float _scale = 1f;
        private Variable Variable;
        private Dictionary<int,AbilityPoint> _abilityPoints;

		void Awake()
		{
			_currentPoint = 1; //temporary, need a way to save the whole meter and load it here
            _abilityPoints = new Dictionary<int, AbilityPoint>
            {
                {1, new AbilityPoint(_straight[0], new [] {2,12 }, true, Ability.None) },
                {2, new AbilityPoint(_straight[1], new [] {1, 3 }, false, Ability.None) },
                {3, new AbilityPoint(_straight[2], new [] {2, 4 }, false, Ability.None) },
                {4, new AbilityPoint(_straight[3], new [] {3, 5, 18, 19 }, false, Ability.None) },
                {5, new AbilityPoint(_straight[4], new [] {4, 6 }, false, Ability.None) },
                {6, new AbilityPoint(_straight[5], new [] {5, 7 }, false, Ability.None) },
                {7, new AbilityPoint(_straight[6], new [] {6, 8, 25, 26 }, false, Ability.None) },
                {8, new AbilityPoint(_straight[7], new [] {7, 9 }, false, Ability.None) },
                {9, new AbilityPoint(_straight[8], new [] {8, 10 }, false, Ability.None) },
                {10, new AbilityPoint(_straight[9], new [] {9, 11, 32 }, false, Ability.None) },
                {11, new AbilityPoint(_straight[10], new [] {10}, false, Ability.None) },
                {12, new AbilityPoint(_firstCurve[0], new [] { 13, 1}, false, Ability.None) },
                {13, new AbilityPoint(_firstCurve[1], new [] { 12, 14}, false, Ability.None) },
                {14, new AbilityPoint(_firstCurve[2], new [] { 13, 15}, false, Ability.None) },
                {15, new AbilityPoint(_firstCurve[3], new [] {14, 16}, false, Ability.None) },
                {16, new AbilityPoint(_firstCurve[4], new [] {15, 17}, false, Ability.None) },
                {17, new AbilityPoint(_firstCurve[5], new [] {16, 18}, false, Ability.None) },
                {18, new AbilityPoint(_firstCurve[6], new [] {17, 4}, false, Ability.None) },
                {19, new AbilityPoint(_secondCurve[0], new [] {20, 4}, false, Ability.None) },
                {20, new AbilityPoint(_secondCurve[1], new [] {19, 21}, false, Ability.None) },
                {21, new AbilityPoint(_secondCurve[2], new [] {20, 22}, false, Ability.None) },
                {22, new AbilityPoint(_secondCurve[3], new [] {21, 23}, false, Ability.None) },
                {23, new AbilityPoint(_secondCurve[4], new [] {22, 24}, false, Ability.None) },
                {24, new AbilityPoint(_secondCurve[5], new [] {23, 25}, false, Ability.None) },
                {25, new AbilityPoint(_secondCurve[6], new [] {24, 7}, false, Ability.None) },
                {26, new AbilityPoint(_thirdCurve[0], new [] {27, 7}, false, Ability.None) },
                {27, new AbilityPoint(_thirdCurve[1], new [] {26, 28}, false, Ability.None) },
                {28, new AbilityPoint(_thirdCurve[2], new [] {27, 29}, false, Ability.None) },
                {29, new AbilityPoint(_thirdCurve[3], new [] {28, 30}, false, Ability.None) },
                {30, new AbilityPoint(_thirdCurve[4], new [] {29, 31}, false, Ability.None) },
                {31, new AbilityPoint(_thirdCurve[5], new [] {30, 32}, false, Ability.None) },
                {32, new AbilityPoint(_thirdCurve[6], new [] {31, 10}, false, Ability.None) },
            };
		}

		void Update()
		{
            bool isClickingOnThisGuiObject = true;
            if (isClickingOnThisGuiObject && Input.GetKey(KeyCode.Mouse0))
            {
                var direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                foreach (int connection in _abilityPoints[_currentPoint].Connections)
                {
                    if (Vector2.Angle(direction, _abilityPoints[connection].Point - _abilityPoints[_currentPoint].Point) < 10f)
                    {
                        AbilityPoint abilityPoint = _abilityPoints[connection];
                        if (abilityPoint.IsActivated)// || pointsAvailable)
                        {
                            SetNewCurrentPoint(connection, abilityPoint);
                            return;
                        }
                    }
                }
            }
		}

        private void SetNewCurrentPoint(int key, AbilityPoint abilityPoint)
        {
            //pointsAvailable go up or down
            //activate/deactivate ability
            //create/destroy associated gameobject
            abilityPoint.IsActivated = !abilityPoint.IsActivated;
            _abilityPoints[key] = abilityPoint;
            _currentPoint = key;
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
