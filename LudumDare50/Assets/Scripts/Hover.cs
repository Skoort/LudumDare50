using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
	[SerializeField] private Vector3 _axis = Vector3.up;
    [SerializeField] private float _delta = 0.1F;
	[SerializeField] private float _periodInSeconds = 1;

	private float _elapsedTime;

	private Vector3 _originalPosition;

	private void Awake()
	{
		_originalPosition = transform.position;
	}

	private void Update()
	{
		_elapsedTime += Time.deltaTime;

		transform.position = _originalPosition + _axis * (Mathf.Sin(_elapsedTime * Mathf.PI * 2 / _periodInSeconds) * _delta);
	}
}
