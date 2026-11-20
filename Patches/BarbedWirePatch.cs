using HarmonyLib;
using Hazards;
using Interactables.Interobjects;
using MapGeneration;
using Mirror;
using ProjectMER.Wrappers;
using UnityEngine;

namespace ProjectMER.Patches;

/// <summary>
/// Fixes the issue that Alpha warhead kills you if you are on top of a primitive.
/// </summary>
[HarmonyPatch(typeof(BarbedWireHazard), nameof(BarbedWireHazard.Start))]
public class BarbedWirePatch
{
	public static bool Prefix(BarbedWireHazard __instance)
	{
		BarbedWire.Get(__instance);
		// Logger.Error($"BarbedWireHazard Start: {__instance.transform.position}");
		// Logger.Error($"BarbedWireHazard Start COMPONENTS: {string.Join('\n', __instance.gameObject.GetComponents<Component>().Select(c => $"[{c.name}] = {c.GetType()}"))}");
		// if (__instance.gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
		// 	Logger.Error($"BarbedWireHazard Start MESH_FILTER: {meshFilter.mesh.name} {meshFilter.mesh.triangles.Length}");
		// else Logger.Warn("COULD NOT GET BARBED_WIRE_HAZARD's MESH_FILTER");
		return true;
	}
}