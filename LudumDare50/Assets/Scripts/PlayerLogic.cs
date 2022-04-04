using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
	private Animator _animator;
	private Health _health;
	private MovementController _movementController;
	[SerializeField] private FiringPattern _pattern1 = default;
	[SerializeField] private FiringPattern _pattern2 = default;

	private void Awake()
	{
		_health = GetComponent<Health>();
		_movementController = GetComponent<MovementController>();
		_animator = GetComponentInChildren<Animator>();
	}

	private void OnEnable()
	{
		_health.OnDamaged += OnDamaged;
		_health.OnKilled += OnKilled;
		_health.OnHealed += OnHealed;
	}

	private void OnDisable()
	{
		_health.OnDamaged -= OnDamaged;
		_health.OnKilled -= OnKilled;
		_health.OnHealed -= OnHealed;
	}

	private void OnDamaged(GameObject source)
	{
		_animator.SetTrigger("Struck");
	}

	private void OnKilled(GameObject source)
	{
		_animator.SetTrigger("Die");

		_movementController.CanMove = false;
		_pattern1.enabled = false;
		_pattern2.enabled = false;
	}

	private void OnHealed()
	{ 
		
	}
}
