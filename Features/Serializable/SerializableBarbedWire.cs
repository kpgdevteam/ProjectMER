using Hazards;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Wrappers;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableBarbedWire : SerializableObject
{
	public override bool RequiresReloading => true;

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		var pos = room.GetAbsolutePosition(Position);
		var rot = room.GetAbsoluteRotation(Rotation);

		if (instance == null || !BarbedWire.TryGet(instance.GetComponent<BarbedWireHazard>(), out var barbedWire))
			barbedWire = BarbedWire.Create(
				pos,
				rot,
				Scale,
				networkSpawn: false)!;
		else
		{
			barbedWire.Position = pos;
			barbedWire.Rotation = rot;
		}
		_prevIndex = Index;

		var l = barbedWire.GameObject.layer;
		Logger.Info($"Layer = {l}");
		for (var i = 1; i <= 1 << sizeof(int); i <<= 1)
		{
			if ((l & i) == 0) continue;
			Logger.Info($"LAYER PRESENT = {LayerMask.LayerToName(i)}");
		}

		barbedWire.Spawn();

		return barbedWire.Base.gameObject;
	}
}
