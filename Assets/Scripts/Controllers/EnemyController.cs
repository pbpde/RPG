using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    protected CharacterStats characterStats;
    private Collider coll;

    [Header("Basic Settings")]

    public float sightRadius;
    public bool isGuard;
    private float speed;
    protected GameObject attackTarget;
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;


    [Header("patrol State")]

    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;
    private Quaternion guardRotation;

    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        GameManager.Instance.AddObserver(this);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }
    private void Update()
    {
        if (characterStats.currentHealth == 0)
        {
            isDead = true;
        }
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }

    void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }
    void SwitchStates()
    {
        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }
        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.SqrMagnitude(guardPos - transform.position) < agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation,guardRotation,0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }  
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                //×·»÷Player
                 
                //ÅäºÏ¶¯»­
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //À­ÍÑ»Øµ½ÐÐÒ»¸ö×´Ì¬
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= lookAtTime;
                    }
                    else if(isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }

                //ÔÚ¹¥»÷·¶Î§ÄÚ¹¥»÷
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //±©»÷ÅÐ¶Ï
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        Attack();
                    }
                    else
                    {
                        Vector3 rotateToTarget = attackTarget.transform.position - transform.position;
                        rotateToTarget.Normalize();
                        Quaternion targetRotation = Quaternion.LookRotation(rotateToTarget);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.003f);
                    }
                }

                break;
            case EnemyStates.DEAD:
                agent.radius = 0;
                coll.enabled = false;
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack()
    {
        if (TargetInSkillRange())
        {
            anim.SetTrigger("Skill");
        }
        else if (TargetInAttackRange())
        {
            anim.SetTrigger("Attack");
        }
        
    }
    bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        }
        else
        {
            return false;
        }
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            if (characterStats.attackData.skillRange > characterStats.attackData.attackRange)
            {
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange &&
                       Vector3.Distance(attackTarget.transform.position, transform.position) >= characterStats.attackData.attackRange;
            }
            else
            {
                return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
            }
        }
        else
        {
            return false;
        }
    }


    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach(var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndVotify()
    {
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
