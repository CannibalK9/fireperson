using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
		private List<FirePlace> _connectedFireplaces;

		void Start()
		{
			_connectedFireplaces = GetAllConnectedFirePlaces(this, null);
			if (IsFullyIgnited)
				LightAllConnectedFireplaces();
		}

		private List<FirePlace> GetAllConnectedFirePlaces(FirePlace fireplace, FirePlace origin)
		{
			var fireplaces = new List<FirePlace>();

			foreach (FirePlace fp in fireplace.GetConnectedFireplaces())
			{
				if (fp != null && fp != origin)
				{
					fireplaces.Add(fp);
					fireplaces.AddRange(GetAllConnectedFirePlaces(fp, fireplace));
				}
			}
			return fireplaces;
		}

	    public void LightAllConnectedFireplaces()
	    {
		    foreach (FirePlace fireplace in _connectedFireplaces)
		    {
				if (fireplace is Stove == false)
					fireplace.IsLit = true;
		    }
	    }

		public void ExtinguishAllConnectedFireplaces()
		{
			foreach (FirePlace fireplace in _connectedFireplaces)
			{
				fireplace.IsLit = false;
			}
		}

		public bool AllChimneysAreClosed()
        {
            return _connectedFireplaces.OfType<ChimneyLid>().All(lid => lid.IsAccessible == false);
        }

		public bool HasConnectedFullyLitStove()
		{
			return _connectedFireplaces.OfType<Stove>().Any(stove => stove.IsFullyIgnited);
		}
	}
}
