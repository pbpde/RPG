using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GolemController : EnemyController
{
    [Header("Skill")]

    public float kickForce = 25;
    public GameObject rockPrefab;
    public Transform handPos;
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();
            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void ThrowRock()
    {
        var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
        if (attackTarget != null)
        {
            rock.GetComponent<Rock>().target = attackTarget;
        }
        else
        {
            rock.GetComponent<Rock>().target = FindObjectOfType<PlayController>().gameObject;
        }
    }
}
