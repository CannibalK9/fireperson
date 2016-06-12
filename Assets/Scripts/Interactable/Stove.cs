using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
        public ChimneyLid ChimneyLid1;
        public ChimneyLid ChimneyLid2;
        public ChimneyLid ChimneyLid3;

        public bool CanBeLitByDenizens()
        {
            var chimneyLids = new List<ChimneyLid>();
            if (ChimneyLid1 != null)
                chimneyLids.Add(ChimneyLid1);
            if (ChimneyLid2 != null)
                chimneyLids.Add(ChimneyLid2);
            if (ChimneyLid3 != null)
                chimneyLids.Add(ChimneyLid3);

            if (chimneyLids.Count == 0)
                return false;
            return chimneyLids.All(lid => lid.IsOpen == false);
        }
    }
}
