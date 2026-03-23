using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ChaserEnemy : MonoBehaviour
{
    public Transform target;
    public float chaseSpeed = 2.5f;
    public float updatePathInterval = 0.25f;

    private NavMeshAgent agent;
    private float pathTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.angularSpeed = 240f;
        agent.acceleration = 8f;
        agent.autoBraking = false;
    }

    void Update()
    {
        if (target == null) return;

        pathTimer += Time.deltaTime;
        if (pathTimer >= updatePathInterval)
        {
            pathTimer = 0f;
            agent.SetDestination(target.position);
        }
    }
}