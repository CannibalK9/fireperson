using UnityEngine;

namespace Assets.Scripts.Environment
{
	public class CloudSpawner : MonoBehaviour
	{
		private GameObject _cloud;
		private Random rnd = new Random();
		private float _timer;
		private float _spawnTime;
		private Camera _camera;

		void Awake()
		{
			_cloud = (GameObject)Resources.Load("background/Skybox/cloud");
			_camera = Camera.main;
		}

		void Update()
		{
			_timer += Time.deltaTime;
			if (_timer > _spawnTime)
			{
				GameObject cloud = (GameObject)Instantiate(_cloud, new Vector3(Random.Range(0, 130), _camera.transform.position.y + 10f, 0), Quaternion.Euler(0, 0, Random.Range(0, 360)));
				cloud.transform.parent = transform;
				_timer = 0;
				_spawnTime = Random.Range(2, 10);
			}
		}
	}
}
