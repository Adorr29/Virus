using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public GameObject[] parts;

    public ParticleSystem deathParticle;
    public ParticleSystem healParticle;

    public new string name; // ???
    [TextArea]
    public string description;
    public Sprite icon;

    public int energyCost;
    public float buildTime;

    [HideInInspector]
    public Vector2Int position;

    public float moveSpeed = 1f;
    public bool canRotate = true;
    public bool moving { get; private set; } = false;
    public bool rotating => transform.rotation.eulerAngles.y != targetRotation;

    [HideInInspector]
    public bool isPreview = false;

    [HideInInspector]
    public float buildProgress = 1f;

    public float targetRotation { get; private set; }

    public int hp { get; private set; }
    public int hpMax { get; private set; }

    Vector2Int? targetPosition = null;
    int distanceToTargetPosition;
    float stuckDelay = 0;

    protected void InitLife(int life)
    {
        hp = life;
        hpMax = life;
    }

    public void TakeDamage(Entity attacker, int damage)
    {
        // if no damage, dont remove hp
        if (damage <= 0)
            return;

        OnDamageTaked(attacker, damage);

        if (damage >= hp)
        {
            hp = 0;
            Die();
        }
        else
            hp -= damage;
        // TODO play damage animation
    }

    // callback function for AI
    virtual protected void OnDamageTaked(Entity attacker, int damage) { }

    public void Heal(int amount)
    {
        // if no amount, dont heal
        if (amount <= 0)
            return;

        if (hp + amount >= hpMax)
            hp = hpMax;
        else
            hp += amount;

        GameManager.Get().PlayPaticle(healParticle, transform.position);
    }

    protected void SetHp(int hp)
    {
        this.hp = Mathf.Clamp(hp, 0, hpMax);
    }

    public void Die()
    {
        GameManager.Get().RemoveEntity(this);
        Player.Get().RemoveToSelection(this);

        GameManager.Get().PlayPaticle(deathParticle, transform.position);

        Destroy(gameObject);

        // check win condition
        Vector2Int worldSize = GameManager.Get().world.size;
        Tile[,] worldTab = GameManager.Get().world.tab;

        for (int i = 0; i < worldSize.x; i++)
            for (int j = 0; j < worldSize.y; j++)
                if (worldTab[i, j]?.entity is Virus)
                    return;

        SoundManager.Get().PlaySound("Victory");
    }

    public void Select()
    {
        transform.Find("Selected").gameObject.SetActive(true);
    }

    public void Deselect()
    {
        transform.Find("Selected").gameObject.SetActive(false);
    }

    public void StartMove(Vector2Int targetPosition, int distanceToTargetPosition = 0)
    {
        this.targetPosition = targetPosition;
        this.distanceToTargetPosition = distanceToTargetPosition;
    }

    public void StopMove()
    {
        targetPosition = position;
    }

    public void StartRotation(float targetRotation)
    {
        this.targetRotation = targetRotation;
    }

    public IEnumerator MoveAnimation()
    {
        moving = true;
        while (Vector3.Distance(transform.localPosition, Vector3.zero) > 0)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            yield return null;
        }
        moving = false;
    }

    // unit can be push, so unit is not blocking (for GameManager.FindPath)
    bool CanWalkThroughUnit(Tile tile)
    {
        // check if tile is not Ground
        if (!GameManager.Get().IsGround(tile))
            return false;

        // check if dont have Entity or if is a Unit and the Unit can be pushed
        if (tile.entity == null || tile.entity is Unit && (tile.entity as Unit).canBePush)
            return true;

        return false;
    }

    public void MoveOnce()
    {
        // can't move twice
        if (moving)
            return;

        List<Vector2Int> path = GameManager.Get().FindPath(position, targetPosition.Value, CanWalkThroughUnit, distanceToTargetPosition);

        if (path == null || path.Count == 0)
        {
            stuckDelay = 1f;
            return;
        }

        Vector2Int nextPosition = path.First();

        Unit unit = GameManager.Get().GetEntity(nextPosition)?.GetComponent<Unit>();

        // if a unit is blocking the path, push the unit
        if (unit)
            unit.Push(position);

        if (GameManager.Get().IsWalkable(nextPosition))
            GameManager.Get().MoveEntity(this, nextPosition);
    }

    void Move()
    {
        // if it stuck, wait before retry to find a path
        if (stuckDelay > 0f)
        {
            stuckDelay -= Time.deltaTime;
            return;
        }

        MoveOnce();

        if (GameManager.Get().ManhattanDistance(position, targetPosition.Value) <= distanceToTargetPosition)
            targetPosition = null;
    }

    void Rotate()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, targetRotation, 0), moveSpeed * 180f * Time.deltaTime);
    }

    // Start is called before the first frame update
    protected void Start()
    {
        
    }

    // Update is called once per frame
    protected void Update()
    {
        if (isPreview)
            return;

        BuildProgress();

        if (buildProgress < 1f)
            return;

        if (targetPosition.HasValue && position != targetPosition)
            Move();

        if (transform.rotation.eulerAngles.y != targetRotation)
            Rotate();
    }

    public void StartBuild()
    {
        buildProgress = 0f;
        hp = 1;

        foreach (GameObject part in parts)
            part.SetActive(false);
    }

    void BuildProgress()
    {
        for (int i = 0; i < parts.Count(); i++)
            if (buildProgress >= i / ((float)parts.Count() - 1))
                parts[i].SetActive(true);
    }
}
