using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
	public enum EEnemyState
	{
		IDLING,
		RUNNING,
		ATTACKING,
		DEAD
	}

	protected Transform _target;
	protected Health _health;
	protected Rigidbody2D _rb2d;
	protected Animator _animator;
	protected SpriteRenderer _renderer;

	[SerializeField]
	protected float _moveSpeed = 3F;

	[field: SerializeField]  // Only for debugging.
	public EEnemyState EnemyState { get; protected set; }


	protected virtual void Awake()
	{
		_health = GetComponent<Health>();
		_rb2d = GetComponent<Rigidbody2D>();
		_animator = GetComponentInChildren<Animator>();
		_renderer = GetComponentInChildren<SpriteRenderer>();
	}

	protected virtual void Start()
	{
		_target = GameObject.FindWithTag("Player").transform;
		CalculateDesiredHeading();

		_health.OnResurrected += OnResurrected;
	}

	protected virtual void OnEnable()
	{
		_health.OnDamaged += OnDamaged;
		_health.OnKilled += OnKilled;
		_health.OnHealed += OnHealed;
	}

	protected virtual void OnDisable()
	{
		_health.OnDamaged -= OnDamaged;
		_health.OnKilled -= OnKilled;
		_health.OnHealed -= OnHealed;
	}

	protected virtual void OnDestroy()
	{
		_health.OnResurrected -= OnResurrected;  // Unlike the other callbacks, this has to happen when the enemy is dead (and disabled).
	}

	protected void CalculateDesiredHeading()
	{
		var toFrom = _target.position - transform.position;
		DirectionToPlayer = toFrom.normalized;
		DistanceToPlayer = toFrom.magnitude;
	}

	protected void LookAtDirection()
	{
		if (DirectionToPlayer.x > 0 && _renderer.flipX)
		{
			_renderer.flipX = false;
		} else
		if (DirectionToPlayer.x < 0 && !_renderer.flipX)
		{
			_renderer.flipX = true;
		}
	}


	protected Vector3 DirectionToPlayer;
	protected float DistanceToPlayer;

	protected bool _isMoving;
	protected void Update()
	{
		CalculateDesiredHeading();

		HandleTimers();

		// This condition doesn't really need to be here as the component should be disabled while in this state.
		if (EnemyState == EEnemyState.DEAD)
		{
			return;
		}

		LookAtDirection();

		switch (EnemyState)
		{
			case EEnemyState.IDLING:
			{
				_isMoving = _rb2d.velocity.magnitude > 0.1F;
				_animator.SetBool("IsMoving", _isMoving);
				HandleIdle();
				break;
			}
			case EEnemyState.RUNNING:
			{
				_isMoving = _rb2d.velocity.magnitude > 0.1F;
				_animator.SetBool("IsMoving", _isMoving);
				HandleRun();
				break;
			}
			case EEnemyState.ATTACKING:
			{
				_isMoving = false;
				_animator.SetBool("IsMoving", _isMoving);
				HandleAttack();
				break;
			}
		}
	}

	protected void LateUpdate()
	{
		// This condition doesn't really need to be here as the component should be disabled while in this state.
		if (EnemyState == EEnemyState.DEAD)
		{
			return;
		}

		HandleMovement();
	}

	protected abstract void HandleTimers();
	protected abstract void HandleIdle();
	protected abstract void HandleRun();
	protected abstract void HandleAttack();
	protected abstract void HandleMovement();

	protected virtual void BeginAttack()
	{
		EnemyState = EEnemyState.ATTACKING;

		_animator.SetTrigger("Attack");
		_isMoving = false;
		_animator.SetBool("IsMoving", _isMoving);
	}

	// Called from editor (animation).
	protected abstract void DoAttack();

	// Called from editor (animation).
	protected virtual void EndAttack()
	{
		EnemyState = EEnemyState.IDLING;

		_isMoving = false;
		_animator.SetBool("IsMoving", _isMoving);
	}

	// Called from Health.
	protected virtual void OnDamaged(GameObject source)
	{
		// This trigger may interfere with the death trigger.
		_animator.SetTrigger("Struck");  
	}

	// Called from Health.
	protected virtual void OnHealed()
	{

	}

	// Called from Health.
	protected virtual void OnKilled(GameObject source)
	{
		// While dead (at any point between falling to the floor and standing back up), states are not updated.
		enabled = false;

		EnemyState = EEnemyState.DEAD;
		_animator.SetTrigger("Die");
		gameObject.layer = LayerMask.NameToLayer("Corpse");
		gameObject.tag = "Corpse";
	}

	// Called from Health.
	protected virtual void OnResurrected()
	{
		_animator.SetTrigger("Resurrect");
		gameObject.layer = LayerMask.NameToLayer("Enemy");
		gameObject.tag = "Untagged";
	}

	// Called from editor (animation).
	protected virtual void OnResurrectedFinished()
	{
		EnemyState = EEnemyState.IDLING;

		// Allow this component to receive updates.
		enabled = true;
	}
}
