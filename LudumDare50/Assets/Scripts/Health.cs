using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field: SerializeField]
    public float MaxHealth { get; private set; }

	[SerializeField]
    private float _currHealth;

	public bool IsDead => _currHealth == 0;

	[SerializeField]
	private float _timeInvulnerableAfterHit = 0.02F;
	[SerializeField]
	private bool _shouldResurrectTriggerInvulnerability = true;

	private float _invulnerabilityTimer;

	private void Awake()
	{
		_currHealth = MaxHealth;
	}

	public void Damage(float damage, GameObject source)
	{
		if (IsDead)
		{
			return;
		}

		if (_invulnerabilityTimer > 0)
		{
			return;
		}

		_invulnerabilityTimer = _timeInvulnerableAfterHit;
		_currHealth -= damage;
		if (_currHealth <= 0)
		{
			_currHealth = 0;
			OnKilled?.Invoke(source);
		}
		else
		{ 
			OnDamaged?.Invoke(source);
		}
	}

	public void Heal(float healing)
	{
		if (IsDead)
		{
			return;
		}

		_currHealth += healing;
		if (_currHealth > MaxHealth)
		{
			_currHealth = MaxHealth;
		}
		OnHealed?.Invoke();
	}

	public void Resurrect()
	{
		if (!IsDead)
		{
			return;
		}

		_currHealth = MaxHealth;
		if (_shouldResurrectTriggerInvulnerability)
		{
			_invulnerabilityTimer = _timeInvulnerableAfterHit;
		}

		OnResurrected?.Invoke();
	}

	private void Update()
	{
		_invulnerabilityTimer -= Time.deltaTime;
	}

	public event Action<GameObject> OnKilled;
	public event Action<GameObject> OnDamaged;
	public event Action OnHealed;
	public event Action OnResurrected;
}
