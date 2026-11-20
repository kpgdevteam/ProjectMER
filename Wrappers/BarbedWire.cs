using System.Diagnostics.CodeAnalysis;
using Hazards;
using LabApi.Features.Networking;
using Mirror;
using UnityEngine;

namespace ProjectMER.Wrappers;

public class BarbedWire
{
  private static BarbedWireHazard? prefab;

  public static Dictionary<BarbedWireHazard, BarbedWire> Dictionary { get; } = [];

  public static IReadOnlyCollection<BarbedWire> List
  {
    get => Dictionary.Values;
  }

  public static BarbedWire? Create(bool networkSpawn = true)
  {
    return Create(Vector3.zero, networkSpawn);
  }

  public static BarbedWire? Create(Vector3 position, bool networkSpawn = true)
  {
    return Create(position, Quaternion.identity, networkSpawn);
  }

  public static BarbedWire? Create(Vector3 position, Quaternion rotation, bool networkSpawn = true)
  {
    return Create(position, rotation, Vector3.one, networkSpawn);
  }

  public static BarbedWire? Create(
    Vector3 position,
    Quaternion rotation,
    Vector3 scale,
    bool networkSpawn = true)
  {
    if (!(prefab == null))
      return Get(SpawnablePrefabs.Spawn(prefab, position, rotation, scale, networkSpawn: networkSpawn));
    Logger.Warn("prefab instance was null.");
    return null;
  }

  [return: NotNullIfNotNull("barbedWire")]
  public static BarbedWire? Get(BarbedWireHazard? barbedWire)
  {
    if (barbedWire == null)
      return null;
    return !TryGet(barbedWire, out var wire) ? new BarbedWire(barbedWire) : wire;
  }

  public static bool TryGet(BarbedWireHazard? barbedWire, [NotNullWhen(true)] out BarbedWire? wire)
  {
    if (barbedWire == null)
    {
      wire = null;
      return false;
    }
    return Dictionary.TryGetValue(barbedWire, out wire) && wire != null;
  }

  internal static void Initialize()
  {
    Logger.Warn($"Prefabs: [\n{string.Join('\n', SpawnablePrefabs.List.Select(x => $"{x.name}: {string.Join(", ", x.GetComponents<NetworkBehaviour>().Select(c => c.GetType().Name))}"))}\n]");
    prefab = SpawnablePrefabs.GetAll<BarbedWireHazard>().FirstOrDefault();
    if (prefab == null)
      Logger.Error($"Failed to find prefab for {nameof(BarbedWire)}!");
  }

  internal BarbedWire(BarbedWireHazard barbedWire)
  {
    Base = barbedWire;
    IsPrefab = barbedWire.IsSpawnablePrefab();
    if (!CanCache)
      return;
    Dictionary.Add(barbedWire, this);
  }

  public BarbedWireHazard Base { get; }


  public bool IsPrefab { get; }

  public GameObject GameObject => Base.gameObject;

  public Transform Transform => Base.transform;

  public Vector3 Position
  {
    get => Transform.position;
    set => Transform.position = value;
  }

  public Quaternion Rotation
  {
    get => Transform.rotation;
    set => Transform.rotation = value;
  }

  public bool IsDestroyed
  {
    get => Base == null || GameObject == null;
  }

  public bool IsNetworkSpawned => Base.netIdentity.netId > 0U;

  public bool CanCache => !IsDestroyed && !IsPrefab && Base.isActiveAndEnabled;

  public void Spawn()
  {
    if (IsDestroyed)
      return;
    NetworkServer.Spawn(GameObject);
  }

  public void Destroy()
  {
    if (IsDestroyed)
      return;
    NetworkServer.Destroy(GameObject);
  }

  public override string ToString()
  {
    return $"[{GetType().Name}: Position={Position}, Rotation={Rotation.eulerAngles}, IsDestroyed={IsDestroyed}]";
  }

  internal static void Reset() => Dictionary.Clear();
}
