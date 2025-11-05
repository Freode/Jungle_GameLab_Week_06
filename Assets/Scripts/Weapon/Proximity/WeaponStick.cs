using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStick : MonoBehaviour
{

    public LayerMask interactMask;
    public float completeTimer = 1f;

    public Vector3 startLoc;
    public Vector3 startRot;

    public Vector3 endLoc;
    public Vector3 endRot;

    private Vector3 _basePos;
    private Vector3 _baseRot;
    private BoxCollider _boxCollider;
    private Coroutine _attackCoroutine;
    private List<ITakeDamage> _hitTargets = new List<ITakeDamage>();

    void Awake()
    {
        _basePos = transform.localPosition;
        _baseRot = transform.rotation.eulerAngles;
        _boxCollider = GetComponentInChildren<BoxCollider>();

        ColliderEventRelay relay = GetComponentInChildren<ColliderEventRelay>();
        if(relay != null)
            relay.onTriggerEnter += HandleTriggerEnter;

    }

    private void OnDestroy()
    {
        ColliderEventRelay relay = GetComponentInChildren<ColliderEventRelay>();
        if (relay != null)
            relay.onTriggerEnter -= HandleTriggerEnter;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && (gameObject.layer == LayerMask.NameToLayer("Player")))
            Attack();    
    }

    public void Attack()
    {
        if (_attackCoroutine != null)
            return;

        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        float timer = 0f;
        _boxCollider.enabled = true;
        transform.localPosition = startLoc;
        transform.localRotation = Quaternion.Euler(startRot);
        _hitTargets.Clear();

        while (timer <= completeTimer)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            Vector3 lerpRot = Vector3.Lerp(startRot, endRot, timer / completeTimer);
            transform.localRotation = Quaternion.Euler(lerpRot);
        }

        transform.localPosition = _basePos;
        transform.localRotation = Quaternion.Euler(_baseRot);
        _boxCollider.enabled = false;
        _attackCoroutine = null;
    }

    private void HandleTriggerEnter(Collider other)
    {
        // 자기 자신과 충돌 방지
        if (transform.root == other.transform.root)
            return;

        if (((1 << other.gameObject.layer) & interactMask.value) == 0)
            return;

        ITakeDamage takeDamage = other.gameObject.GetComponentInParent<ITakeDamage>();
        if (takeDamage == null) 
            return;

        // 이미 맞은 대상이면, 무시
        if (_hitTargets.Contains(takeDamage))
            return;

        _hitTargets.Add(takeDamage);

        int damage = 0;
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
            damage = GameManager.instance.GetPlayerDamage();
        else
            damage = 5;

        takeDamage.TakeDamage(gameObject, damage);
    }

    public bool IsAttacking()
    {
        return _attackCoroutine != null;
    }

}
