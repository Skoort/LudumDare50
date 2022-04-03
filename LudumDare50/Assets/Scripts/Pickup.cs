using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
	private bool _alreadyPickedUp;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (_alreadyPickedUp)
		{
			return;
		}

		var gameObject = collision.gameObject;
		if (gameObject.tag == "Player")
		{  // The pickups should be on a separate layer that can only collide with players anyway, but let's be safe.
			PickUp(gameObject);
			_alreadyPickedUp = true; ;
		}
		Destroy(this.gameObject);
	}

	public abstract void PickUp(GameObject playerGameObject);
}
