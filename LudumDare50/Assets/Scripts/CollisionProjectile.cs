using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProjectile : Projectile
{
    [SerializeField] private Rigidbody2D _rb2d = default;

    protected override void CalculateMovement()
    {
        //_rb2d.velocity = this.transform.right * Speed;
    }

	protected void OnTriggerEnter2D(Collider2D collision)
	{
        var gameObject = collision.gameObject;
		if (ShouldRegisterHit(gameObject))
		{
            var health = gameObject.transform.root.GetComponent<Health>();
            if (health)
            {
                var damage = Random.Range(MinDamage, MaxDamage);

                health.Damage(damage, FiredBy);
            }

            OnHit(gameObject);
        }
	}

	protected override void Init()
	{
		base.Init();

        _rb2d.velocity = this.transform.right * Speed;
    }
}
