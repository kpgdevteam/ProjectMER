using Hazards;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableWorkstation : SerializableObject
{
	/// <summary>
	/// Gets or sets a value indicating whether the player can interact with the workstation.
	/// </summary>
	public bool IsInteractable { get; set; } = true;

	public override bool RequiresReloading => true;

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		if (instance == null || !Workstation.TryGet(instance.GetComponent<SpawnableStructure>(), out var workstation))
			workstation = Workstation.Create(
				room.GetAbsolutePosition(Position),
				room.GetAbsoluteRotation(Rotation),
				Scale,
				networkSpawn: false)!;
		else
		{
			workstation.Position = room.GetAbsolutePosition(Position);
			workstation.Rotation = room.GetAbsoluteRotation(Rotation);
		}
		_prevIndex = Index;

		workstation.Status = IsInteractable
			? WorkstationController.WorkstationStatus.Offline
			: (WorkstationController.WorkstationStatus)4;

		workstation.Spawn();

		return workstation.Base.gameObject;
	}
}
