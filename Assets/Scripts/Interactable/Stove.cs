using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
		void Start()
		{
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
		    foreach (FirePlace fireplace in GetAllConnectedFirePlaces(this, null))
		    {
				if (fireplace is Stove == false)
					fireplace.IsLit = true;
		    }
	    }

		public void ExtinguishAllConnectedFireplaces()
		{
			foreach (FirePlace fireplace in GetAllConnectedFirePlaces(this, null))
			{
				fireplace.IsLit = false;
			}
		}

		public bool AllChimneysAreClosed()
        {
            return GetAllConnectedFirePlaces(this, null).Any(fp => fp is Stove == false && fp.IsAccessible) == false;
        }

		public bool HasConnectedFullyLitStove()
		{
			return GetAllConnectedFirePlaces(this, null).OfType<Stove>().Any(stove => stove.IsFullyIgnited);
		}
	}
}
