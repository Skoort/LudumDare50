using UnityEngine;

public class KnifeFiringPattern : FiringPattern
{
	[SerializeField] private Transform _knifeAimPointPrefab = default;

	[field: SerializeField]
	public float MaxArc { get; set; } = 45;
	[field: SerializeField]
	public float PreferredKnifeAngle { get; set; } = 15;
	[field: SerializeField]
	public int NumKnives { get; set; } = 1;
	[field: SerializeField]
	public bool IsGhostTouch { get; set; }
	[field: SerializeField]
	public int Penetration { get; set; } = 0;

	[SerializeField]
	private Projectile _projectilePrefab = default;

	[SerializeField]
	private Rigidbody2D _rb2d = default;

	private Camera _camera;

	private void Awake()
	{
		_camera = Camera.main;
		_rb2d = GetComponent<Rigidbody2D>();

		RecalculateKnifePositions();
	}
	private Vector2 RotateVector2(Vector2 vector, float degrees)
	{
		return RotateVector2(vector, degrees);
	}

	private Vector3 RotateVector2(Vector3 vector, float degrees)
	{
		float theta = degrees * Mathf.Deg2Rad;
		Vector2 newVector;
		newVector.x = vector.x * Mathf.Cos(theta) - vector.y * Mathf.Sin(theta);
		newVector.y = vector.x * Mathf.Sin(theta) + vector.y * Mathf.Cos(theta);
		return newVector;
	}

	public void RecalculateKnifePositions()
	{
		// First calculate the angle between the knives.
		float knifeAngle;
		if (NumKnives * PreferredKnifeAngle <= MaxArc)
		{
			knifeAngle = PreferredKnifeAngle;
		}
		else
		{
			knifeAngle = MaxArc / (NumKnives - 1);
		}

		// Then calculate the direction of the leftmost knife.
		float maxAngle;
		if (NumKnives % 2 == 0)
		{
			maxAngle = (NumKnives - 1) * 0.5F * knifeAngle;
		}
		else
		{
			maxAngle = Mathf.FloorToInt(NumKnives / 2) * knifeAngle;
		}
		var currDirection = RotateVector2(FireOrigin.right, -maxAngle);

		for (int i = 0; i < NumKnives; ++i)
		{
			Transform aimPoint;
			if (i < FireOrigin.childCount)
			{
				aimPoint = FireOrigin.GetChild(i);
			}
			else
			{
				aimPoint = Instantiate(_knifeAimPointPrefab, FireOrigin);
			}
			aimPoint.position = FireOrigin.position + currDirection * 0.5F;
			aimPoint.right = currDirection;

			// Calculate the direction of the next leftmost knife.
			currDirection = RotateVector2(currDirection, knifeAngle);
		}
	}

	public override void DoFire()
	{
		if (ObjectPool.Instance)
		{
			// Calculate the aiming direction.
			var aimingPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
			var direction = (aimingPosition - FireOrigin.position).normalized;
			direction.z = 0;

			FireOrigin.right = direction;

			for (int i = 0; i < NumKnives; ++i)
			{
				var aimPoint = FireOrigin.GetChild(i);

				var projectile = ObjectPool.Instance.RequestInstance<Projectile>(_projectilePrefab, 
					desiredPosition: aimPoint.position, 
					desiredRight: aimPoint.right);
				//projectile.InheritedVelocity = _rb2d.velocity;
				projectile.FiredBy = gameObject;
				projectile.Target = null;

				// Set the upgrades on the knives.
				projectile.Penetration = Penetration;
			}
		}
	}
}
