using System.Collections;
using UnityEngine;

public abstract class FiringPattern : MonoBehaviour
{
	public bool ShouldFire { get; set; }

	[SerializeField] 
	protected Transform FireOrigin = default;

	public float MinTimeBetweenShots => 1 / MinRateOfFire;
	[field: SerializeField] 
	public float MinRateOfFire { get; private set; }
	public float MaxTimeBetweenShots => 1 / MaxRateOfFire;
	[field: SerializeField]
	public float MaxRateOfFire { get; private set; }

	[field: SerializeField]
	public bool ShouldBurst { get; private set; }
	[field: SerializeField]
	public int BurstAmount { get; private set; }
	public float MinTimeBetweenBursts => 1 / MinRateOfBurst;
	[field: SerializeField]
	public float MinRateOfBurst { get; private set; }
	public float MaxTimeBetweenBursts => 1 / MaxRateOfBurst;
	[field: SerializeField]
	public float MaxRateOfBurst { get; private set; }

	Coroutine _spawningCoroutine;
	private void OnEnable()
	{
		_spawningCoroutine = StartCoroutine(SpawnProjectiles());		
	}

	private void OnDisable()
	{
		StopCoroutine(_spawningCoroutine);
	}

	private IEnumerator SpawnProjectiles()
	{
		while (true)
		{
			var timeBeforeShot = Time.time;
			if (ShouldBurst)
			{
				for (int i = 0; i < BurstAmount; ++i)
				{
					DoFire();

					yield return new WaitForSeconds(Random.Range(MinTimeBetweenBursts, MaxTimeBetweenBursts));
				}
			}
			else
			{
				DoFire();
			}

			var timeAfterShot = Time.time;
			var cooldown = Random.Range(MinTimeBetweenShots, MaxTimeBetweenShots);
			if (ShouldBurst)
			{
				cooldown -= timeBeforeShot;
			}

			yield return new WaitForSeconds(cooldown);
		}
	}

	public abstract void DoFire();
}
