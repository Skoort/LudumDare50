using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnTimer : MonoBehaviour
{
    [SerializeField] private GameObject[] _enemyPrefabs = default;

    [SerializeField] private float _minSpawnTime = 1;
    [SerializeField] private float _maxSpawnTime = 5;
    [SerializeField] private float _spawnRadius = 10;

    private Coroutine _spawningBehaviour;

    private void OnEnable()
    {
        _spawningBehaviour = StartCoroutine(SpawningBehaviour());
    }

    private IEnumerator SpawningBehaviour()
    {
        while (true)
        {
            var spawnPosition = Random.insideUnitCircle * _spawnRadius;

            var randomIndex = Random.Range(0, _enemyPrefabs.Length);
            var randomEnemy = _enemyPrefabs[randomIndex];

            Instantiate(randomEnemy, spawnPosition, Quaternion.identity);

            var cooldown = Mathf.Lerp(_minSpawnTime, _maxSpawnTime, Random.value);
            yield return new WaitForSeconds(cooldown);
        }
    }

	private void OnDisable()
	{
        StopCoroutine(_spawningBehaviour);
	}
}
