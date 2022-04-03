using System.Collections;
using System.Linq;
using UnityEngine;

public class ArcherEnemy : Enemy
{
	[SerializeField] private Transform _fireOrigin = default;
	[SerializeField] private Projectile _projectilePrefab = default;

	[SerializeField] private float _minStrikeRange = 5;
	[SerializeField] private float _maxStrikeRange = 8;
	private float _strikeRange;

	[SerializeField] private float _attackCooldown = 1F;
	private float _attackTimer;

	protected override void Awake()
	{
		base.Awake();

		_playerAvoidanceRadius = Random.Range(_minPlayerAvoidanceRadius, _maxPlayerAvoidanceRadius);
		_strikeRange = Random.Range(_minStrikeRange, _maxStrikeRange);
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
		}
		else
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
		// Shoot the arrow at the player.
		var position = _fireOrigin.position + DirectionToPlayer * 0.5F;

		var projectile = ObjectPool.Instance.RequestInstance<Projectile>(_projectilePrefab, desiredPosition: position, desiredRight: DirectionToPlayer);
		projectile.FiredBy = gameObject;
		projectile.Target = null;
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
	private float _avoidanceRadius;
	[SerializeField] private float _minPlayerAvoidanceRadius = 3;
	[SerializeField] private float _maxPlayerAvoidanceRadius = 4.5F;
	private float _playerAvoidanceRadius;
	[SerializeField] private float _colliderRadius = 0.25F;
	private Vector3 _avoidanceComponent;
	private IEnumerator BunchingAvoidance()
	{
		while (true)
		{
			// The avoidance radius gets smaller the closer the enemy gets to the target, capped between 1 unit away and up to 5 units away.
			var t = Mathf.Clamp01(InverseLerp(1, 5, DistanceToPlayer));
			_avoidanceRadius = Mathf.Lerp(_minAvoidanceRadius, _maxAvoidanceRadius, t);

			_avoidanceComponent = Vector3.zero;

			int numSums = 0;

			if (DistanceToPlayer < _playerAvoidanceRadius)
			{
				// Because of collider radius, we want to make avoidance strength 1 when on the border of that radius.
				var avoidanceStrength = 1 - (DistanceToPlayer - _colliderRadius) / (_playerAvoidanceRadius - _colliderRadius);
				_avoidanceComponent -= DirectionToPlayer * avoidanceStrength;
				++numSums;
			}

			var results = Physics2D.OverlapCircleAll(transform.position, _avoidanceRadius, _avoidanceLayer);
			if (results.Length > 0)
			{
				foreach (var result in results)
				{
					if (result.gameObject == gameObject)
					{
						continue;  // We found ourselves. Skip.
					}

					if (result.gameObject.tag == "Player")
					{
						continue;  // We have already handled the player's avoidance above. Skip.
					}

					var fromTo = result.gameObject.transform.position - transform.position;
					if (fromTo.magnitude <= _avoidanceRadius)
					{  // It's possible that the origin of the target is actually not within the radius.
						var avoidanceStrength = 1 - (fromTo.magnitude - _colliderRadius) / (_avoidanceRadius - _colliderRadius);
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

	private void HandleAttackInterruption()
	{
		// Handle the attack being interrupted.
	}

	protected override void OnDamaged(GameObject source)
	{
		base.OnDamaged(source);

		if (EnemyState == EEnemyState.ATTACKING)
		{
			HandleAttackInterruption();
		}
	}

	protected override void OnHealed()
	{
	}

	protected override void OnKilled(GameObject source)
	{
		base.OnKilled(source);

		_rb2d.velocity = Vector2.zero;
		StopCoroutine(_bunchingAvoidance);

		HandleAttackInterruption();

		// If the resurrection is interrupted it doesn't really matter if we got in here.
	}


	protected override void OnResurrected()
	{
		base.OnResurrected();

		// The resurrection animation can be interrupted, so do this here.
		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());
		_attackTimer = 0;
	}

	protected override void OnResurrectedFinished()
	{
		base.OnResurrectedFinished();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _playerAvoidanceRadius);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _strikeRange);
	}
}
