using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChaserEnemy : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;

    [Header("Movement")]
    public float chaseSpeed = 2.5f;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 1.0f;
    public float stuckDistanceThreshold = 0.05f;
    public float recoveryDuration = 1.0f;
    public float recoverySpeed = 3.0f;

    private Rigidbody rb;
    private float stuckTimer;
    private float recoveryTimer;
    private Vector3 lastCheckedPosition;
    private bool isRecovering;
    private Vector3 recoveryDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        lastCheckedPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (isRecovering)
        {
            RecoveryMove();
            return;
        }

        ChaseTarget();
        StuckCheck();
    }

    void ChaseTarget()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        rb.MovePosition(rb.position + direction * chaseSpeed * Time.fixedDeltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    void StuckCheck()
    {
        stuckTimer += Time.fixedDeltaTime;
        if (stuckTimer < stuckCheckInterval) return;
        stuckTimer = 0f;

        float moved = Vector3.Distance(transform.position, lastCheckedPosition);
        lastCheckedPosition = transform.position;

        if (moved < stuckDistanceThreshold)
        {
            StartRecovery();
        }
    }

    void StartRecovery()
    {
        isRecovering = true;
        recoveryTimer = 0f;

        Vector3 toTarget = (target.position - transform.position).normalized;
        Vector3 perp = Vector3.Cross(Vector3.up, toTarget);
        recoveryDirection = (Random.value > 0.5f ? perp : -perp);
        recoveryDirection.y = 0f;
    }

    void RecoveryMove()
    {
        recoveryTimer += Time.fixedDeltaTime;
        rb.MovePosition(rb.position + recoveryDirection * recoverySpeed * Time.fixedDeltaTime);

        if (recoveryTimer >= recoveryDuration)
        {
            isRecovering = false;
            lastCheckedPosition = transform.position;
        }
    }
}
