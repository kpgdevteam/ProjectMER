using AdminToys;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;
using WaypointToy = AdminToys.WaypointToy;

namespace ProjectMER.Features.Serializable;

public class SerializableWaypoint : SerializableObject, IIndicatorDefinition
{
	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		if (instance == null || !LabApi.Features.Wrappers.WaypointToy.TryGet(instance.GetComponent<WaypointToy>(), out var waypoint))
			waypoint = LabApi.Features.Wrappers.WaypointToy.Create(
				room.GetAbsolutePosition(Position),
				room.GetAbsoluteRotation(Rotation),
				networkSpawn: false)!;
		else
		{
			waypoint.Position = room.GetAbsolutePosition(Position);
			waypoint.Rotation = room.GetAbsoluteRotation(Rotation);
		}
		_prevIndex = Index;

		waypoint.BoundsSize = Scale;
		waypoint.MovementSmoothing = 60;
		waypoint.VisualizeBounds = true;

		if (!waypoint.IsNetworkSpawned) waypoint.Spawn();

		return waypoint.Base.gameObject;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy primitive = instance == null ? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject) : instance.GetComponent<PrimitiveObjectToy>();
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);

		primitive.NetworkPrimitiveFlags = PrimitiveFlags.None;
		primitive.NetworkPrimitiveType = PrimitiveToyType.Cube;
		primitive.transform.localScale = Scale;
		primitive.transform.SetPositionAndRotation(position, rotation);

		return primitive.gameObject;
	}
}
