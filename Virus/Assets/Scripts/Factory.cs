using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Factory : Entity
{
    public List<Entity> buildList = new List<Entity>();

    public Vector2Int? moveAfterBuild;

    bool buildUnit = false;
    float buildDelay;

    float inactiveTime = 0f;

    Animator animator;

    void Awake()
    {
        InitLife(50);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();

        animator.speed = 0f;
    }

    // Update is called once per frame
    new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        if (buildProgress < 1f)
            return;

        animator.speed = buildUnit ? 1f : 0f;

        if (buildUnit)
        {
            inactiveTime = 0f;

            buildDelay += Time.deltaTime;

            if (buildDelay >= buildList.First().buildTime)
            {
                buildUnit = false;
                Build(buildList.First());
                buildList.RemoveAt(0);
                NextBuild();
            }
        }
        else
        {
            inactiveTime += Time.deltaTime;

            if (inactiveTime > 1f)
            {
                inactiveTime = 0f;
                NextBuild();
            }
        }
    }

    public void AddToBuildList(Entity entity)
    {
        buildList.Add(entity);

        if (!buildUnit)
            NextBuild();
    }

    void NextBuild()
    {
        if (buildUnit || buildList.Count == 0)
            return;

        Entity entity = buildList.First();

        if (Player.Get().energy < entity.energyCost)
            return;

        Player.Get().energy -= entity.energyCost;

        buildUnit = true;
        buildDelay = 0f;
    }

    void Build(Entity entityToBuild)
    {
        Vector2Int buildPosition = GetBuildPosition();

        Unit inFrontUnit = GameManager.Get().GetEntity(buildPosition) as Unit;

        // if a unit is already on buildPosition push the unit
        if (inFrontUnit != null)
            inFrontUnit.Push(position);

        if (GameManager.Get().IsWalkable(buildPosition))
        {
            Entity entity = Instantiate(entityToBuild);

            entity.transform.rotation = transform.rotation;
            entity.StartRotation(transform.rotation.eulerAngles.y);

            GameManager.Get().AddEntity(entity, buildPosition, position);

            if (moveAfterBuild.HasValue)
                entity.StartMove(moveAfterBuild.Value);
        }
    }

    Vector2Int GetBuildPosition()
    {
        if (transform.rotation.eulerAngles.y >= -45 && transform.rotation.eulerAngles.y < 45)
            return position + new Vector2Int(1, 0);
        if (transform.rotation.eulerAngles.y >= -45 + 90 && transform.rotation.eulerAngles.y < 45 + 90)
            return position + new Vector2Int(0, -1);
        if (transform.rotation.eulerAngles.y >= -45 + 90 * 2 && transform.rotation.eulerAngles.y < 45 + 90 * 2)
            return position + new Vector2Int(-1, 0);
        if (transform.rotation.eulerAngles.y >= -45 + 90 * 3 && transform.rotation.eulerAngles.y < 45 + 90 * 3)
            return position + new Vector2Int(0, 1);

        throw new System.Exception("What ??");
    }
}
