using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStats {HitPlayer,HitEnemy,HitNothing}
    private Rigidbody rb;
    public RockStats rockState;
    [Header("Base Setting")]

    public float force;
    public int damage;
    public GameObject target;
    private Vector3 direction;
    public GameObject breakEffect;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockState = RockStats.HitPlayer;
        FlyToTarget();
    }
    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 1)
        {
            rockState = RockStats.HitNothing;
        }
    }
    public void FlyToTarget()
    {
        direction = (target.transform.position - transform.position + 2*Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockState)
        {
            case RockStats.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());
                    rockState = RockStats.HitNothing;
                } 
                break;
            case RockStats.HitEnemy:
                if (other.gameObject.GetComponent<GolemController>())
                {
                    var otherState = other.gameObject.GetComponent<CharacterStats>();
                    otherState.TakeDamage(damage, otherState);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
