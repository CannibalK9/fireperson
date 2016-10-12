using UnityEngine;

namespace Assets.Scripts.Player
{
	public static class ChannelingHandler
	{
		public static bool ChannelingSet { get; set; }
		public static bool IsChanneling { get { return _channelingTime != 0; } }
		public static bool IsTethered { get; set; }
		public static bool PlExists { get; set; }
		public static float ChannelPercent { get { return _channelingTime == 0 ? 0 : _channelingTime / _maximumChannelingTime; } }

		private const float _maximumChannelingTime = 3f;
		private static float _channelingTime;

		public static void StartChanneling()
		{
			_channelingTime += Time.deltaTime;
		}

		public static void Channel()
		{
			if (PlExists && _channelingTime < _maximumChannelingTime)
				_channelingTime += Time.deltaTime;
		}

		public static void StopChanneling()
		{
			ChannelingSet = true;
		}

		public static void BreakChannel()
		{
			PlExists = false;
			ChannelingSet = false;
			_channelingTime = 0;
		}
	}
}
