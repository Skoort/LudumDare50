using UnityEngine;

public class UpgradePickup : Pickup
{
	[SerializeField] private string _weaponType;

	public override void PickUp(GameObject playerGameObject)
	{
		UpgradeManager.Instance.RegisterUpgrade(_weaponType);
	}
}
