using UnityEngine;

namespace Assets.Scripts.Player
{
	public static class ChannelingHandler
	{
		public static bool ChannelingSet { get; set; }
		public static bool IsChanneling { get { return _channelingTime != 0; } }

		public static float Stability(float value)
		{
			return value / (_channelingTime + 1);
		}

		public static float Intensity(float value)
		{
			return value / (_channelingTime + 1);
		}

		public static float Control(float value)
		{
			return value / value;
		}

		private static float _stability;
		public static float PlStability()
		{
			return _stability * _channelingTime;
		}

		private static float _intensity;
		public static float PlIntensity()
		{
			return _intensity * _channelingTime;
		}

		private static float _control;
		public static float PlControl()
		{
			return _control * _channelingTime;
		}

		private const float _maximumChannelingTime = 3f;
		private static float _channelingTime;

		public static void Channel()
		{
			if (_channelingTime < _maximumChannelingTime)
				_channelingTime += Time.deltaTime;
		}

		public static void StopChanneling(float stability, float intensity, float control)
		{
			ChannelingSet = true;
			_stability = stability;
			_intensity = intensity;
			_control = control;
		}

		private const float _breakChannelTime = 2f;
		private static float _breakChannelingTime;

		public static void BreakChannel()
		{
			if (ChannelingSet == false)
			{
				_channelingTime = 0;
				_breakChannelingTime = 0;
				return;
			}

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
