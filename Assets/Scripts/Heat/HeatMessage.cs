namespace Assets.Scripts.Heat
{
	public struct HeatMessage
	{
		public float Range { get; set; }
        public float Intensity { get; set; }

		public HeatMessage(float intensity, float range)
		{
			Intensity = intensity > 100 ? 100 : intensity;
			Range = range;
		}
	}
}
