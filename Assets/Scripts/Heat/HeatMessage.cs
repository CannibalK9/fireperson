namespace Assets.Scripts.Heat
{
	public struct HeatMessage
	{
		public float HeatRange { get; set; }
        public float DistanceToMove { get; set; }

		public HeatMessage(float distanceToMove, float heatRange)
		{
			DistanceToMove = distanceToMove;
			HeatRange = heatRange;
		}
	}
}
