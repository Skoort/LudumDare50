using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

	private void Awake()
    {
        Debug.Assert(Instance == null, "UpgradeManager.Awake: Attempted to create multiple instances of UpgradeManager.");

        Instance = this;
    }

	private KnifeFiringPattern _knifeInfo;
	private AxeFiringPattern _axeInfo;

	private void Start()
	{
		_knifeInfo = Component.FindObjectOfType<KnifeFiringPattern>();
		_axeInfo = Component.FindObjectOfType<AxeFiringPattern>();
	}

	internal void RegisterUpgrade(string weaponType)
	{
		if (weaponType == "Knife")
		{
			UpgradeDagger();
		} else
		if (weaponType == "Axe")
		{
			UpgradeAxe();
		}
		else
		{
			throw new System.Exception("Attempted to register a non-existant upgrade!");
		}
	}

	private int _currentKnifeUpgrade = -1;
	private int _currentAxeUpgrade = -1;

	public bool MaxKnifeUpgradesReached { get; private set; }
	public bool MaxAxeUpgradesReached { get; private set; }

	private void UpgradeDagger()
	{
		if (_currentKnifeUpgrade == -1)
		{
			// Activates the corresponding component w/ a default of 1 dagger.
			_knifeInfo.enabled = true;
			_knifeInfo.RecalculateKnifePositions();
		} else
		if (_currentKnifeUpgrade == 0)
		{
			// Increases the penetration of the dagger (total 2 hits).
			_knifeInfo.Penetration = 1;
		} else
		if (_currentKnifeUpgrade == 1)
		{
			// Adds a secondary dagger.
			_knifeInfo.NumKnives = 2;
			_knifeInfo.RecalculateKnifePositions();
		} else
		if (_currentKnifeUpgrade == 2)
		{
			// Increases the rate of fire (150%).
			_knifeInfo.MinRateOfFire += _knifeInfo.MinRateOfFire * 0.5F;
			_knifeInfo.MaxRateOfFire += _knifeInfo.MaxRateOfFire * 0.5F;
		} else
		if (_currentKnifeUpgrade == 3)
		{
			// Adds a tertiary dagger.
			_knifeInfo.NumKnives = 3;
			_knifeInfo.RecalculateKnifePositions();
		} else
		if (_currentKnifeUpgrade == 4)
		{
			// Increases the penetration (total 3 hits).
			_knifeInfo.Penetration = 2;
		} else
		if (_currentKnifeUpgrade == 5)
		{
			// Adds a fourth dagger.
			_knifeInfo.NumKnives = 4;
			_knifeInfo.RecalculateKnifePositions();
		} else
		if (_currentKnifeUpgrade == 6)
		{
			// Adds a fifth dagger.
			_knifeInfo.NumKnives = 5;
			_knifeInfo.RecalculateKnifePositions();

			MaxKnifeUpgradesReached = true;
		}

		++_currentKnifeUpgrade;
	}

	private void UpgradeAxe()
	{
		if (_currentAxeUpgrade == -1)
		{
			// Activates the corresponding component w/ a default of 1 axe.
			_axeInfo.enabled = true;
		} else
		if (_currentAxeUpgrade == 0)
		{
			// Increases the burst to be two axes.
			_axeInfo.BurstAmount = 2;
		} else
		if (_currentAxeUpgrade == 1)
		{
			// Increases the size of the axes and makes them do more damage.
			_axeInfo.SizeMod = 1.25F;
			_axeInfo.DamageMod = 1.5F;
		} else
		if (_currentAxeUpgrade == 2)
		{
			// Increases the burst to be three axes.
			_axeInfo.BurstAmount = 3;
		} else
		if (_currentAxeUpgrade == 3)
		{
			// Increases the size of the axes and makes them do more damage.
			_axeInfo.SizeMod = 1.5F;
			_axeInfo.DamageMod = 2;
		} else
		if (_currentAxeUpgrade == 4)
		{
			// Increases the burst to be four axes.
			_axeInfo.BurstAmount = 4;

			MaxAxeUpgradesReached = true;
		}

		++_currentAxeUpgrade;
	}


	[SerializeField] private HealthPickup _healthPickupPrefab = default;
	[SerializeField] private UpgradePickup _knifePickupPrefab = default;
	[SerializeField] private UpgradePickup _axePickupPrefab = default;

	public void AttemptToSpawnDrop(Vector3 position)
	{
		var randVal = Random.value * 100;
		if (randVal < 75)
		{
			return;
		}

		Pickup prefab = null;
		if (randVal >= 95)
		{
			if (MaxKnifeUpgradesReached)
			{
				prefab = _healthPickupPrefab;
			}
			else 
			{
				prefab = _knifePickupPrefab;
			}
		} else
		if (randVal >= 90)
		{
			if (MaxAxeUpgradesReached)
			{
				prefab = _healthPickupPrefab;
			}
			else
			{
				prefab = _axePickupPrefab;
			}
		} else
		if (randVal >= 75)
		{
			prefab = _healthPickupPrefab;
		}

		Instantiate<Pickup>(prefab, position + new Vector3(0, -0.1F, 0), Quaternion.identity, null);
	}
}
