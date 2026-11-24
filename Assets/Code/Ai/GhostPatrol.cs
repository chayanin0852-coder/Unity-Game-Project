
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostPatrol : MonoBehaviour
{
    private enum AIState
    {
        Patrol,
        Investigate,
        Chase,
        Search
    }

    [Header("Player Reference")]
    [SerializeField] private PlayerMove player;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 18f;
    [SerializeField, Range(0f, 180f)]
    private float fieldOfView = 120f;
    [SerializeField] private float eyeHeight = 1.3f;
    [SerializeField] private float playerAimHeight = 1f;
    [SerializeField] private float visionCheckInterval = 0.15f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 15f;
    [SerializeField] private float minTravelDistance = 4f;
    [SerializeField] private float sampleDistance = 3f;
    [SerializeField] private int maxAttempts = 30;

    [Header("Patrol Wait Settings")]
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 2f;

    [Header("Destination Memory")]
    [SerializeField] private int rememberedDestinations = 6;
    [SerializeField] private float avoidRepeatDistance = 3f;

    [Header("Patrol Movement")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float patrolAngularSpeed = 300f;
    [SerializeField] private float patrolAcceleration = 12f;
    [SerializeField] private float patrolStoppingDistance = 0.35f;

    [Header("Investigate Movement")]
    [SerializeField] private float investigateSpeed = 4.2f;
    [SerializeField] private float investigateSampleDistance = 4f;
    [SerializeField] private float investigateMinWaitTime = 4f;
    [SerializeField] private float investigateMaxWaitTime = 7f;
    [SerializeField] private float minNoiseUpdateDistance = 1.5f;

    [Header("Chase Movement")]
    [SerializeField] private float chaseSpeed = 7.5f;
    [SerializeField] private float chaseAngularSpeed = 420f;
    [SerializeField] private float chaseAcceleration = 18f;
    [SerializeField] private float chaseStoppingDistance = 1.2f;
    [SerializeField] private float chaseDestinationInterval = 0.25f;
    [SerializeField] private float chaseSampleDistance = 3f;

    [Header("Chase Detection")]
    [SerializeField] private float chaseNoiseRadiusThreshold = 14f;
    [SerializeField] private float loseSightDuration = 2.5f;

    [Header("Search Settings")]
    [SerializeField] private float searchDuration = 8f;
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private float searchSampleDistance = 3f;
    [SerializeField] private float searchMinPause = 1.5f;
    [SerializeField] private float searchMaxPause = 2.5f;

    private NavMeshAgent agent;
    private NavMeshPath path;

    private readonly List<Vector3> recentDestinations =
        new List<Vector3>();

    private AIState currentState = AIState.Patrol;

    private bool hasPatrolDestination;
    private bool isWaiting;
    private bool warnedOffMesh;
    private bool canSeePlayer;

    private float waitTimer;
    private float searchTimer;

    private float nextVisionCheckTime;
    private float nextChaseDestinationTime;
    private float nextSearchDestinationTime;
    private float lastPlayerEvidenceTime;

    private Vector3 patrolDestination;
    private Vector3 investigateDestination;
    private Vector3 lastHeardPosition;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 searchCenter;

    public string CurrentState => currentState.ToString();

    private void OnEnable()
    {
        GhostNoiseSystem.NoiseEmitted += HandleNoise;
    }

    private void OnDisable()
    {
        GhostNoiseSystem.NoiseEmitted -= HandleNoise;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        if (player == null)
            player = FindFirstObjectByType<PlayerMove>();

        ApplyPatrolMovement();

        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updateRotation = true;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogWarning(
                "[GhostAI] PlayerMove not found. " +
                "Assign Player in the Inspector.",
                this
            );
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "[GhostAI] Ghost is not on NavMesh.",
                this
            );
        }
    }

    private void Update()
    {
        if (agent == null || !agent.enabled)
            return;

        if (!agent.isOnNavMesh)
        {
            if (!warnedOffMesh)
            {
                Debug.LogWarning(
                    "[GhostAI] Agent is not on NavMesh.",
                    this
                );

                warnedOffMesh = true;
            }

            return;
        }

        warnedOffMesh = false;

        UpdatePlayerVision();

        if (isWaiting)
        {
            UpdateWaiting();

            if (isWaiting)
                return;
        }

        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;

            case AIState.Investigate:
                UpdateInvestigate();
                break;

            case AIState.Chase:
                UpdateChase();
                break;

            case AIState.Search:
                UpdateSearch();
                break;
        }
    }

    // =====================================================
    // VISION DETECTION
    // =====================================================

    private void UpdatePlayerVision()
    {
        if (player == null)
            return;

        // Cloaked players cannot be seen.
        if (player.IsCloaked)
        {
            canSeePlayer = false;
            return;
        }

        if (Time.time < nextVisionCheckTime)
            return;

        nextVisionCheckTime =
            Time.time + visionCheckInterval;

        canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            lastKnownPlayerPosition =
                player.transform.position;

            lastPlayerEvidenceTime = Time.time;

            if (currentState != AIState.Chase)
            {
                EnterChase(
                    lastKnownPlayerPosition,
                    "Visual detection"
                );
            }
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 origin =
            transform.position + Vector3.up * eyeHeight;

        Vector3 target =
            player.transform.position +
            Vector3.up * playerAimHeight;

        Vector3 direction = target - origin;

        float distance = direction.magnitude;

        if (distance > viewDistance || distance <= 0.01f)
            return false;

        Vector3 flatForward =
            Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;

        Vector3 flatDirection =
            Vector3.ProjectOnPlane(
                direction,
                Vector3.up
            ).normalized;

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.Angle(
                flatForward,
                flatDirection
            );

            if (angle > fieldOfView * 0.5f)
                return false;
        }

        // Walls and other colliders block vision.
        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distance,
            visionMask,
            QueryTriggerInteraction.Ignore))
        {
            PlayerMove hitPlayer =
                hit.collider.GetComponentInParent<PlayerMove>();

            return hitPlayer == player;
        }

        return false;
    }

    // =====================================================
    // SOUND DETECTION
    // =====================================================

    private void HandleNoise(
        Vector3 soundPosition,
        float audibleRadius)
    {
        if (!isActiveAndEnabled ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        if (audibleRadius <= 0f)
            return;

        // Cloak blocks sound detection.
        if (player != null && player.IsCloaked)
            return;

        float distance = Vector3.Distance(
            transform.position,
            soundPosition
        );

        if (distance > audibleRadius)
            return;

        // While chasing, footsteps update the last known position.
        if (currentState == AIState.Chase)
        {
            lastKnownPlayerPosition = soundPosition;
            lastPlayerEvidenceTime = Time.time;
            return;
        }

        // Loud sounds from nearby running can trigger Chase.
        if (player != null &&
            audibleRadius >= chaseNoiseRadiusThreshold)
        {
            EnterChase(soundPosition, "Loud sound");
            return;
        }

        BeginInvestigate(soundPosition);
    }

    // =====================================================
    // PATROL
    // =====================================================

    private void UpdatePatrol()
    {
        if (!hasPatrolDestination)
        {
            ChooseNextDestination();
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            hasPatrolDestination = false;
            BeginWaiting(1f);
            return;
        }

        if (HasReachedDestination(patrolDestination))
        {
            hasPatrolDestination = false;

            BeginWaiting(
                Random.Range(minWaitTime, maxWaitTime)
            );
        }
    }

    private void ChooseNextDestination()
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint =
                Random.insideUnitCircle * patrolRadius;

            Vector3 candidate = transform.position +
                new Vector3(
                    randomPoint.x,
                    0f,
                    randomPoint.y
                );

            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                sampleDistance,
                agent.areaMask))
            {
                continue;
            }

            if (Vector3.Distance(
                transform.position,
                hit.position) < minTravelDistance)
            {
                continue;
            }

            if (IsRecentlyVisited(hit.position))
                continue;

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            if (!agent.SetDestination(hit.position))
                continue;

            RememberDestination(hit.position);

            patrolDestination = hit.position;
            hasPatrolDestination = true;

            Debug.Log(
                "[GhostAI] Patrol destination: " +
                hit.position,
                this
            );

            return;
        }

        BeginWaiting(2f);
    }

    private bool IsRecentlyVisited(Vector3 position)
    {
        foreach (Vector3 oldPosition in recentDestinations)
        {
            if (Vector3.Distance(
                position,
                oldPosition
            ) < avoidRepeatDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void RememberDestination(Vector3 position)
    {
        recentDestinations.Add(position);

        while (recentDestinations.Count >
               rememberedDestinations)
        {
            recentDestinations.RemoveAt(0);
        }
    }

    // =====================================================
    // INVESTIGATE
    // =====================================================

    private void BeginInvestigate(Vector3 soundPosition)
    {
        if (currentState == AIState.Chase)
            return;

        if (currentState == AIState.Investigate &&
            Vector3.Distance(
                soundPosition,
                lastHeardPosition
            ) < minNoiseUpdateDistance)
        {
            return;
        }

        if (!NavMesh.SamplePosition(
            soundPosition,
            out NavMeshHit hit,
            investigateSampleDistance,
            agent.areaMask))
        {
            return;
        }

        if (!agent.CalculatePath(hit.position, path))
            return;

        if (path.status != NavMeshPathStatus.PathComplete)
            return;

        currentState = AIState.Investigate;

        isWaiting = false;
        hasPatrolDestination = false;

        ApplyInvestigateMovement();

        agent.isStopped = false;

        if (!agent.SetDestination(hit.position))
        {
            currentState = AIState.Patrol;
            ApplyPatrolMovement();
            return;
        }

        investigateDestination = hit.position;
        lastHeardPosition = soundPosition;

        Debug.Log(
            "[GhostAI] Investigating sound: " +
            soundPosition,
            this
        );
    }

    private void UpdateInvestigate()
    {
        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            ReturnToPatrol(1f);
            return;
        }

        if (HasReachedDestination(investigateDestination))
        {
            Debug.Log(
                "[GhostAI] Reached sound location. Searching...",
                this
            );

            BeginWaiting(
                Random.Range(
                    investigateMinWaitTime,
                    investigateMaxWaitTime
                )
            );
        }
    }

    // =====================================================
    // CHASE
    // =====================================================

    private void EnterChase(
        Vector3 evidencePosition,
        string reason)
    {
        currentState = AIState.Chase;

        isWaiting = false;
        hasPatrolDestination = false;

        agent.isStopped = false;

        ApplyChaseMovement();

        lastKnownPlayerPosition = evidencePosition;
        lastPlayerEvidenceTime = Time.time;

        nextChaseDestinationTime = 0f;

        TrySetChaseDestination();

        Debug.Log(
            "[GhostAI] CHASE STARTED! Reason: " + reason,
            this
        );
    }

    private void UpdateChase()
    {
        // If the player is hidden or behind a wall,
        // continue towards the last known position.
        if (Time.time - lastPlayerEvidenceTime >
            loseSightDuration)
        {
            EnterSearch(lastKnownPlayerPosition);
            return;
        }

        // Update destination periodically instead of every frame.
        if (Time.time >= nextChaseDestinationTime)
        {
            TrySetChaseDestination();
        }
    }

    private void TrySetChaseDestination()
    {
        nextChaseDestinationTime =
            Time.time + chaseDestinationInterval;

        if (!NavMesh.SamplePosition(
            lastKnownPlayerPosition,
            out NavMeshHit hit,
            chaseSampleDistance,
            agent.areaMask))
        {
            return;
        }

        if (!agent.CalculatePath(hit.position, path))
            return;

        if (path.status != NavMeshPathStatus.PathComplete)
            return;

        agent.SetDestination(hit.position);
    }

    // =====================================================
    // SEARCH
    // =====================================================

    private void EnterSearch(Vector3 lastPosition)
    {
        currentState = AIState.Search;

        searchCenter = lastPosition;
        searchTimer = searchDuration;
        nextSearchDestinationTime = 0f;

        agent.ResetPath();
        agent.isStopped = false;

        ApplySearchMovement();

        Debug.Log(
            "[GhostAI] Lost player. Starting search.",
            this
        );

        ChooseSearchDestination();
    }

    private void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            Debug.Log(
                "[GhostAI] Search ended. Returning to Patrol.",
                this
            );

            ReturnToPatrol(1f);
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            ChooseSearchDestination();
            return;
        }

        if (Time.time >= nextSearchDestinationTime &&
            HasReachedSearchDestination())
        {
            ChooseSearchDestination();
        }
    }

    private void ChooseSearchDestination()
    {
        if (!agent.isOnNavMesh)
            return;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint =
                Random.insideUnitCircle * searchRadius;

            Vector3 candidate = searchCenter +
                new Vector3(
                    randomPoint.x,
                    0f,
                    randomPoint.y
                );

            if (!NavMesh.SamplePosition(
                candidate,
                out NavMeshHit hit,
                searchSampleDistance,
                agent.areaMask))
            {
                continue;
            }

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            agent.isStopped = false;

            if (!agent.SetDestination(hit.position))
                continue;

            nextSearchDestinationTime =
                Time.time + Random.Range(
                    searchMinPause,
                    searchMaxPause
                );

            return;
        }

        nextSearchDestinationTime = Time.time + 1f;
    }

    private bool HasReachedSearchDestination()
    {
        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return true;

        return agent.remainingDistance <=
               agent.stoppingDistance + 0.2f;
    }

    // =====================================================
    // MOVEMENT SETTINGS
    // =====================================================

    private void ApplyPatrolMovement()
    {
        agent.speed = patrolSpeed;
        agent.angularSpeed = patrolAngularSpeed;
        agent.acceleration = patrolAcceleration;
        agent.stoppingDistance = patrolStoppingDistance;
    }

    private void ApplyInvestigateMovement()
    {
        agent.speed = investigateSpeed;
        agent.angularSpeed = 300f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 0.4f;
    }

    private void ApplyChaseMovement()
    {
        agent.speed = chaseSpeed;
        agent.angularSpeed = chaseAngularSpeed;
        agent.acceleration = chaseAcceleration;
        agent.stoppingDistance = chaseStoppingDistance;
        agent.autoBraking = false;
    }

    private void ApplySearchMovement()
    {
        agent.speed = investigateSpeed;
        agent.angularSpeed = 300f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 0.4f;
        agent.autoBraking = true;
    }

    // =====================================================
    // SHARED WAITING AND NAVIGATION
    // =====================================================

    private bool HasReachedDestination(Vector3 destination)
    {
        if (agent.pathPending)
            return false;

        if (agent.hasPath)
        {
            return agent.remainingDistance <=
                   agent.stoppingDistance + 0.2f;
        }

        return Vector3.Distance(
            agent.nextPosition,
            destination
        ) <= agent.stoppingDistance + 0.5f;
    }

    private void BeginWaiting(float duration)
    {
        if (agent.hasPath)
            agent.ResetPath();

        agent.isStopped = true;

        isWaiting = true;
        waitTimer = Mathf.Max(0f, duration);
    }

    private void UpdateWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer > 0f)
            return;

        isWaiting = false;
        agent.isStopped = false;

        if (currentState == AIState.Investigate)
        {
            currentState = AIState.Patrol;
            ApplyPatrolMovement();

            Debug.Log(
                "[GhostAI] Investigation finished. Returning to Patrol.",
                this
            );
        }

        if (currentState == AIState.Patrol &&
            !hasPatrolDestination)
        {
            ChooseNextDestination();
        }
    }

    private void ReturnToPatrol(float waitTime)
    {
        currentState = AIState.Patrol;

        isWaiting = false;
        hasPatrolDestination = false;

        ApplyPatrolMovement();

        BeginWaiting(waitTime);
    }
}
