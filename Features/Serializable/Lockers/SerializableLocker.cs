using InventorySystem.Items.Pickups;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using UnityEngine;
using ExperimentalWeaponLocker = LabApi.Features.Wrappers.ExperimentalWeaponLocker;
using Room = LabApi.Features.Wrappers.Room;
using LabApiLocker = LabApi.Features.Wrappers.Locker;
using LapApiLockerChamber = LabApi.Features.Wrappers.LockerChamber;
using Locker = MapGeneration.Distributors.Locker;

namespace ProjectMER.Features.Serializable.Lockers;

public class SerializableLocker : SerializableObject
{
	public LockerType LockerType { get; set; } = LockerType.PedestalScp500;

	public List<SerializableLockerLoot> Loot { get; set; } = [];

	public List<SerializableLockerChamber> Chambers { get; set; } = [];

	private static LabApiLocker CreateLocker(LockerType type, Vector3 position, Quaternion rotation)
	{
		return type switch
		{
			LockerType.Misc => LabApiLocker.Create(LockerStructureType.Standard,
				position, rotation, networkSpawn: false)!,
			LockerType.Medkit => LabApiLocker.Create(LockerStructureType.PainkillerCabinet,
				position, rotation, networkSpawn: false)!,
			LockerType.Adrenaline => LabApiLocker.Create(LockerStructureType.AdrenalineCabinet,
				position, rotation, networkSpawn: false)!,
			LockerType.RifleRack => LabApiLocker.Create(LockerStructureType.RifleRack,
				position, rotation, networkSpawn: false)!,
			LockerType.LargeGun => LabApiLocker.Create(LockerStructureType.Large,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalAntiScp207 => PedestalLocker.Create(PedestalLockerType.AntiScp207,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp018 => PedestalLocker.Create(PedestalLockerType.Scp018,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp207 => PedestalLocker.Create(PedestalLockerType.Scp207,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp244 => PedestalLocker.Create(PedestalLockerType.Scp244,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp268 => PedestalLocker.Create(PedestalLockerType.Scp268,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp500 => PedestalLocker.Create(PedestalLockerType.Scp500,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp1344 => PedestalLocker.Create(PedestalLockerType.Scp1344,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp1509 => PedestalLocker.Create(PedestalLockerType.Scp1509,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp1853 => PedestalLocker.Create(PedestalLockerType.Scp1853,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp2176 => PedestalLocker.Create(PedestalLockerType.Scp2176,
				position, rotation, networkSpawn: false)!,
			LockerType.PedestalScp1576 => PedestalLocker.Create(PedestalLockerType.Scp1576,
				position, rotation, networkSpawn: false)!,
			LockerType.ExperimentalWeapon => ExperimentalWeaponLocker.Create(
				position, rotation, networkSpawn: false)!,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		if (instance == null || !LabApiLocker.TryGet(instance.GetComponent<Locker>(), out var locker))
			locker = CreateLocker(LockerType,
				room.GetAbsolutePosition(Position),
				room.GetAbsoluteRotation(Rotation));
		else
		{
			locker.Position = room.GetAbsolutePosition(Position);
			locker.Rotation = room.GetAbsoluteRotation(Rotation);
		}
		_prevIndex = Index;

		if (LockerType != _prevType)
			SetDefaultSettings(locker);

		locker.ClearLockerLoot();
		foreach (SerializableLockerLoot loot in Loot)
		{
			locker.AddLockerLoot(loot.TargetItem, loot.RemainingUses, loot.ProbabilityPoints, loot.MinPerChamber, loot.MaxPerChamber);
		}

		int i = 0;
		locker.ClearAllChambers();
		foreach (LapApiLockerChamber chamber in locker.Chambers)
		{
			if (i > Chambers.Count - 1)
				break;

			chamber.AcceptableItems = Chambers[i].AcceptableItems.ToArray();
			chamber.RequiredPermissions = Chambers[i].RequiredPermissions;
			i++;
		}

		_prevType = LockerType;
		if (!locker.IsNetworkSpawned)
			locker.Spawn();

		Timing.CallDelayed(0.25f, () =>
		{
			foreach (ItemPickupBase itemPickupBase in locker.Base.GetComponentsInChildren<ItemPickupBase>())
			{
				if (itemPickupBase.TryGetComponent(out Rigidbody rigidbody))
					rigidbody.isKinematic = false;
			}

			int i = 0;
			foreach (LapApiLockerChamber chamber in locker.Chambers)
			{
				chamber.IsOpen = Chambers[i].IsOpen;
				i++;
			}
		});

		return locker.Base.gameObject;
	}

	private void SetDefaultSettings(LabApiLocker labApiLocker)
	{
		Loot.Clear();
		Chambers.Clear();

		foreach (LockerLoot loot in labApiLocker.Loot)
		{
			Loot.Add(new SerializableLockerLoot(loot.TargetItem, loot.RemainingUses, loot.MaxPerChamber, loot.ProbabilityPoints, loot.MinPerChamber));
		}

		foreach (LapApiLockerChamber chamber in labApiLocker.Chambers)
		{
			Chambers.Add(new SerializableLockerChamber(chamber.AcceptableItems, chamber.IsOpen, chamber.RequiredPermissions));
		}
	}

	private Locker LockerPrefab
	{
		get
		{
			Locker prefab = LockerType switch
			{
				LockerType.PedestalScp500 => PrefabManager.PedestalScp500,
				LockerType.LargeGun => PrefabManager.LockerLargeGun,
				LockerType.RifleRack => PrefabManager.LockerRifleRack,
				LockerType.Misc => PrefabManager.LockerMisc,
				LockerType.Medkit => PrefabManager.LockerRegularMedkit,
				LockerType.Adrenaline => PrefabManager.LockerAdrenalineMedkit,
				LockerType.PedestalScp018 => PrefabManager.PedestalScp018,
				LockerType.PedestalScp207 => PrefabManager.PedstalScp207,
				LockerType.PedestalScp244 => PrefabManager.PedestalScp244,
				LockerType.PedestalScp268 => PrefabManager.PedestalScp268,
				LockerType.PedestalScp1853 => PrefabManager.PedstalScp1853,
				LockerType.PedestalScp2176 => PrefabManager.PedestalScp2176,
				LockerType.PedestalScp1576 => PrefabManager.PedestalScp1576,
				LockerType.PedestalAntiScp207 => PrefabManager.PedestalAntiScp207,
				LockerType.PedestalScp1344 => PrefabManager.PedestalScp1344,
				LockerType.ExperimentalWeapon => PrefabManager.LockerExperimentalWeapon,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => true;

	internal LockerType _prevType = LockerType.None;
}
