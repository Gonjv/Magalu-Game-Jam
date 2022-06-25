﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    protected Life _life;
    protected NavMeshAgent _nav;
    protected ZombieAnimations _animations;
    protected bool canAttack = true;

    [Header("Enemy")]
    public Transform model;
    public float destroyTime = 2f;

    [Header("Target Setup")]
    public Transform target;
    public float distanceToAttack = 2f;
    public float updateTargetPosition = 1f;
    public float detectDistance = 10f;

    [Header("Enemy Status")]
    public float damage = 10f;
    public float attackRate = 1f;

    public float TargetDistance => Vector3.Distance(transform.position, target.position);

    protected virtual void Start()
    {
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        _life = GetComponent<Life>();
        _animations = model.GetComponent<ZombieAnimations>();
        _nav = GetComponent<NavMeshAgent>();

        _nav.stoppingDistance = distanceToAttack;
        _animations.Walk(1f);

        _life.OnDie.AddListener(DeathFeedback);

        StartCoroutine(RoutineIA());
    }

    private void LateUpdate()
    {
        if (!_life.isDead && !canAttack)
        {
            transform.LookAt(target.position);
        }
    }

    protected virtual IEnumerator RoutineIA()
    {
        while (!_life.isDead)
        {
            if (TargetDistance > distanceToAttack && TargetDistance < detectDistance && canAttack)
            {
                _nav.SetDestination(target.position);
                _animations.Walk(1f);
            }

            else if (TargetDistance <= distanceToAttack && canAttack)
            {
                _nav.enabled = false;
                canAttack = false;
                _animations.Attack();

                StartCoroutine(Attack());
            }

            else if (TargetDistance >= detectDistance)
            {
                _animations.Idle();
                _nav.SetDestination(transform.position);
            }

            yield return new WaitForSeconds(updateTargetPosition);
        }
    }


    protected virtual IEnumerator Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distanceToAttack);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.transform != transform)
            {
                var life = hitCollider.gameObject.GetComponent<Life>();

                if (life != null)
                {
                    life.TakeDamage(damage);
                }
            }
        }

        yield return new WaitForSeconds(attackRate);

        canAttack = true;
        _nav.enabled = true;
    }

    protected virtual void DeathFeedback()
    {
        _nav.isStopped = true;
        _animations.Death();
        Destroy(this.gameObject, destroyTime);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanceToAttack);
    }
}