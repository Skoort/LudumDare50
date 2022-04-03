using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastProjectile : Projectile
{
	protected override void CalculateMovement()
	{
        var delta = (transform.right * Speed + InheritedVelocity) * Time.fixedDeltaTime;
        var deltaMagnitude = delta.magnitude;

        var hitInfo = Physics2D.Raycast(transform.position, delta, deltaMagnitude, HitLayer.value);
        if (hitInfo.transform && ShouldRegisterHit(hitInfo.transform.gameObject))
        {
            transform.position = hitInfo.point;

            var health = hitInfo.transform.root.GetComponent<Health>();
            if (health)
            {
                var damage = Random.Range(MinDamage, MaxDamage);

                health.Damage(damage, FiredBy);
            }

            OnHit(hitInfo.transform.gameObject);
        }
        else
        {
            transform.position += delta;
        }
    }
}
