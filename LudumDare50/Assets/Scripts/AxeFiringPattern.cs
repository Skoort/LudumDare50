using UnityEngine;

public class AxeFiringPattern : FiringPattern
{
	[SerializeField] private Projectile _projectilePrefab = default;

	[field: SerializeField]
	public float SizeMod { get; set; } = 1;
	[field: SerializeField]
	public float DamageMod { get; set; } = 1;

	private Vector3[] _firingDirections = new Vector3[]
	{
		new Vector3(+0.4F, Mathf.Sqrt(1 - 0.4F * 0.4F)),
		new Vector3(+0.3F, Mathf.Sqrt(1 - 0.3F * 0.3F)),
		new Vector3(+0.2F, Mathf.Sqrt(1 - 0.2F * 0.2F)),
		new Vector3(+0.1F, Mathf.Sqrt(1 - 0.1F * 0.1F)),
		new Vector3(0, 1),
		new Vector3(-0.1F, Mathf.Sqrt(1 - 0.1F * 0.1F)),
		new Vector3(-0.2F, Mathf.Sqrt(1 - 0.2F * 0.2F)),
		new Vector3(-0.3F, Mathf.Sqrt(1 - 0.3F * 0.3F)),
		new Vector3(-0.4F, Mathf.Sqrt(1 - 0.4F * 0.4F)),
	};

	private Vector3 ChooseRandomThrowingDirection()
	{
		var randIndex = Random.Range(0, _firingDirections.Length);
		return _firingDirections[randIndex];
	}

	public override void DoFire()
	{
		if (ObjectPool.Instance)
		{
			var direction = ChooseRandomThrowingDirection();

			var projectile = ObjectPool.Instance.RequestInstance<Projectile>(_projectilePrefab,
				desiredPosition: FireOrigin.position,
				desiredRight: direction);
			//projectile.InheritedVelocity = _rb2d.velocity;
			projectile.FiredBy = gameObject;
			projectile.Target = null;

			// Set the upgrades on the axes.
			projectile.SizeMod = SizeMod;
			projectile.MinDamage = _projectilePrefab.MinDamage * DamageMod;
			projectile.MaxDamage = _projectilePrefab.MaxDamage * DamageMod;
		}
	}
}
