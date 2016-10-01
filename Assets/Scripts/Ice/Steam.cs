using Assets.Scripts.Player.Config;
using UnityEngine;

namespace Assets.Scripts.Ice
{
	public class Steam : MonoBehaviour
	{
		private float _duration;
        private ParticleSystem _particles;

		void Awake()
		{
			_duration = PlayerPrefs.GetFloat(FloatVariable.Control.ToString()) + 5;
            _particles = GetComponent<ParticleSystem>();
		}

		void Update()
		{
			_duration -= Time.deltaTime;
            if (_duration <= 5)
                _particles.loop = false;
			if (_duration <= 0)
				DestroyObject(gameObject);
		}
	}
}
