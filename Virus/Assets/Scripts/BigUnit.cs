using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigUnit : OffensiveUnit
{
    public Bullet bullet;

    Animator animator;

    float firingDelay = 0f;

    void Awake()
    {
        InitLife(75);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        firingDelay -= Time.deltaTime;
    }

    protected override void Attack()
    {
        if (GameManager.Get().ManhattanDistance(position, targetEntity.position) <= attackRange)
        {
            if (firingDelay <= 0f)
            {
                firingDelay = attackCooldown;

                // run animation
                animator.SetTrigger("Fire");

                LaunchBullet(targetEntity);
            }
        }
        else
        {
            StartMove(targetEntity.position, attackRange);
        }
    }

    void LaunchBullet(Entity target)
    {
        GameObject FiringObject = transform.Find("FiringPosition").gameObject;

        Vector3 startPosition = FiringObject.transform.position;

        Bullet projectile = Instantiate(bullet, startPosition, Quaternion.identity);

        projectile.Launch(target, new Vector3(0f, 1f, 0f));

        FiringObject.GetComponent<AudioSource>().Play();
    }
}
