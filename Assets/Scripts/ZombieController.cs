using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls a single zombie NPC.
/// 
/// Setup per zombie prefab:
///  - NavMeshAgent component (for wandering)
///  - Animator component with a state that has parameter "isWalking" (bool) 
///    and "ragdoll" trigger  — OR simply disable Animator on hit
///  - Rigidbody + Collider on root (kinematic while alive)
///  - Ragdoll: use Unity Ragdoll Wizard on the skeleton; all bone Rigidbodies
///    start kinematic = true. This script flips them on hit.
///  - Tag the root collider as "Zombie"
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius   = 15f;
    public float minWaitTime    = 1f;
    public float maxWaitTime    = 3f;

    [Header("Ragdoll")]
    [Tooltip("How much force is applied to the hit bone on collision")]
    public float ragdollForceMultiplier = 3f;

    [Header("Respawn")]
    public float respawnDelay = 2.5f; // 2–3 s as required

    // ── Private refs ──────────────────────────────────────────────────────────
    private NavMeshAgent  _agent;
    private Animator      _anim;
    private Rigidbody     _rootRb;
    private Collider      _rootCol;
    private Rigidbody[]   _ragdollBodies;
    private Collider[]    _ragdollColliders;

    private bool   _isRagdoll;
    private bool   _isRespawning;
    private ZombieSpawner _spawner;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _anim    = GetComponent<Animator>();
        _rootRb  = GetComponent<Rigidbody>();
        _rootCol = GetComponent<Collider>();

        // Collect all child ragdoll rigidbodies (exclude root)
        _ragdollBodies    = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdollActive(false); // start in animated mode
    }

    void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    // ── Called by ZombieSpawner after instantiation ───────────────────────────
    public void Init(ZombieSpawner spawner)
    {
        _spawner = spawner;
    }

    // ── Wander ────────────────────────────────────────────────────────────────
    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (_isRagdoll || _isRespawning)
            {
                yield return null;
                continue;
            }

            Vector3 dest = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
            if (dest != Vector3.zero)
            {
                _agent.SetDestination(dest);
                if (_anim != null) _anim.SetFloat("MoveSpeed", 1f);
            }

            // Wait until we reach destination (or time out)
            float timeout = 8f;
            while (!_isRagdoll && !_isRespawning &&
                   (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance))
            {
                timeout -= Time.deltaTime;
                if (timeout <= 0f) break;
                yield return null;
            }

            // Short idle pause
            if (!_isRagdoll && !_isRespawning)
            {
                if (_anim != null) _anim.SetFloat("MoveSpeed", 0f);
                yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            }
        }
    }

    // ── Hit by car ────────────────────────────────────────────────────────────
    void OnCollisionEnter(Collision col)
    {
        if (_isRagdoll || _isRespawning) return;
        if (!col.gameObject.CompareTag("Player")) return;

        // Direction of impact
        Vector3 hitForce = col.relativeVelocity * ragdollForceMultiplier;

        ActivateRagdoll(hitForce, col.contacts[0].point);
        GameManager.Instance?.AddScore(1);
        StartCoroutine(RespawnRoutine());
    }

    // ── Ragdoll ───────────────────────────────────────────────────────────────
    public void ActivateRagdoll(Vector3 force, Vector3 hitPoint)
    {
        if (_isRagdoll) return;
        _isRagdoll = true;

        // Stop navigation
        _agent.enabled = false;

        // Stop animation
        if (_anim != null)
        {
            _anim.SetFloat("MoveSpeed", 0f);
            _anim.enabled = false;
        }

        SetRagdollActive(true);

        // Apply impulse to the nearest ragdoll bone to hit point
        Rigidbody closestBone = GetClosestBone(hitPoint);
        if (closestBone != null)
            closestBone.AddForce(force, ForceMode.Impulse);
    }

    void SetRagdollActive(bool active)
    {
        foreach (Rigidbody rb in _ragdollBodies)
        {
            if (rb == _rootRb) continue; // root handled separately
            rb.isKinematic = !active;
        }
        foreach (Collider c in _ragdollColliders)
        {
            if (c == _rootCol) continue;
            c.enabled = active;
        }

        // Root rigidbody
        if (_rootRb != null)
            _rootRb.isKinematic = active; // root becomes kinematic once ragdoll takes over
        if (_rootCol != null)
            _rootCol.enabled = !active;   // disable root trigger so we don't double-score
    }

    Rigidbody GetClosestBone(Vector3 point)
    {
        Rigidbody closest = null;
        float minDist = float.MaxValue;
        foreach (Rigidbody rb in _ragdollBodies)
        {
            float d = Vector3.Distance(rb.position, point);
            if (d < minDist) { minDist = d; closest = rb; }
        }
        return closest;
    }

    // ── Respawn ───────────────────────────────────────────────────────────────
    IEnumerator RespawnRoutine()
    {
        _isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        if (_spawner != null)
            _spawner.RespawnZombie(this);
        else
            ResetZombie(_spawner != null ? _spawner.GetRandomSpawnPoint() : transform.position);
    }

    /// <summary>Resets this zombie at a new world position.</summary>
    public void ResetZombie(Vector3 position)
    {
        _isRagdoll    = false;
        _isRespawning = false;

        // Teleport
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // Re-enable animated mode
        SetRagdollActive(false);

        if (_anim != null)
            _anim.enabled = true;

        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.Warp(position);
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    static Vector3 RandomNavSphere(Vector3 origin, float radius, int layerMask)
    {
        Vector3 randDir = Random.insideUnitSphere * radius + origin;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randDir, out hit, radius, layerMask))
            return hit.position;
        return Vector3.zero;
    }
}
