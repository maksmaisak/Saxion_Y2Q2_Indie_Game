using UnityEngine;
using UnityEngine.AI;

// A facade for an Animator-Rigidbody-based enemy character. Automatically applies motion from a NavMeshAgent.
[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class EnemyCharacter : MonoBehaviour
{
    [SerializeField] float movingTurnSpeed = 360f;
    [SerializeField] float stationaryTurnSpeed = 180f;
    [SerializeField] float runSpeed = 4.25f;
    [SerializeField] float moveSpeedMultiplier = 1f;
    [SerializeField] float animSpeedMultiplier = 1f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] float gravityMultiplier = 1f;
    
    private Animator animator;
    private new Rigidbody rigidbody;
    private NavMeshAgent navMeshAgent;

    private bool isGrounded;
    private Vector3 groundNormal;
    private float currentGroundCheckDistance;
    
    private float forwardAmount;
    private float turnAmount;
        
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator  = GetComponent<Animator>();
        
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        if (navMeshAgent)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;
        }

        currentGroundCheckDistance = groundCheckDistance;
    }

    void Update()
    {
        if (!navMeshAgent) return;
        if (!navMeshAgent.isActiveAndEnabled) return;
        if (!navMeshAgent.isOnNavMesh) return;
                
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            Move(navMeshAgent.desiredVelocity);
        else
            Move(Vector3.zero);
        
        UpdateNavMeshAgent();
    }
    
    void OnAnimatorMove()
    {
        if (!isGrounded || Time.deltaTime <= 0f) return;

        // Override default root motion.
        // this allows us to modify the positional speed before it's applied.
        Vector3 delta = animator.deltaPosition * moveSpeedMultiplier;
        if (rigidbody.isKinematic)
        {
            rigidbody.MovePosition(rigidbody.position + delta);
        }
        else
        {
            Vector3 v = delta / Time.deltaTime;
            v.y = rigidbody.velocity.y;
            rigidbody.velocity = v;
        }
    }
    
    public void Move(Vector3 move)
    {
        // TODO change forward amount based on magnitude.

        float desiredSpeed = move.magnitude;
        
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.sqrMagnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, groundNormal);
        
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = desiredSpeed / runSpeed;

        ApplyExtraTurnRotation();

        if (!isGrounded) ApplyExtraGravity();

        // send input and other state parameters to the animator
        UpdateAnimator(desiredSpeed);
    }

    /// Pull the agent towards the animated character
    private void UpdateNavMeshAgent()
    {
        Vector3 position = rigidbody.position;
        position.y = navMeshAgent.nextPosition.y;
        rigidbody.position = position;
        
        Vector3 delta = navMeshAgent.nextPosition - rigidbody.position;
        if (delta.sqrMagnitude > navMeshAgent.radius * navMeshAgent.radius)
            navMeshAgent.nextPosition = rigidbody.position + 0.9f * delta;
    }

    private void CheckGroundStatus()
    {        
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hitInfo, currentGroundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            animator.applyRootMotion = isGrounded = true;
            currentGroundCheckDistance = groundCheckDistance;
        }
        else
        {
            groundNormal = Vector3.up;
            animator.applyRootMotion = isGrounded = false;
            animator.applyRootMotion = true;
            currentGroundCheckDistance = rigidbody.velocity.y > 0f ? 0.01f : groundCheckDistance;
        }
    }

    private void ApplyExtraTurnRotation()
    {
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0f, turnAmount * turnSpeed * Time.deltaTime, 0f);
    }
    
    private void ApplyExtraGravity()
    {
        rigidbody.AddForce(Physics.gravity * (gravityMultiplier - 1f));
    }
    
    private void UpdateAnimator(float desiredSpeed)
    {
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn"   , turnAmount   , 0.1f, Time.deltaTime);

        if (isGrounded && desiredSpeed > 0f) 
            animator.speed = animSpeedMultiplier;
        else
            animator.speed = 1f;
    }
}