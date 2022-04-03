using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [field: SerializeField]
    public float MaxHealth { get; private set; }

    private float _currHealth;

	private void Awake()
	{
		_currHealth = MaxHealth;
	}

	public void Damage(float damage, GameObject source)
	{
		_currHealth -= damage;
		if (_currHealth <= 0)
		{
			_currHealth = 0;
			OnDamaged?.Invoke(source);
			OnKilled?.Invoke(source);
		}
		else
		{ 
			OnDamaged?.Invoke(source);
		}
	}

	public void Heal(float healing)
	{
		_currHealth += healing;
		if (_currHealth > MaxHealth)
		{
			_currHealth = MaxHealth;
		}
		OnHealed?.Invoke();
	}

	public event Action<GameObject> OnKilled;
	public event Action<GameObject> OnDamaged;
	public event Action OnHealed;
}
