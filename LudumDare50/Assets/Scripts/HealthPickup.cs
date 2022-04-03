using UnityEngine;

public class HealthPickup : Pickup
{
	[SerializeField] private float _healing = 50;

	public override void PickUp(GameObject playerGameObject)
	{
		var health = playerGameObject.GetComponent<Health>();
		if (health)
		{
			health.Heal(_healing);
		}
	}
}
