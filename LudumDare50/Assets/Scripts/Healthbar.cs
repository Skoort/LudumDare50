using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
	[SerializeField] private Health _health = default;
	[SerializeField] private Image _image = default;

	private float _currValue;

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

	private void Start()
	{
		RefreshUI();
	}

	private void OnDamaged(GameObject source)
	{
		RefreshUI();
	}

	private void OnKilled(GameObject source)
	{
		RefreshUI();
	}

	private void OnHealed()
	{
		RefreshUI();
	}

	private void RefreshUI()
	{ 
		_currValue = _health.CurrHealth / _health.MaxHealth;
		_image.fillAmount = _currValue;
	}
}
