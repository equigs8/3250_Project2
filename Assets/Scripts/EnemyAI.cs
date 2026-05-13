using UnityEngine;
using UnityEngine.AI;
using InfimaGames.LowPolyShooterPack;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum AIState { Idle, Patrol, Chase, Attack, Investigate, Dead }

    [Header("Core State")]
    public AIState currentState = AIState.Idle;
    public Transform player;
    public Animator animator;
    private NavMeshAgent agent;
    
    [Header("Detection Settings")]
    public float visionRange = 25f;
    public float visionAngle = 60f;
    public float hearingRange = 15f; 
    public LayerMask obstacleMask;
    private bool playerInSight;
    private Vector3 lastKnownPosition;

    [Header("Combat Settings")]
    public WeaponBehaviour activeWeapon; 
    public Transform weaponBarrel; // ASSIGN THIS IN THE INSPECTOR
    public float fireRate = 0.5f; 
    public float attackRange = 10f; 
    [Range(0, 2)] public float accuracyOffset = 0.1f;
    private float nextFireTime;

    [Header("Movement & Navigation")]
    public float speed = 4f;
    public float sprintSpeed = 6f; 
    public float rotationSpeed = 10f; 
    public Vector3 rotationOffset;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public float patrolWaitTime = 2f;
    private float waitTimer;

    [Header("Tactical Settings")]
    public float investigateTime = 5f;
    private float investigateTimer;
    private float strafeTimer;
    
    private float currentIKWeight = 0f;

    void Start()
    {
        if (activeWeapon == null) activeWeapon = GetComponentInChildren<WeaponBehaviour>();
        if (animator == null) animator = GetComponent<Animator>();
        
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; 

        if (patrolPoints.Length > 0) SwitchState(AIState.Patrol);
    }

    // --- NEW: State Debugger ---
    // Use this function to change states instead of setting the variable directly.
    void SwitchState(AIState newState)
    {
        if (currentState == newState) return;
        
        // This will print exactly when and why the enemy changes its mind
        Debug.Log($"<color=orange>{gameObject.name}</color> switched from {currentState} to <color=green>{newState}</color>");
        
        currentState = newState;
    }

    void Update()
    {
        if (currentState == AIState.Dead || player == null) return;

        CheckSenses();

        switch (currentState) 
        { 
            case AIState.Idle:
                agent.isStopped = true;
                UpdateAnimations(Vector3.zero, false);
                animator.SetBool("Aiming", false);
                if (playerInSight) SwitchState(AIState.Chase);
                break;

            case AIState.Patrol:
                agent.isStopped = false;
                agent.speed = speed;
                PatrolRoutine();
                break;

            case AIState.Chase:
                agent.isStopped = false;
                agent.speed = sprintSpeed;
                ChaseRoutine();
                break;

            case AIState.Attack:
                agent.isStopped = false;
                agent.speed = speed;
                animator.SetBool("Aiming", true);
                AttackRoutine();
                break;

            case AIState.Investigate:
                agent.isStopped = false;
                agent.speed = speed;
                InvestigateRoutine();
                break;
        }
    }

    void CheckSenses()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        Vector3 enemyEyePos = transform.position + Vector3.up * 1.5f;
        Vector3 playerChestPos = player.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (playerChestPos - enemyEyePos).normalized;

        if (distanceToPlayer <= visionRange && Vector3.Angle(transform.forward, directionToPlayer) <= visionAngle)
        {
            if (!Physics.Raycast(enemyEyePos, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                playerInSight = true;
                lastKnownPosition = player.position;
                return; 
            }
        }
        
        if (distanceToPlayer <= hearingRange)
        {
             lastKnownPosition = player.position;
             if (currentState == AIState.Idle || currentState == AIState.Patrol)
             {
                 investigateTimer = 0f;
                 SwitchState(AIState.Investigate);
             }
             playerInSight = false; 
             return;
        }

        playerInSight = false;
    }

    void PatrolRoutine()
    {
        if (playerInSight) { SwitchState(AIState.Chase); return; }
        if (patrolPoints.Length == 0) { SwitchState(AIState.Idle); return; }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        agent.SetDestination(targetPoint.position);
        
        if (agent.velocity.sqrMagnitude > 0.1f) LookAtDirection(agent.velocity);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            UpdateAnimations(Vector3.zero, false);
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                waitTimer = 0f;
            }
        }
        else
        {
            UpdateAnimations(agent.velocity, false);
        }
    }

    void ChaseRoutine()
    {
        if (!playerInSight) { investigateTimer = 0f; SwitchState(AIState.Investigate); return; }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange)
        {
            agent.SetDestination(player.position);
            LookAtDirection(player.position - transform.position);
            UpdateAnimations(agent.velocity, true);
        }
        else
        {
            if (agent.hasPath) agent.ResetPath();
            strafeTimer = 1.5f; 
            SwitchState(AIState.Attack);
        }
    }

    void AttackRoutine()
    {
        if (!playerInSight) { investigateTimer = 0f; SwitchState(AIState.Investigate); return; }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange * 1.2f) { SwitchState(AIState.Chase); return; }

        LookAtDirection(player.position - transform.position);

        strafeTimer -= Time.deltaTime;
        
        if (strafeTimer <= 0 && (!agent.hasPath || agent.remainingDistance < 0.5f))
        {
            float randomX = Random.Range(-1f, 1f) > 0 ? 1f : -1f;
            Vector3 strafeTarget = transform.position + (transform.right * randomX * 3f);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(strafeTarget, out hit, 3f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            strafeTimer = Random.Range(1f, 3f);
        }

        UpdateAnimations(agent.velocity, false);

        if (Time.time >= nextFireTime)
        {
            if (activeWeapon != null)
            {
                activeWeapon.Fire(accuracyOffset);
                if (!activeWeapon.HasAmmunition()) activeWeapon.Reload();
            }
            nextFireTime = Time.time + fireRate + Random.Range(-0.1f, 0.2f); 
        }
    }

    void InvestigateRoutine()
    {
        if (playerInSight) { SwitchState(AIState.Chase); return; }

        agent.SetDestination(lastKnownPosition);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            UpdateAnimations(Vector3.zero, false);
            transform.Rotate(Vector3.up * (rotationSpeed * 5f) * Time.deltaTime);

            investigateTimer += Time.deltaTime;
            if (investigateTimer >= investigateTime)
            {
                SwitchState(patrolPoints.Length > 0 ? AIState.Patrol : AIState.Idle);
            }
        }
        else
        {
            if (agent.velocity.sqrMagnitude > 0.1f) LookAtDirection(agent.velocity);
            UpdateAnimations(agent.velocity, false);
        }
    }

    void LookAtDirection(Vector3 direction)
    {
        direction.y = 0; 
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void UpdateAnimations(Vector3 velocity, bool isSprinting)
    {
        bool isMoving = velocity.sqrMagnitude > 0.1f;
        animator.SetBool("Moving", isMoving);

        if (!isMoving)
        {
            animator.SetFloat("xMovement", 0f, 0.1f, Time.deltaTime);
            animator.SetFloat("zMovement", 0f, 0.1f, Time.deltaTime);
            return;
        }

        Vector3 localDir = transform.InverseTransformDirection(velocity.normalized);
        float zSpeed = isSprinting ? localDir.z * 2f : localDir.z;

        animator.SetFloat("xMovement", localDir.x, 0.1f, Time.deltaTime);
        animator.SetFloat("zMovement", zSpeed, 0.1f, Time.deltaTime);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        bool shouldAim = (currentState == AIState.Attack || currentState == AIState.Chase) && playerInSight;
        float targetWeight = shouldAim ? 1f : 0f;
        currentIKWeight = Mathf.Lerp(currentIKWeight, targetWeight, Time.deltaTime * 5f);

        animator.SetLookAtWeight(currentIKWeight, currentIKWeight * 0.4f, currentIKWeight * 0.9f, currentIKWeight, 0.5f);
        
        if (player != null)
        {
            animator.SetLookAtPosition(player.position + Vector3.up * 1.5f);
        }
    }

    // --- NEW: Exact Barrel Aiming ---
    void LateUpdate()
    {
        // LateUpdate runs AFTER the Animator has finished posing the rig for this frame.
        // This is where we override the animation to force the gun barrel to aim perfectly.
        
        if (animator == null || player == null || weaponBarrel == null) return;

        if ((currentState == AIState.Attack || currentState == AIState.Chase) && playerInSight)
        {
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest != null)
            {
                Vector3 targetPos = player.position + Vector3.up * 1.5f; // Aim at chest height
                Vector3 directionToTarget = targetPos - weaponBarrel.position;

                // Calculate the angular difference between where the barrel IS pointing, and where it SHOULD point
                Quaternion correctiveRotation = Quaternion.FromToRotation(weaponBarrel.forward, directionToTarget);
                
                // Apply that twist smoothly to the chest bone
                chest.rotation = Quaternion.Slerp(chest.rotation, correctiveRotation * chest.rotation, currentIKWeight);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the vision cone
        Gizmos.color = playerInSight ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        // Draw the hearing range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        // Draw the exact line the gun barrel is pointing
        if (weaponBarrel != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(weaponBarrel.position, weaponBarrel.forward * 20f);
        }
    }
}