using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field: SerializeField]
    public float MaxHealth { get; private set; }

	[field: SerializeField]
	public float CurrHealth { get; private set; }

	public bool IsDead => CurrHealth == 0;

	[SerializeField]
	private float _timeInvulnerableAfterHit = 0.02F;
	[SerializeField]
	private bool _shouldResurrectTriggerInvulnerability = true;

	private float _invulnerabilityTimer;

	private void Awake()
	{
		CurrHealth = MaxHealth;
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
		CurrHealth -= damage;
		if (CurrHealth <= 0)
		{
			CurrHealth = 0;
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

		CurrHealth += healing;
		if (CurrHealth > MaxHealth)
		{
			CurrHealth = MaxHealth;
		}
		OnHealed?.Invoke();
	}

	public void Resurrect()
	{
		if (!IsDead)
		{
			return;
		}

		CurrHealth = MaxHealth;
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
