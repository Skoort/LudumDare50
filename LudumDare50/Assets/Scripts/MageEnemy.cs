using System.Collections;
using System.Linq;
using UnityEngine;

public class MageEnemy : Enemy
{
	[SerializeField] private Transform _fireOrigin = default;
	[SerializeField] private Projectile _projectilePrefab = default;

	[SerializeField] private float _minStrikeRange = 5;
	[SerializeField] private float _maxStrikeRange = 8;
	private float _strikeRange;

	[SerializeField] private float _attackCooldown = 1F;
	private float _attackTimer;

	[SerializeField] private int _maxResurrects = 5;
	[SerializeField] private float _resurrectRadius = 3F;
	[SerializeField] private float _corpseViewRadius = 15;
	[SerializeField] private float _resurrectCooldown = 10F;
	private float _resurrectTimer;

	protected override void Awake()
	{
		base.Awake();

		_playerAvoidanceRadius = Random.Range(_minPlayerAvoidanceRadius, _maxPlayerAvoidanceRadius);
		_strikeRange = Random.Range(_minStrikeRange, _maxStrikeRange);
	}

	private Coroutine _bunchingAvoidance;
	private Coroutine _corpseSeeker;
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
		_resurrectTimer -= Time.deltaTime;

		// Resurrect is off cooldown, we aren't already looking for corpses, and we aren't in the process of finishing a resurrection.
		if (_resurrectTimer <= 0 && !_isLookingForCorpses && !_isResurrecting)
		{
			StartLookingForCorpses();
		}
	}

	protected override void HandleIdle()
	{
		// When you're in the IDLE state, it's harder to accidentally leave it because of bumping.
		var distanceLeeway = 0.5F;

		if (IsInRange(distanceLeeway))
		{
			if (_isLookingForCorpses && _resurrectReady && _resurrectTimer <= 0)
			{
				BeginResurrect();
			} else
			if (!_isLookingForCorpses && _attackTimer <= 0)
			{
				BeginAttack();
			}
		}
		else
		{
			EnemyState = EEnemyState.RUNNING;
		}
	}

	protected override void LookAtDirection()
	{
		if (!_isLookingForCorpses)
		{
			base.LookAtDirection();
		}
		else
		{
			var fromTo = _corpseAveragePosition - transform.position;
			if (fromTo.x > 0 && _renderer.flipX)
			{
				_renderer.flipX = false;
			} else
			if (fromTo.x < 0 && !_renderer.flipX)
			{
				_renderer.flipX = true;
			}
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
			if (_isLookingForCorpses && !_corpseTarget)
			{
				adjustedDirection = _avoidanceComponent;
			} else
			if (_isLookingForCorpses && _corpseTarget)
			{
				var fromTo = _corpseAveragePosition - transform.position;
				var dirToTarget = fromTo.normalized;
				_distanceToAveragePosition = fromTo.magnitude;
				adjustedDirection = Vector3.ClampMagnitude(dirToTarget * 0.5F + _avoidanceComponent, 1);
			}
			else
			{ 
				adjustedDirection = Vector3.ClampMagnitude(DirectionToPlayer * 0.5F + _avoidanceComponent, 1);
			}
		}

		_rb2d.velocity = adjustedDirection * _moveSpeed;
	}

	private bool IsInRange(float buffer = 0)
	{
		if (_isLookingForCorpses)
		{
			return _corpseTarget && _distanceToAveragePosition <= (0.5F * _resurrectRadius + buffer);
		}
		else
		{ 
			return DistanceToPlayer <= (_strikeRange + buffer);
		}
	}

	protected override void BeginAttack()
	{
		base.BeginAttack();
	}

	protected override void DoAttack()
	{
		if (_isResurrecting)
		{
			var corpses = Physics2D.OverlapCircleAll(transform.position, _avoidanceRadius, _avoidanceLayer)
						   .Where(x => x.gameObject.tag == "Corpse")
						   .Select(x => x.gameObject)
						   .Distinct().ToList();
			var rezCount = Mathf.Min(corpses.Count, _maxResurrects);
			for (int i = 0; i < rezCount; ++i)
			{
				var health = corpses[i].gameObject.GetComponent<Health>();
				if (health)
				{
					health.Resurrect();
				}
			}

			// Successfully got the rez off.
			_isLookingForCorpses = false;
			_corpseTarget = null;
			_distanceToAveragePosition = 0;
			_corpseAveragePosition = Vector3.zero;
			_resurrectReady = false;
		}
		else
		{
			// Shoot the fire ball at the player.
			var position = _fireOrigin.position + DirectionToPlayer * 0.5F;

			var projectile = ObjectPool.Instance.RequestInstance<Projectile>(_projectilePrefab, desiredPosition: position, desiredRight: DirectionToPlayer);
			projectile.FiredBy = gameObject;
			projectile.Target = null;
		}
	}

	protected override void EndAttack()
	{
		base.EndAttack();

		if (_isResurrecting)
		{
			_isResurrecting = false;
			_resurrectTimer = _resurrectCooldown;
		}
		else 
		{ 
			_attackTimer = _attackCooldown;  // Prepare the cooldown for the idle state.
		}
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

			// When looking for corpses, the mage is much less bothered by the player's proximity.
			var chosenDist = _isLookingForCorpses ? _avoidanceRadius : _playerAvoidanceRadius;
			if (DistanceToPlayer < chosenDist)
			{
				// Because of collider radius, we want to make avoidance strength 1 when on the border of that radius.
				var avoidanceStrength = 1 - (DistanceToPlayer - _colliderRadius) / (chosenDist - _colliderRadius);
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

					var multiplier = 1.0F;

					if (result.gameObject.tag == "Corpse" && !_isLookingForCorpses)
					{
						continue;  // When not looking for corpses, we ignore them like everyone else.
					} else
					if (result.gameObject.tag == "Corpse" && _isLookingForCorpses)
					{
						// When looking for corpses, we avoid them just barely, so that we don't go directly on top of them.
						multiplier = 0.2F;
					}

					var fromTo = result.gameObject.transform.position - transform.position;
					if (fromTo.magnitude <= _avoidanceRadius)
					{  // It's possible that the origin of the target is actually not within the radius.
						var avoidanceStrength = 1 - (fromTo.magnitude - _colliderRadius) / (_avoidanceRadius - _colliderRadius);
						_avoidanceComponent -= fromTo.normalized * (avoidanceStrength * multiplier);
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

	[SerializeField] private bool _isLookingForCorpses;
	[SerializeField] private GameObject _corpseTarget;
	[SerializeField] private Vector3 _corpseAveragePosition;
	[SerializeField] private float _distanceToAveragePosition;
	[SerializeField] private bool _resurrectReady;
	[SerializeField] private bool _isResurrecting;
	private void StartLookingForCorpses()
	{
		_isLookingForCorpses = true;

		// See if a corpse is nearby. Try up to five corpses.
		for (int i = 0; i < 5; ++i)
		{
			var corpse = GameObject.FindGameObjectWithTag("Corpse");
			if (corpse)
			{ 
				var newDirection = corpse.transform.position - transform.position;
				var newDistance = newDirection.magnitude;
				if (newDistance < _corpseViewRadius)
				{
					_corpseTarget = corpse;
					_corpseAveragePosition = corpse.transform.position;
					_distanceToAveragePosition = newDistance;
					_corpseSeeker = StartCoroutine(CorpseSeeker());
					return;
				}
			}
		}

		// Found no corpse. Wait a bit before trying again.
		_resurrectTimer = _resurrectCooldown * 0.33F;
		_isLookingForCorpses = false;
	}

	private IEnumerator CorpseSeeker()
	{
		int numDeepenings = 0;
		while (true)
		{
			_distanceToAveragePosition = Vector3.Distance(_corpseAveragePosition, transform.position);
			if (_distanceToAveragePosition <= _resurrectRadius)
			{
				int numSums = 0;
				Vector3 summedPosition = Vector3.zero;

				var results = Physics2D.OverlapCircleAll(transform.position, _avoidanceRadius, _avoidanceLayer);
				foreach (var result in results)
				{
					if (result.gameObject.tag != "Corpse")
					{
						continue;  // We are looking for corpses.
					}

					summedPosition += result.gameObject.transform.position;
					++numSums;
				}
				if (numSums >= 1)
				{
					_corpseAveragePosition = summedPosition / numSums;
					_distanceToAveragePosition = Vector3.Distance(summedPosition, transform.position);
					++numDeepenings;
					if (numDeepenings > 5)
					{
						// We only look for optimal location of corpses for a bit before resurrecting.
						_resurrectReady = true;
						yield break;
					}
				}
			}

			yield return new WaitForSeconds(0.2F);
		}
	}

	private void BeginResurrect()
	{
		_isResurrecting = true;
		BeginAttack();
	}

	private void HandleAttackInterruption()
	{
		//EnemyState = EEnemyState.IDLING;

		//_isMoving = false;
		//_animator.SetBool("IsMoving", _isMoving);

		if (_isResurrecting)
		{
			// We interrupted the resurrection animation. Make sure it finishes cleanly.
			if (_isLookingForCorpses)
			{
				// We interrupted before the resurrection could be completed. Allow it to be retried.
				_isResurrecting = false;
			}
			else
			{
				// The resurrection went off successfully, only the animation was interrupted.
				_resurrectTimer = _resurrectCooldown;
				_isResurrecting = false;
			}
		}
		else
		{
			// Handle the attack being interrupted.
		}
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
		_resurrectTimer = 0;
		_isLookingForCorpses = false;
		_corpseTarget = null;
		_distanceToAveragePosition = 0;
		_corpseAveragePosition = Vector3.zero;
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

		if (_isLookingForCorpses && _corpseTarget)
		{ 
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(_corpseTarget.transform.position, 0.1F);

			Gizmos.color = Color.black;
			Gizmos.DrawSphere(_corpseAveragePosition, 0.1F);
		}
	}
}
