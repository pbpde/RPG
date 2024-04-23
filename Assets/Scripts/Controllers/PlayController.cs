using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;

    private GameObject attackTarget;
    private float lastAttackTime;
    private bool isDead;

    private float stopDistance;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }
    private void OnEnable()
    {
        GameManager.Instance.RigsterPlayer(characterStats);
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        
    }
    private void OnDisable()
    {
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void Update()
    {
        isDead = characterStats.currentHealth == 0;

        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", isDead);
    }
    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead)
        {
            return;
        }
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject target)
    {
        if (isDead)
        {
            return;
        }
        if (target != null)
        {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        while (Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange+0.3f)
        {
            agent.destination = attackTarget.transform.position;
            yield return null; 
        }
        transform.LookAt(attackTarget.transform);
        agent.isStopped = true;
        if (lastAttackTime < 0)
        {
            anim.SetTrigger("Attack");
            anim.SetBool("Critical", characterStats.isCritical);
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }

    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>()&&attackTarget.GetComponent<Rock>().rockState== Rock.RockStats.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockState = Rock.RockStats.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

}
