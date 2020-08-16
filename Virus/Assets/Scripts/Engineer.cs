using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum BuildStatus
{ 
    None,
    Foundation,
    Build,
    Repair
};

public class Engineer : Unit
{
    public ParticleSystem repairParticle;

    public int repairAmout = 20;
    public float repairCooldown = 1f;

    Entity targetEntity = null;
    Vector2Int buildPosition;
    float buildRotation;
    BuildStatus buildStatus = BuildStatus.None;
    float hpStack = 0f;
    float repairDelay = 0f;

    Animator animator;

    void Awake()
    {
        InitLife(20);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();

        animator.speed = 0f;
    }

    // Update is called once per frame
    protected new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        canBePush = true;
        animator.speed = 0f;

        if (buildStatus != BuildStatus.None && !moving && !rotating)
        {
            if (buildStatus == BuildStatus.Foundation)
                Foundation();
            else if (buildStatus == BuildStatus.Build)
                Build();
            else if (buildStatus == BuildStatus.Repair)
                Repair();
        }
    }

    void Foundation()
    {
        if (GameManager.Get().ManhattanDistance(position, buildPosition) > 1)
        {
            StartMove(buildPosition, 1);
            return;
        }

        if (targetRotation != GameManager.Get().GetBestAngle(position, buildPosition))
        {
            GameManager.Get().RotateEntity(this, GameManager.Get().GetBestAngle(position, buildPosition));
            return;
        }

        if (GameManager.Get().IsWalkable(buildPosition))
        {
            if (Player.Get().energy >= targetEntity.energyCost)
            {
                Player.Get().energy -= targetEntity.energyCost;

                Entity entity = Instantiate(targetEntity);

                entity.StartBuild();
                entity.transform.rotation = Quaternion.Euler(0, buildRotation, 0);

                GameManager.Get().AddEntity(entity, buildPosition);

                // switch to Build
                StartBuild(entity);
            }
        }
        else
        {
            Entity entity = GameManager.Get().GetEntity(buildPosition);

            if (entity.buildProgress < 1f)
                StartBuild(entity);
        }
    }

    void Build()
    {
        if (GameManager.Get().ManhattanDistance(position, targetEntity.position) > 1)
        {
            StartMove(targetEntity.position, 1);
            return;
        }

        if (targetRotation != GameManager.Get().GetBestAngle(this, targetEntity))
        {
            GameManager.Get().RotateEntity(this, GameManager.Get().GetBestAngle(this, targetEntity));
            return;
        }

        canBePush = false;
        animator.speed = 1f;

        if (targetEntity.buildProgress < 1f)
        {
            float progress = Time.deltaTime / targetEntity.buildTime;

            targetEntity.buildProgress += progress;

            // limite buildProgress to 1.0
            targetEntity.buildProgress = Mathf.Min(targetEntity.buildProgress, 1f);

            hpStack += progress * targetEntity.hpMax;

            if (hpStack >= 1f)
            {
                int amount = Mathf.CeilToInt(hpStack);

                targetEntity.Heal(amount);
                hpStack -= amount;
            }
        }
        else
            buildStatus = BuildStatus.Repair;
    }

    void Repair()
    {
        repairDelay -= Time.deltaTime;

        if (repairDelay > 0f)
        {
            canBePush = false;
            animator.speed = 1f;
            return;
        }

        if (GameManager.Get().ManhattanDistance(position, targetEntity.position) > 1)
        {
            StartMove(targetEntity.position, 1);
            return;
        }

        if (targetRotation != GameManager.Get().GetBestAngle(this, targetEntity))
        {
            GameManager.Get().RotateEntity(this, GameManager.Get().GetBestAngle(this, targetEntity));
            return;
        }

        if (targetEntity.hp < targetEntity.hpMax)
        {
            int toRepair = targetEntity.hpMax - targetEntity.hp;

            // limite to repairAmout
            toRepair = Mathf.Min(toRepair, repairAmout);

            // limite to Player Energy
            toRepair = Mathf.Min(toRepair, Player.Get().energy);

            if (toRepair > 0)
            {
                repairDelay = repairCooldown;
                canBePush = false;
                animator.speed = 1f;

                // remove temporarily the HealParticle of targetEntity for only play the repairParticle of Engineer
                ParticleSystem targetEntityHealParticle = targetEntity.healParticle;
                targetEntity.healParticle = null;

                GameManager.Get().PlayPaticle(repairParticle, targetEntity.transform.position);

                Player.Get().energy -= toRepair;

                targetEntity.Heal(toRepair);

                targetEntity.healParticle = targetEntityHealParticle;
            }
        }
    }

    public void StartFoundation(Entity entity, Vector2Int position, float rotation = 0)
    {
        if (GameManager.Get().IsWalkable(position))
        {
            targetEntity = entity;
            buildPosition = position;
            buildRotation = rotation;

            if (GameManager.Get().ManhattanDistance(this.position, buildPosition) > 1)
                StartMove(buildPosition, 1);

            buildStatus = BuildStatus.Foundation;
        }
    }

    public void StartBuild(Entity entity)
    {
        if (entity.buildProgress < 1f)
        {
            targetEntity = entity;
            hpStack = 0f;

            if (GameManager.Get().ManhattanDistance(position, targetEntity.position) > 1)
                StartMove(targetEntity.position, 1);

            buildStatus = BuildStatus.Build;
        }
    }

    public void StartRepair(Entity entity)
    {
        if (entity.hp < entity.hpMax)
        {
            targetEntity = entity;

            if (GameManager.Get().ManhattanDistance(position, targetEntity.position) > 1)
                StartMove(targetEntity.position, 1);

            buildStatus = BuildStatus.Repair;
        }
    }

    public void Cancel()
    {
        buildStatus = BuildStatus.None;
    }
}
