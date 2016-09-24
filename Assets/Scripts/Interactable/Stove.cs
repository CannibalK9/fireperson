using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interactable
{
    public class Stove : FirePlace
    {
		private ParticleSystem _particles;

		void Start()
		{
			_particles = GetComponentInChildren<ParticleSystem>();
			if (IsFullyIgnited)
				LightAllConnectedFireplaces();
		}

		void Update()
		{
			if (IsFullyIgnited)
			{
				if (ContainsPl == false && AllChimneysAreClosed() == false)
				{
					IsFullyIgnited = false;
					IsLit = false;
					if (HasConnectedFullyLitStove() == false)
						ExtinguishAllConnectedFireplaces();
				}
			}

			if (IsLit)
			{
				_particles.startSpeed = IsFullyIgnited ? 5 : 2;
				//_particles.colorBySpeed.color = new ParticleSystem.MinMaxGradient(ContainsPl ? Color.cyan : Color.red);
				_particles.GetComponent<Renderer>().sortingOrder = IsAccessible ? 3 : 1;
				if (_particles.isPlaying == false)
					_particles.Play();
			}
			else
				if (_particles.isStopped == false)
					_particles.Stop();
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
