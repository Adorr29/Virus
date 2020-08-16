using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : Entity
{
    [HideInInspector]
    public bool canBePush = true;

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
    }

    public void Push(Vector2Int pushOrigin)
    {
        Push(new List<Vector2Int> { pushOrigin });
    }

    bool Push(List<Vector2Int> pushOrigins)
    {
        if (moving || !canBePush)
            return false;

        IEnumerable<Tile> nearTiles = GameManager.Get().GetNearTile(position).Where(t => !pushOrigins.Contains(t.position));
        List<Tile> nearWalkableTiles = nearTiles.Where(t => GameManager.Get().IsWalkable(t)).ToList();

        if (nearWalkableTiles.Count > 0)
        {
            GameManager.Get().MoveEntity(this, nearWalkableTiles[Random.Range(0, nearWalkableTiles.Count())]);
            return true;
        }
        else
        {
            List<Unit> units = nearTiles.Where(t => t?.entity is Unit).Select(t => t.entity as Unit).ToList();

            while (units.Count > 0)
            {
                int unitIndex = Random.Range(0, units.Count);
                Unit unit = units[unitIndex];
                Vector2Int unitPosition = unit.position;

                List<Vector2Int> newPushOrigins = pushOrigins;

                newPushOrigins.Add(position);

                if (unit.Push(newPushOrigins))
                {
                    GameManager.Get().MoveEntity(this, unitPosition);
                    return true;
                }

                units.RemoveAt(unitIndex);
            }
        }

        return false;
    }
}
