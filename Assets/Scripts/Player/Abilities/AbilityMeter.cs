using Assets.Scripts.Helpers;
using Assets.Scripts.Player.Config;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
	public class AbilityMeter : MonoBehaviour
	{
		public string Meter = "Stability";
		private int _currentPoint;
        private Dictionary<int,AbilityPoint> _abilityPoints;
		private GameObject _uiLight;
		private float _delay = 0.1f;
		private float _elapsed;

		void Awake()
		{
			_uiLight = Resources.Load(string.Format("particles/{0}Light", Meter)) as GameObject;
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
                {15, new AbilityPoint(_firstCurve[3], new [] {14, 16}, false, Ability.Tether) },
                {16, new AbilityPoint(_firstCurve[4], new [] {15, 17}, false, Ability.None) },
                {17, new AbilityPoint(_firstCurve[5], new [] {16, 18}, false, Ability.None) },
                {18, new AbilityPoint(_firstCurve[6], new [] {17, 4}, false, Ability.None) },
                {19, new AbilityPoint(_secondCurve[0], new [] {20, 4}, false, Ability.None) },
                {20, new AbilityPoint(_secondCurve[1], new [] {19, 21}, false, Ability.None) },
                {21, new AbilityPoint(_secondCurve[2], new [] {20, 22}, false, Ability.None) },
                {22, new AbilityPoint(_secondCurve[3], new [] {21, 23}, false, Ability.Scout) },
                {23, new AbilityPoint(_secondCurve[4], new [] {22, 24}, false, Ability.None) },
                {24, new AbilityPoint(_secondCurve[5], new [] {23, 25}, false, Ability.None) },
                {25, new AbilityPoint(_secondCurve[6], new [] {24, 7}, false, Ability.None) },
                {26, new AbilityPoint(_thirdCurve[0], new [] {27, 7}, false, Ability.None) },
                {27, new AbilityPoint(_thirdCurve[1], new [] {26, 28}, false, Ability.None) },
                {28, new AbilityPoint(_thirdCurve[2], new [] {27, 29}, false, Ability.None) },
                {29, new AbilityPoint(_thirdCurve[3], new [] {28, 30}, false, Ability.Flash) },
                {30, new AbilityPoint(_thirdCurve[4], new [] {29, 31}, false, Ability.None) },
                {31, new AbilityPoint(_thirdCurve[5], new [] {30, 32}, false, Ability.None) },
                {32, new AbilityPoint(_thirdCurve[6], new [] {31, 10}, false, Ability.None) },
            };
		}

		void Update()
		{
			if (_elapsed > 0 && _elapsed < _delay)
			{
				_elapsed += Time.deltaTime;
				return;
			}
			else if (_elapsed > _delay)
			{
				_elapsed = 0;
			}

            if (Input.GetKey(KeyCode.Mouse0) && Pointer.IsPointerOverUIObject())
            {
                var direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

                foreach (int connection in _abilityPoints[_currentPoint].Connections)
                {
                    if (Vector2.Angle(direction, _abilityPoints[connection].Point - _abilityPoints[_currentPoint].Point) < 30f)
                    {
						_elapsed += Time.deltaTime;

						if ((_currentPoint == 4 && connection == 3 && _abilityPoints[3].IsActivated == false)
							|| (_currentPoint == 4 && connection == 18 && _abilityPoints[18].IsActivated == false)
							|| (_currentPoint == 7 && connection == 6 && _abilityPoints[6].IsActivated == false)
							|| (_currentPoint == 7 && connection == 25 && _abilityPoints[25].IsActivated == false)
							|| (_currentPoint == 10 && connection == 9 && _abilityPoints[9].IsActivated == false)
							|| (_currentPoint == 10 && connection == 32 && _abilityPoints[32].IsActivated == false))
							return;

                        AbilityPoint abilityPoint = _abilityPoints[connection];
                        if (abilityPoint.IsActivated)
                        {
							PlayerPrefs.SetFloat(Meter, (abilityPoint.Point.y * 10) + 1);
							DeactivateCurrentPoint();
							_currentPoint = connection;
                        }
						else if (PlayerPrefs.GetFloat(Variable.AvailablePoints.ToString()) > 0)
						{
							SetNewCurrentPoint(connection, abilityPoint);
						}
                        return;
					}
				}
            }
		}

		private void DeactivateCurrentPoint()
		{
			AbilityPoint abilityPoint = _abilityPoints[_currentPoint];
			abilityPoint.IsActivated = false;
			_abilityPoints[_currentPoint] = abilityPoint;
			AbilityState.SetActive(abilityPoint.Ability, false);
			PlayerPrefs.SetFloat(Variable.AvailablePoints.ToString(), PlayerPrefs.GetFloat(Variable.AvailablePoints.ToString()) + 1);
			PlayerPrefs.Save();

			Transform t = transform.FindChild(_currentPoint.ToString());
			if (t != null)
				Destroy(t.gameObject);
		}

        private void SetNewCurrentPoint(int key, AbilityPoint abilityPoint)
        {
			abilityPoint.IsActivated = true;
			_abilityPoints[key] = abilityPoint;
			_currentPoint = key;

			AbilityState.SetActive(abilityPoint.Ability, true);
			PlayerPrefs.SetFloat(Meter, (abilityPoint.Point.y * 10) + 1);
			PlayerPrefs.SetFloat(Variable.AvailablePoints.ToString(), PlayerPrefs.GetFloat(Variable.AvailablePoints.ToString()) - 1);
			PlayerPrefs.Save();

			GameObject g = Instantiate(_uiLight);
			g.name = key.ToString();
			g.transform.parent = transform;
			g.transform.localPosition = abilityPoint.Point;
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
			new Vector2(-56f/64f,10f/64f),
			new Vector2(-112f/64f,28f/64f),
			new Vector2(-160f/64f,54f/64f),
			new Vector2(-3f,100f/64f),
			new Vector2(-160f/64f,138f/64f),
			new Vector2(-112f/64f,164f/64f),
			new Vector2(-56f/64f,182f/64f),
		};

		private List<Vector2> _secondCurve = new List<Vector2>
		{
			new Vector2(56f/64f,202f/64f),
			new Vector2(112f/64f,220f/64f),
			new Vector2(160f/64f,246f/64f),
			new Vector2(3f,292f/64f),
			new Vector2(160f/64f,330f/64f),
			new Vector2(112f/64f,356f/64f),
			new Vector2(56f/64f,374f/64f),
		};

		private List<Vector2> _thirdCurve = new List<Vector2>
		{
			new Vector2(-56f/64f,394f/64f),
			new Vector2(-112f/64f,412f/64f),
			new Vector2(-160f/64f,438f/64f),
			new Vector2(-3f,484f/64f),
			new Vector2(-160f/64f,522f/64f),
			new Vector2(-112f/64f,548f/64f),
			new Vector2(-56f/64f,566f/64f),
		};
	}
}
