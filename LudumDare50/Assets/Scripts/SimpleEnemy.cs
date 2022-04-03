using System.Collections;
using UnityEngine;

public class SimpleEnemy : Enemy
{
	[SerializeField] private float _strikeRange = 1F;
	[SerializeField] private float _attackCooldown = 1F;
	private float _attackTimer;

	[SerializeField]
	private bool _spontaneouslyCombust = false;

	protected override void Start()
	{
		base.Start();

		if (_spontaneouslyCombust)
		{
			_health.Damage(_health.MaxHealth, null);
		}
	}

	private Coroutine _bunchingAvoidance;
	protected override void OnEnable()
	{
		base.OnEnable();

		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		StopCoroutine(_bunchingAvoidance);
	}

	protected override void HandleTimers()
	{
		_attackTimer -= Time.deltaTime;
	}

	protected override void HandleIdle()
	{
		// When you're in the IDLE state, it's harder to accidentally leave it because of bumping.
		var distanceLeeway = 0.5F;

		if (IsInRange(distanceLeeway))
		{
			if (_attackTimer <= 0)
			{
				BeginAttack();
			}
		}
		else
		{
			EnemyState = EEnemyState.RUNNING;
		}
	}

	protected override void HandleRun()
	{
		if (IsInRange())
		{
			EnemyState = EEnemyState.IDLING;
		}
	}

	protected override void HandleAttack()
	{  // Does nothing.
	}

	protected override void HandleMovement()
	{
		Vector3 adjustedDirection = Vector3.zero;
		if (EnemyState == EEnemyState.IDLING)
		{  // The enemy has already reached the player so it just has to avoid obstacles.
			adjustedDirection = _avoidanceComponent;
		} else
		if (EnemyState == EEnemyState.RUNNING)
		{  // The enemy has yet to reach the player.
			adjustedDirection = Vector3.ClampMagnitude(DirectionToPlayer * 0.5F + _avoidanceComponent, 1);
		}

		_rb2d.velocity = adjustedDirection * _moveSpeed;
	}

	private bool IsInRange(float buffer = 0)
	{
		return DistanceToPlayer <= (_strikeRange + buffer);
	}

	protected override void BeginAttack()
	{
		base.BeginAttack();
	}

	protected override void DoAttack()
	{
		// TODO
	}

	protected override void EndAttack()
	{
		base.EndAttack();

		_attackTimer = _attackCooldown;  // Prepare the cooldown for the idle state.
	}

	private float InverseLerp(float a, float b, float v)
	{
		return (v - a) / (b - a);
	}

	[SerializeField] private LayerMask _avoidanceLayer = default;
	[SerializeField] private float _minAvoidanceRadius = 0.5F;
	[SerializeField] private float _maxAvoidanceRadius = 3;
	[SerializeField] private float _colliderRadius = 0.25F;
	private float _avoidanceRadius;
	private Vector3 _avoidanceComponent;
	private IEnumerator BunchingAvoidance()
	{
		while (true)
		{
			// The avoidance radius gets smaller the closer the enemy gets to the target,
			// capped between striking range and up to 5 units away.
			var t = Mathf.Clamp01(InverseLerp(_strikeRange, 5, DistanceToPlayer));
			_avoidanceRadius = Mathf.Lerp(_minAvoidanceRadius, _maxAvoidanceRadius, t);

			_avoidanceComponent = Vector3.zero;

			var results = Physics2D.OverlapCircleAll(transform.position, _avoidanceRadius, _avoidanceLayer);
			if (results.Length > 0)
			{
				int numSums = 0;
				foreach (var result in results)
				{
					if (result.gameObject == gameObject)
					{
						continue;  // We found ourselves. Skip.
					}

					var tempAvoidanceRadius = _avoidanceRadius;
					if (result.gameObject.tag == "Player" && _avoidanceRadius > _strikeRange)
					{
						tempAvoidanceRadius = _strikeRange;
					}

					var fromTo = result.gameObject.transform.position - transform.position;
					if (fromTo.magnitude <= tempAvoidanceRadius)
					{  // It's possible that the origin of the target is actually not within the radius.
						var avoidanceStrength = 1 - (fromTo.magnitude - _colliderRadius) / (tempAvoidanceRadius - _colliderRadius);
						_avoidanceComponent -= fromTo.normalized * avoidanceStrength;
						++numSums;
					}
				}
				if (numSums > 1)
				{
					_avoidanceComponent /= numSums;
				}
			}

			yield return new WaitForSeconds(0.1F);
		}
	}

	protected override void OnDamaged(GameObject source)
	{
		base.OnDamaged(source);
	}

	protected override void OnHealed()
	{
	}

	protected override void OnKilled(GameObject source)
	{
		base.OnKilled(source);

		_rb2d.velocity = Vector2.zero;
		StopCoroutine(_bunchingAvoidance);
	}

	protected override void OnResurrected()
	{
		base.OnResurrected();

		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());
	}

	protected override void OnResurrectedFinished()
	{
		base.OnResurrectedFinished();
	}
}
