using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public World world { get; private set; }

    public Material previewMaterial;

    // Start is called before the first frame update
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public IEnumerable<Tile> GetNearTile(Vector2Int position)
    {
        List<Tile> tiles = new List<Tile>();

        List<Vector2Int> positions = new List<Vector2Int> {
            position + new Vector2Int(0, -1),
            position + new Vector2Int(0, 1),
            position + new Vector2Int(-1, 0),
            position + new Vector2Int(1, 0)};

        foreach (Vector2Int pos in positions)
        {
            // if pos is out of world, skip this pos
            if (pos.x < 0 || pos.y < 0 || pos.x >= world.size.x || pos.y >= world.size.y)
                continue;

            tiles.Add(world.tab[pos.x, pos.y]);
        }

        return tiles;
    }

    public IEnumerable<Entity> GetNearEntity(Vector2Int position)
    {
        return GetNearTile(position).Where(t => t.entity != null).Select(t => t.entity);
    }

    public IEnumerable<Tile> GetNearTileAtRange(Vector2Int position, int range)
    {
        List<Tile> tiles = new List<Tile>();

        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                Vector2Int tilePosition = position + new Vector2Int(i, j);

                // if tilePosition is out of world, skip this tilePosition
                if (tilePosition.x < 0 || tilePosition.y < 0 || tilePosition.x >= world.size.x || tilePosition.y >= world.size.y)
                    continue;

                if (ManhattanDistance(position, tilePosition) <= range)
                    tiles.Add(GetTile(tilePosition));
            }

        return tiles;
    }

    public IEnumerable<Entity> GetNearEntityAtRange(Vector2Int position, int range)
    {
        return GetNearTileAtRange(position, range).Where(t => t.entity != null).Select(t => t.entity);
    }

    public void AddEntity(Entity entity, Vector2Int position, Vector2Int? spawnPosition = null)
    {
        // check if tile is walkable
        if (!IsWalkable(position, out Tile tile))
            throw new System.ArgumentException("Tile is not walkable");

        Transform tileTransfrom = tile.transform.Find("EntityPosition").transform;

        tile.entity = entity;
        entity.position = position;
        entity.transform.parent = tileTransfrom;

        if (spawnPosition.HasValue) {
            Tile spawnTile = world.tab[spawnPosition.Value.x, spawnPosition.Value.y];
            Transform spawnTileTransfrom = spawnTile.gameObject.transform.Find("EntityPosition").transform;

            entity.transform.position = spawnTileTransfrom.position;

            // run move animation
            entity.StartCoroutine(entity.MoveAnimation());
        }
        else
            entity.transform.position = tileTransfrom.position;
    }

    public void RemoveEntity(Vector2Int position)
    {
        Entity entity = GetEntity(position);

        if (entity != null)
            RemoveEntity(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        Tile tile = GetTile(entity.position);

        if (tile != null)
            tile.entity = null;
    }

    public void MoveEntity(Entity entity, Tile tile)
    {
        // check if entity move
        if (tile.position == entity.position)
            throw new System.ArgumentException("Entity dont move"); // TODO remove this ?

        // check if is walkable
        if (!IsWalkable(tile))
            throw new System.ArgumentException("Tile is not walkable");

        // check if entity move too far
        if ((tile.position - entity.position).sqrMagnitude > 1)
            throw new System.ArgumentException("Entity move too far");

        // if entity can rotate run rotate animation
        if (entity.canRotate)
        {
            Vector2Int direction = tile.position - entity.position;

            if (direction.x > 0)
                RotateEntity(entity, 0);
            else if (direction.x < 0)
                RotateEntity(entity, 180);
            else if (direction.y > 0)
                RotateEntity(entity, 270);
            else if (direction.y < 0)
                RotateEntity(entity, 90);
        }

        // remove entity of this position
        GetTile(entity.position).entity = null;

        // set position and parent
        tile.entity = entity;
        entity.position = tile.position;
        entity.transform.parent = tile.transform.Find("EntityPosition").transform;

        // run move animation
        entity.StartCoroutine(entity.MoveAnimation());
    }

    public void MoveEntity(Entity entity, Vector2Int position)
    {
        // check if tile is walkable
        if (!IsWalkable(position, out Tile tile))
            throw new System.ArgumentException("Tile is not walkable");

        MoveEntity(entity, tile);
    }

    public void RotateEntity(Entity entity, float rotation)
    {
        entity.StartRotation(rotation);
    }

    public bool IsWalkable(Vector2Int position, out Tile tile)
    {
        tile = GetTile(position);

        return IsWalkable(tile);
    }

    public bool IsWalkable(Vector2Int position)
    {
        Tile tile = GetTile(position);

        return IsWalkable(tile);
    }

    public bool IsWalkable(Tile tile)
    {
        // check if tile is not Ground
        if (!IsGround(tile))
            return false;

        // check if have already entity
        if (tile.entity != null)
            return false;

        return true;
    }

    public bool IsGround(Tile tile)
    {
        return tile.name == "Ground"; // name or type ?
    }

    public Tile GetTile(Vector2Int position)
    {
        return world.tab[position.x, position.y];
    }

    public Entity GetEntity(Vector2Int position)
    {
        return GetTile(position).entity;
    }

    // for FindPath
    public delegate bool CanWalkThrough(Tile tile);

    public List<Vector2Int> FindPath(Vector2Int startPosition, Vector2Int targetPosition, int distanceToTarget = 0)
    {
        return FindPath(startPosition, targetPosition, IsWalkable, distanceToTarget);
    }
    public List<Vector2Int> FindPath(Vector2Int startPosition, Vector2Int targetPosition, CanWalkThrough canWalkThrough, int distanceToTarget = 0)
    {
        return FindPath(GetTile(startPosition), GetTile(targetPosition), canWalkThrough, distanceToTarget);
    }

    public List<Vector2Int> FindPath(Tile startTile, Tile targetTile, int distanceToTarget = 0)
    {
        return FindPath(startTile, targetTile, IsWalkable, distanceToTarget);
    }

    public List<Vector2Int> FindPath(Tile startTile, Tile targetTile, CanWalkThrough canWalkThrough, int distanceToTarget = 0)
    {
        if (startTile == targetTile)
            return null;

        if (distanceToTarget == 0 && !canWalkThrough(targetTile))
            return null;

        List<Tile> openList = new List<Tile>();
        List<Tile> closeList = new List<Tile>();

        openList.Add(startTile);

        while (openList.Count > 0)
        {
            int currentIndex = MinimumCostIndex(openList);
            Tile current = openList[currentIndex];

            openList.RemoveAt(currentIndex);
            closeList.Add(current);

            if (current != startTile && current.hCost <= distanceToTarget)
                return CreatePath(startTile, current);

            foreach (Tile neighbour in GetNearTile(current.position))
            {
                if (!canWalkThrough(neighbour) || closeList.Contains(neighbour))
                    continue;

                bool neighbourInOpenList = openList.Contains(neighbour);

                int distance = ManhattanDistance(current.position, neighbour.position);
                int gCost = current.gCost + distance;
                int hCost = ManhattanDistance(neighbour.position, targetTile.position);

                if (!neighbourInOpenList || gCost < neighbour.gCost)
                {
                    neighbour.gCost = gCost;
                    neighbour.hCost = hCost;
                    neighbour.parent = current;

                    if (!neighbourInOpenList)
                        openList.Add(neighbour);
                }
            }
        }

        return null; // null or empty list ?
    }

    public List<Vector2Int> CreatePath(Tile startTile, Tile targetTile)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Tile current = targetTile;

        while (current != startTile)
        {
            path.Insert(0, current.position);
            current = current.parent;
        }

        return path;
    }

    int MinimumCostIndex(List<Tile> list)
    {
        int min = 0;

        for (int i = 1; i < list.Count; i++)
            if (list[i].fCost < list[min].fCost || list[i].fCost == list[min].fCost && list[i].hCost < list[min].hCost)
                min = i;

        return min;
    }

    public int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(b.x - a.x) + Mathf.Abs(b.y - a.y);
    }

    public void ToPreview(GameObject gameObject)
    {
        Renderer renderer = gameObject.transform.GetComponent<Renderer>();
        Collider collider = gameObject.transform.GetComponent<Collider>();
        Entity entity = gameObject.GetComponent<Entity>();

        if (renderer != null)
        {
            renderer.material = previewMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        if (collider != null)
        {
            collider.enabled = false;
        }

        if (entity != null)
        {
            entity.isPreview = true;
        }

        for (int i = 0; i < gameObject.transform.childCount; i++)
            ToPreview(gameObject.transform.GetChild(i).gameObject);
    }

    public void ValidPreviewPosition(bool isValid)
    {
        if (isValid)
            previewMaterial.color = new Color(0f, 0.25f, 0f, 0.5f);
        else
            previewMaterial.color = new Color(0.25f, 0f, 0f, 0.5f);
    }

    public float GetBestAngle(Entity entity, Entity target)
    {
        return GetBestAngle(entity.position, target.position);
    }

    public float GetBestAngle(Vector2Int entityPosition, Vector2Int targetPosition)
    {
        Vector2Int direction = targetPosition - entityPosition;

        if (direction == Vector2Int.zero)
            return 0;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && direction.x > 0)
            return 0;
        else if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && direction.x < 0)
            return 180;
        else if (Mathf.Abs(direction.x) <= Mathf.Abs(direction.y) && direction.y > 0)
            return 270;
        else if (Mathf.Abs(direction.x) <= Mathf.Abs(direction.y) && direction.y < 0)
            return 90;

        throw new System.Exception("What ??");
    }

    // ascend to the base of game object
    public GameObject AscendToTheBaseObject(GameObject gameObject)
    {
        if (gameObject == null)
            return null;

        if (gameObject.transform.parent == null)
            return gameObject;

        if (gameObject.transform.parent.gameObject == null)
            return null;

        if (gameObject.layer != gameObject.transform.parent.gameObject.layer)
            return gameObject;

        return AscendToTheBaseObject(gameObject.transform.parent.gameObject);
    }

    public void PlayPaticle(ParticleSystem particle, Vector3 position)
    {
        if (particle == null)
            return;

        // play and destroy paticle
        Destroy(Instantiate(particle, position, Quaternion.identity).gameObject, particle.main.duration);
    }

    static public GameManager Get() // ?
    {
        return GameObject.Find("GameManager").GetComponent<GameManager>();
    }
}
