using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFiringPattern : FiringPattern
{
	[SerializeField]
	private Projectile _projectilePrefab = default;

	[SerializeField]
	private Rigidbody2D _rb2d = default;

	private Camera _camera;

	private void Awake()
	{
		_camera = Camera.main;
		_rb2d = GetComponent<Rigidbody2D>();
	}

	public override void DoFire()
	{
		if (ObjectPool.Instance)
		{ 
			var aimingPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

			var direction = (aimingPosition - FireOrigin.position).normalized;
			direction.z = 0;
			var position = FireOrigin.position + direction * 0.5F;

			var projectile = ObjectPool.Instance.RequestInstance<Projectile>(_projectilePrefab, desiredPosition: position, desiredRight: direction);
			projectile.FiredBy = gameObject;
			projectile.Target = null;
			//projectile.InheritedVelocity = _rb2d.velocity;
		}
	}
}
