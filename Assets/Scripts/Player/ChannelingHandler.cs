using UnityEngine;

namespace Assets.Scripts.Player
{
	public static class ChannelingHandler
	{
		public static bool ChannelingSet { get; set; }
		public static bool IsChanneling { get { return _channelingTime != 0; } }
		public static bool IsTethered { get; set; }
		public static bool PlExists { get; set; }
		public static float ChannelPercent { get { return _channelingTime / _maximumChannelingTime; } }

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

		private const float _breakChannelTime = 2f;
		private static float _breakChannelingTime;

		public static void StartBreaking()
		{
			_breakChannelingTime += Time.deltaTime;
		}

		public static void BreakChannel()
		{
			if (ChannelingSet == false)
			{
				_channelingTime = 0;
				_breakChannelingTime = 0;
				return;
			}

			if (_breakChannelingTime == 0)
				return;

			_breakChannelingTime += Time.deltaTime;
			if (_breakChannelingTime > _breakChannelTime)
			{
				_channelingTime = 0f;
				ChannelingSet = false;
			}
		}

		public static void StopBreaking()
		{
			_breakChannelingTime = 0f;
		}
	}
}
