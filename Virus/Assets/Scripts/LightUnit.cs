using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightUnit : OffensiveUnit
{
    public Bullet bullet;

    float firingDelayLeft = 0f;
    float firingDelayRight = 0f;

    float firingDelay = 0;

    Animator animator;

    private void Awake()
    {
        InitLife(45);
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

        firingDelayLeft -= Time.deltaTime;
        firingDelayRight -= Time.deltaTime;

        firingDelay -= Time.deltaTime;
    }

    protected override void Attack()
    {
        if (GameManager.Get().ManhattanDistance(position, targetEntity.position) <= attackRange)
        {
            if (firingDelayLeft <= 0f && firingDelay <= 0f)
            {
                firingDelayLeft = attackCooldown;
                firingDelay = attackCooldown / 2f;

                // run left animation
                animator.SetTrigger("FireLeft");

                LaunchBullet(targetEntity, true);
            }

            if (firingDelayRight <= 0 && firingDelay <= 0)
            {
                firingDelayRight = attackCooldown;
                firingDelay = attackCooldown / 2f;

                // run right animation
                animator.SetTrigger("FireRight");

                LaunchBullet(targetEntity, false);
            }
        }
        else
        {
            StartMove(targetEntity.position, attackRange);
        }
    }

    void LaunchBullet(Entity target, bool left)
    {
        GameObject FiringObject = transform.Find("FiringPosition" + (left ? "Left" : "Right")).gameObject;

        Vector3 startPosition = FiringObject.transform.position;
        Vector3 myPosition = transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = new Vector3(targetPosition.x - myPosition.x, 0, targetPosition.z - myPosition.z);

        Bullet projectile = Instantiate(bullet, startPosition, Quaternion.identity);

        Vector3 offsetY = Vector3.up * 1f;
        Vector3 offsetXZ = direction * 1f;

        projectile.Launch(target, offsetXZ + offsetY);

        FiringObject.GetComponent<AudioSource>().Play();
    }
}
