using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class OffensiveUnit : Unit
{
    public int attackRange = 1;
    public float attackCooldown = 1;

    protected Entity targetEntity = null;

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        if (!moving && !rotating)
        {
            if (targetEntity != null)
            {
                if (targetRotation != GameManager.Get().GetBestAngle(this, targetEntity))
                    GameManager.Get().RotateEntity(this, GameManager.Get().GetBestAngle(this, targetEntity));
                else
                    Attack();
            }
            else
            {
                int autoAttackRange = Mathf.FloorToInt(attackRange * 1.5f);
                IEnumerable<Entity> entitys = GameManager.Get().GetNearEntityAtRange(position, autoAttackRange).Where(e => e is Virus);

                if (entitys.Count() > 0)
                {
                    entitys = entitys.OrderBy(e => GameManager.Get().ManhattanDistance(position, e.position));

                    StartAttack(entitys.First());
                }
            }
        }
    }

    protected abstract void Attack();

    public void StartAttack(Entity entity)
    {
        targetEntity = entity;
    }

    public void StopAttack()
    {
        targetEntity = null;
    }

    protected override void OnDamageTaked(Entity attacker, int damage)
    {
        if (!moving && !rotating && targetEntity == null)
            targetEntity = attacker;
    }
}
