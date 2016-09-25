using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
		private List<ParticleSystem> _particles;

		void Start()
		{
			_particles = GetComponentsInChildren<ParticleSystem>().Where(p => p.tag == "StoveParticles").ToList();
			if (IsFullyIgnited)
				LightAllConnectedFireplaces();
		}

		void Update()
		{
			if (IsFullyIgnited && AllChimneysAreClosed() == false)
			{
				IsFullyIgnited = false;
				if (ContainsPl == false)
				{
					IsLit = false;
					ExtinguishAllConnectedFireplaces();
				}
			}

			if (IsLit)
			{
				foreach (var ps in _particles)
				{
					ps.startSpeed = IsFullyIgnited ? 5 : 2;
					//ps.colorBySpeed.color = new ParticleSystem.MinMaxGradient(ContainsPl ? Color.cyan : Color.red);
					ps.GetComponent<Renderer>().sortingOrder = IsAccessible ? 3 : 1;
					if (ps.isPlaying == false)
						ps.Play();
				}

			}
			else
			{
				foreach (var ps in _particles)
				{
					if (ps.isStopped == false)
						ps.Stop();
				}
			}
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
				{
					fireplace.IsLit = true;
					fireplace.IsFullyIgnited = IsFullyIgnited;
				}
			}
	    }

		public void ExtinguishAllConnectedFireplaces()
		{
			foreach (FirePlace fireplace in GetAllConnectedFirePlaces(this, null))
			{
				fireplace.IsLit = false;
				fireplace.IsFullyIgnited = false;
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
