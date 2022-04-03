using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
	public enum EEnemyState
	{
		IDLING,
		RUNNING,
		ATTACKING,
		DEAD
	}

	[SerializeField] private Transform _target = default;

	[SerializeField] private float _moveSpeed = 3F;
	[SerializeField] private float _strikeRange = 1F;

	[SerializeField] private Animator _animator = default;
	[SerializeField] private SpriteRenderer _renderer = default;
	private Rigidbody2D _rb2d;

	private Health _health;

	private void Awake()
	{
		_rb2d = GetComponent<Rigidbody2D>();

		if (!_animator)
		{
			_animator = GetComponent<Animator>();
		}

		if (!_renderer)
		{
			_renderer = GetComponent<SpriteRenderer>();
		}

		_target = GameObject.FindWithTag("Player").transform;
		var toFrom = _target.position - transform.position;
		_desiredDirection = toFrom.normalized;
		_distanceToTarget = toFrom.magnitude;

		_health = GetComponent<Health>();
	}

	private void Start()
	{
		_health.OnResurrected += OnResurrected;
	}

	private Coroutine _bunchingAvoidance;
	private void OnEnable()
	{
		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());

		_health.OnDamaged += OnDamaged;
		_health.OnKilled += OnKilled;
		_health.OnHealed += OnHealed;
	}

	private void OnDisable()
	{
		StopCoroutine(_bunchingAvoidance);

		_health.OnDamaged -= OnDamaged;
		_health.OnKilled -= OnKilled;
		_health.OnHealed -= OnHealed;
	}

	private void OnDestroy()
	{
		_health.OnResurrected -= OnResurrected;  // Unlike the other callbacks, this has to happen when the enemy is dead (and disabled).
	}

	[field: SerializeField]
	public EEnemyState EnemyState { get; private set; }

	private Vector3 _desiredDirection;
	private float _distanceToTarget;

	private bool _isMoving;
	[SerializeField] private float _attackCooldown = 1F;
	private float _attackTimer;
	private void Update()
	{
		var toFrom = _target.position - transform.position;
		_desiredDirection = toFrom.normalized;
		_distanceToTarget = toFrom.magnitude;

		if (EnemyState == EEnemyState.DEAD)
		{
			_attackTimer -= Time.deltaTime / _attackCooldown;
			return;
		}

		if (_desiredDirection.x > 0 && _renderer.flipX)
		{
			_renderer.flipX = false;
		} else
		if (_desiredDirection.x < 0 && !_renderer.flipX)
		{
			_renderer.flipX = true;
		}

		switch (EnemyState)
		{
			case EEnemyState.IDLING:
			{
				if (IsInRange(buffer: 0.5F))
				{
					if (_attackTimer <= 0)
					{
						BeginAttack();
					}
				}
				else
				{
					// When you're in the IDLE state, it's harder to accidentally leave it because of bumping.
					EnemyState = EEnemyState.RUNNING;
				}
				break;
			}
			case EEnemyState.RUNNING:
			{
				if (IsInRange())
				{
					EnemyState = EEnemyState.IDLING;
				}
				break;
			}
		}
	}

	private void FixedUpdate()
	{
		Vector3 adjustedDirection = Vector3.zero;
		if (EnemyState == EEnemyState.IDLING)
		{  // The enemy has already reached the player so it just has to avoid obstacles.
			adjustedDirection = _avoidanceComponent;
		} else
		if (EnemyState == EEnemyState.RUNNING)
		{  // The enemy has yet to reach the player.
			adjustedDirection = Vector3.ClampMagnitude(_desiredDirection * 0.5F + _avoidanceComponent, 1);
		} else
		if (EnemyState == EEnemyState.DEAD)
		{
			return;
		}

		_isMoving = _rb2d.velocity.magnitude > 0.1F;
		_animator.SetBool("IsMoving", _isMoving);
		//if (_isMoving && adjustedDirection.magnitude < 0.1F)
		//{
		//	_isMoving = false;
		//	_animator.SetBool("IsMoving", _isMoving);
		//}
		//else
		//if (!_isMoving && adjustedDirection.magnitude >= 0.1F && EnemyState != EEnemyState.ATTACKING)
		//{
		//	_isMoving = true;
		//	_animator.SetBool("IsMoving", _isMoving);
		//}
		//_isMoving = adjustedDirection.magnitude > 0.1F;
		//_animator.SetBool("IsMoving", _isMoving);
		//if (!_isMoving)
		//{
		//	adjustedDirection = Vector3.zero;
		//}

		_rb2d.velocity = adjustedDirection * _moveSpeed;
	}

	private bool IsInRange(float buffer = 0)
	{
		return _distanceToTarget <= (_strikeRange + buffer);
	}

	private void BeginAttack()
	{
		EnemyState = EEnemyState.ATTACKING;
		_attackTimer = _attackCooldown;
		_animator.SetTrigger("Attack");
		_isMoving = false;
	}

	private void DoAttack()
	{

	}

	private void EndAttack()
	{
		EnemyState = EEnemyState.IDLING;
		_attackTimer = _attackCooldown;
		_isMoving = false;
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
			var t = Mathf.Clamp01(InverseLerp(_strikeRange, 5, _distanceToTarget));
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

	private void OnDamaged(GameObject source)
	{
		if (!_health.IsDead)
		{ 
			_animator.SetTrigger("Struck");  // This trigger interferes with the death trigger.
		}
	}

	private void OnHealed()
	{ 
	
	}

	private void OnKilled(GameObject source)
	{ 
		EnemyState = EEnemyState.DEAD;
		_rb2d.velocity = Vector2.zero;
		_animator.SetTrigger("Die");
		gameObject.layer = LayerMask.NameToLayer("Corpse");

		StopCoroutine(_bunchingAvoidance);
	}

	private void OnResurrected()
	{
		_animator.SetTrigger("Resurrect");
		gameObject.layer = LayerMask.NameToLayer("Enemy");

		_bunchingAvoidance = StartCoroutine(BunchingAvoidance());
	}

	private void OnResurrectionFinished()
	{
		EnemyState = EEnemyState.IDLING;
	}
}
