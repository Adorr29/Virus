using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    public GameObject[] tiles;
    public Entity startBuild;
    public Unit playerUnit;
    public Virus virusUnit;

    Dictionary<string, GameObject> tileDictionary = new Dictionary<string, GameObject>();

    public Vector2Int size;
    public Tile[,] tab { get; private set; }

    int[,] heightTab;

    public AnimationCurve mountainCurve;
    public AnimationCurve spikeCurve;

    // Start is called before the first frame update
    void Start()
    {
        // fill tileDictionary
        foreach (GameObject tile in tiles)
            tileDictionary.Add(tile.name, tile);

        // Init
        tab = new Tile[size.x, size.y];
        heightTab = new int[size.x, size.y];

        Generate();

        List<Vector2Int> groundPositions = new List<Vector2Int>();

        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                if (tab[i, j].name == "Ground" && tab[i + 1, j].name == "Ground")
                    groundPositions.Add(new Vector2Int(i, j));

        Vector2Int playerSpawnPosition = groundPositions[Random.Range(0, groundPositions.Count)];

        Entity build = Instantiate(startBuild);
        Unit player = Instantiate(playerUnit);
        GameManager.Get().AddEntity(build, playerSpawnPosition);
        GameManager.Get().AddEntity(player, playerSpawnPosition + new Vector2Int(1, 0));

        groundPositions.Clear();

        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                if (tab[i, j].name == "Ground" && Vector2.Distance(playerSpawnPosition, new Vector2(i, j)) > (size.x + size.y) / 4f)
                    groundPositions.Add(new Vector2Int(i, j));

        Vector2Int virusSpawnPosition = groundPositions[Random.Range(0, groundPositions.Count)];

        Virus virus = Instantiate(virusUnit);
        GameManager.Get().AddEntity(virus, virusSpawnPosition);
    }

    void PutMountain(Vector2Int startPosition, int height, float thickness, AnimationCurve curve)
    {
        Vector2Int[] offsets = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        List<Vector2Int> openPositions = new List<Vector2Int>();
        List<Vector2Int> closePositions = new List<Vector2Int>();

        openPositions.Add(startPosition);

        while (openPositions.Count > 0)
        {
            int index = Random.Range(0, openPositions.Count);
            Vector2Int position = openPositions[index];
            float distance = Vector2Int.Distance(startPosition, position) / thickness;

            openPositions.RemoveAt(index);
            closePositions.Add(position);

            int tileHeight = Mathf.RoundToInt(curve.Evaluate(distance) * height);

            if (tileHeight > heightTab[position.x, position.y])
                heightTab[position.x, position.y] = tileHeight;

            if (heightTab[position.x, position.y] > 0)
            {
                foreach (Vector2Int offset in offsets)
                {
                    Vector2Int newPosition = position + offset;

                    if (newPosition.x < 0 || newPosition.y < 0 || newPosition.x >= size.x || newPosition.y >= size.y)
                        continue;

                    if (!openPositions.Contains(position + offset) && !closePositions.Contains(position + offset))
                        openPositions.Add(position + offset);
                }
            }
        }
    }

    void Generate()
    {
        int nbMountain = Mathf.RoundToInt(Random.Range((size.x + size.y) / 30f, (size.x + size.y) / 20f));

        for (int i = 0; i < nbMountain; i++)
            PutMountain(new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y)), 8, Random.Range(4, 12), mountainCurve);

        int nbSpike = Mathf.RoundToInt(Random.Range((size.x + size.y) / 10f, (size.x + size.y) / 1f));

        for (int i = 0; i < nbSpike; i++)
            PutMountain(new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y)), 4, Random.Range(1, 3), spikeCurve);

        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
            {
                if (i == 0 || j == 0 || i == size.x - 1 || j == size.y - 1)
                    if (heightTab[i, j] == 0)
                        heightTab[i, j] = (i + j) % 2 == 0 ? 2 : 1;

                if (heightTab[i, j] == 0)
                    tab[i, j] = AddTile("Ground", new Vector2Int(i, j));
                else
                    tab[i, j] = AddTile("Wall" + heightTab[i, j].ToString(), new Vector2Int(i, j));
            }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public Tile GetTile(Vector2Int position) // ??
    {
        return tab[position.x, position.y];
    }

    Tile AddTile(string tileName, Vector2Int position)
    {
        Tile tile = Instantiate(tileDictionary[tileName], new Vector3(position.x, 0, position.y), transform.rotation, transform).GetComponent<Tile>();

        tile.name = tileName; // ?
        tile.type = tileName; // ??
        tile.position = position;
        tile.entity = null; // useless

        return tile;
    }
}
