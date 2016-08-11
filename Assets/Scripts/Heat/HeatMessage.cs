namespace Assets.Scripts.Heat
{
	public struct HeatMessage
	{
        public float DistanceToMove { get; set; }

		public HeatMessage(float distanceToMove)
		{
			DistanceToMove = distanceToMove;
		}
	}
}
