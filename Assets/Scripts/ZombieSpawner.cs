using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and manages the zombie pool.
/// Attach to an empty "ZombieSpawner" GameObject in the scene.
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Drag your zombie prefab here (with ZombieController, NavMeshAgent, Animator, Ragdoll set up)")]
    public GameObject zombiePrefab;

    [Header("Spawn Settings")]
    [Tooltip("Minimum active zombies at all times")]
    public int minZombieCount = 10;

    [Tooltip("Half-size of the arena in world units (spawns within this box)")]
    public Vector2 arenaHalfExtents = new Vector2(20f, 20f);

    [Tooltip("Minimum distance from car spawn point to avoid overlap")]
    public float minDistFromCenter = 5f;

    // ── Private ───────────────────────────────────────────────────────────────
    private List<ZombieController> _zombies = new List<ZombieController>();

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        for (int i = 0; i < minZombieCount; i++)
            SpawnNewZombie();
    }

    // ── Called by ZombieController when it needs to respawn ──────────────────
    public void RespawnZombie(ZombieController z)
    {
        z.ResetZombie(GetRandomSpawnPoint());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void SpawnNewZombie()
    {
        Vector3 pos = GetRandomSpawnPoint();
        GameObject go = Instantiate(zombiePrefab, pos,
            Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        ZombieController zc = go.GetComponent<ZombieController>();
        if (zc != null)
        {
            zc.Init(this);
            _zombies.Add(zc);
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        Vector3 pos;
        int attempts = 0;
        do
        {
            pos = new Vector3(
                Random.Range(-arenaHalfExtents.x, arenaHalfExtents.x),
                0f,
                Random.Range(-arenaHalfExtents.y, arenaHalfExtents.y)
            );
            attempts++;
        }
        while (pos.magnitude < minDistFromCenter && attempts < 20);

        return pos;
    }
}
