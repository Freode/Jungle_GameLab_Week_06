using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, IUnitWithFog, ITakeDamage
{
    [Header("Component References")]
    [HideInInspector] public NavMeshAgent agent;
    public Transform player;

    [Header("Patrol Settings")]
    public float patrolHp = 20f;
    public float patrolRadius = 20f;
    public float patrolSpeed = 3.5f;
    public float patrolWaitTime = 2f;
    [HideInInspector] public Vector3 startPoint;

    [Header("Detection & Combat Settings")]
    public float sightRange = 15f;
    [UnityEngine.Range(0, 360)] public float fieldOfViewAngle = 90f;
    public float chaseSpeed = 6f;
    public float attackDamage = 10f; // 적의 공격력

    [Header("State Management")]
    public LayerMask whatIsPlayer;

    // 제네릭으로 선언된 상태 머신
    public StateMachine<EnemyAI> stateMachine;

    // 모든 자식의 메시 랜더러 가지고 있기
    private MeshRenderer[] _renderers;

    private bool _isUpdate = true;
    public bool _isHitting {private set; get;}
    private Coroutine _hitCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 상태 머신을 생성할 때 자기 자신(this)을 owner로 넘겨줍니다.
        stateMachine = new StateMachine<EnemyAI>(this);
    }

    void Start()
    {
        startPoint = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogError("Player not found! Make sure your player has the 'Player' tag.");

        WeaponStick weaponStick = GetComponentInChildren<WeaponStick>();
        if(weaponStick != null)
            weaponStick.SetDamage((int)attackDamage);

        // 초기 상태를 PatrolState로 설정
        stateMachine.Initialize(new PatrolState(this, stateMachine));

        // 모든 자식의 Mesh Renderer를 가져오기
        _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        // 시작할 때, 모든 메시 랜더러 끄기
        OnMeshInactive();
    }

    void Update()
    {
        if(_isUpdate)
            // 상태 머신의 Execute를 호출합니다.
            stateMachine.Execute();
    }

    // 플레이어가 현재 시야 안에 있는지 확인 - 수정 필요 - 
    public bool IsPlayerInSight()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > sightRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, directionToPlayer) > fieldOfViewAngle / 2) return false;

        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, ~whatIsPlayer)) return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.cyan;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfViewAngle / 2, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfViewAngle / 2, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * sightRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * sightRange);
    }

    // 메시 모두 활성화
    public void OnMeshActive()
    {
        if (_isUpdate == false)
            return;

        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }

    // 메시 모두 비활성화
    public void OnMeshInactive()
    {
        if (_isUpdate == false)
            return;

        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.enabled = false;
        }
    }

    public void TakeDamage(GameObject opponent, float damage)
    {
        patrolHp -= damage;
        _isHitting = true;

        stateMachine.ChangeState(new ChaseState(this, stateMachine));

        if (_hitCoroutine != null)
            StopCoroutine(_hitCoroutine);
        _hitCoroutine = StartCoroutine(HitCoroutine());
    }

    public bool IsDie()
    {
        if (patrolHp > 0f)
            return false;

        return true;
    }

    public void SetUpdate(bool isUpdate)
    {
        _isUpdate = isUpdate;
    }

    IEnumerator HitCoroutine()
    {
        _isHitting = true;

        yield return new WaitForSeconds(5f);

        _isHitting = false;
    }
}

