using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
		List<FirePlace> _connectedFireplaces;
		
		void Start()
		{
			_connectedFireplaces = GetAllConnectedFirePlaces(this, null);
		}

		public List<FirePlace> GetAllConnectedFirePlaces(FirePlace fireplace, FirePlace origin)
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
				fireplace.IsLit = true;
		    }
	    }

        public bool CanBeLitByDenizens()
        {
            return _connectedFireplaces.OfType<ChimneyLid>().All(lid => lid.IsOpen == false);
        }
    }
}
