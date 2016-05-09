using Assets.Scripts.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Sprites
{
	public class StiltDestruction : MonoBehaviour
	{
		public GameObject AttachedBuilding;

        public bool IsBurning { get; set; }

        private List<ParticleSystem> _particles;
        private HingeJoint2D _hingeJoint;
        private GameObject _stilt;
        private float _burnUpDuration = 3f;
        private bool _jointsDestroyed;

        void Awake()
        {
            _hingeJoint = gameObject.GetComponent<HingeJoint2D>();
            _stilt = _hingeJoint.connectedBody.gameObject;
        }

        void Update()
        {
            if (IsBurning)
                _burnUpDuration -= Time.deltaTime;

            //if (_burnUpDuration < 3f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[0].emission;
            //    emission.enabled = true;
            //}
            //else if (_burnUpDuration < 2.5f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[1].emission;
            //    emission.enabled = true;
            //}

            //else if (_burnUpDuration < 2f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[2].emission;
            //    emission.enabled = true;
            //}

            //else if (_burnUpDuration < 1.5f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[3].emission;
            //    emission.enabled = true;
            //}

            //else if (_burnUpDuration < 1f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[4].emission;
            //    emission.enabled = true;
            //}

            //else if (_burnUpDuration < 0.5f)
            //{
            //    ParticleSystem.EmissionModule emission = _particles[5].emission;
            //    emission.enabled = true;
            //}

            if (_burnUpDuration < 0)
            {
                IsBurning = false;
                BurnStilt();

                //ParticleSystem.EmissionModule emission = _particles[6].emission;
                //emission.enabled = true;
            }
        }

        public void BurnStilt()
		{
			if (AttachedBuilding != null)
			{
				Joint2D[] joints = AttachedBuilding.GetComponents<Joint2D>();
				foreach (Joint2D joint in joints)
				{
					joint.enabled = false;
				}
			}
            Destroy(_stilt);
			_hingeJoint.enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
			gameObject.layer = LayerMask.NameToLayer(Layers.Background);
		}
	}
}
