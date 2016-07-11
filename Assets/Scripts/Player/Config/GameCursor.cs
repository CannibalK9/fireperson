using UnityEngine.EventSystems;

namespace Assets.Scripts.Player.Config
{
	public static class GameCursor
	{
		public static bool IsOverUi()
		{
			return EventSystem.current.IsPointerOverGameObject();
		}
	}
}
