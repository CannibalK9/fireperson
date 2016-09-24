namespace Assets.Scripts.Heat
{
	public class CircleHeater : HeatHandler
	{
		protected override void SetColliderSizes(float range)
		{
			transform.localScale = UnityEngine.Vector3.one * HeatMessage.HeatRange;
		}
	}
}
