using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirusAttack : Virus
{
    public Bullet bullet;

    public int attackRange = 1;

    int spikeCost = 5;

    const int nbSpike = 8;
    GameObject[] spikes = new GameObject[nbSpike];
    bool[] spikeReadys = new bool[nbSpike];

    void Awake()
    {
        InitLife(37);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        for (int i = 0; i < nbSpike; i++)
        {
            spikes[i] = transform.Find("Spike" + i.ToString()).gameObject;
            spikeReadys[i] = false;

            spikes[i].SetActive(false);
        }
    }

    // Update is called once per frame
    new void Update()
    {
        if (isPreview)
            return;

        base.Update();
    }

    protected override void VirusUpdate()
    {
        bool attack = Attack();

        if (!attack)
        {
            if (energy >= spikeCost)
                BuildSpike();
        }

        // energy needed for build missing spikes
        needEnergy = (nbSpike - GetUsableSpikeIndex().Count) * spikeCost;

        // energy needed for regeneration
        needEnergy += hpMax - hp;
    }

    bool Attack()
    {
        List<int> usableSpikeIndex = GetUsableSpikeIndex();

        if (usableSpikeIndex.Count == 0)
            return false;

        List<Entity> entitys = GameManager.Get().GetNearEntityAtRange(position, attackRange).Where(e => !(e is Virus)).ToList();

        if (entitys.Count == 0)
            return false;

        entitys = entitys.OrderBy(e => GameManager.Get().ManhattanDistance(position, e.position)).ToList();

        for (int i = 0; i < usableSpikeIndex.Count && i < entitys.Count; i++)
            LaunchSpike(usableSpikeIndex[i], entitys[i]);

        return true;
    }

    List<int> GetUsableSpikeIndex()
    {
        List<int> usableSpikeIndex = new List<int>();

        for (int i = 0; i < nbSpike; i++)
            if (spikeReadys[i])
                usableSpikeIndex.Add(i);

        return usableSpikeIndex;
    }

    void BuildSpike()
    {
        List<int> toBuildSpikeIndex = new List<int>();

        for (int i = 0; i < nbSpike; i++)
            if (!spikeReadys[i])
                toBuildSpikeIndex.Add(i);

        if (toBuildSpikeIndex.Count == 0)
            return;

        int spikeIndex = toBuildSpikeIndex[Random.Range(0, toBuildSpikeIndex.Count)];

        UseEnergy(spikeCost);

        spikes[spikeIndex].SetActive(true);
        spikeReadys[spikeIndex] = true;
    }

    void LaunchSpike(int index, Entity target)
    {
        if (!spikeReadys[index])
            return;

        spikes[index].SetActive(false);
        spikeReadys[index] = false;

        Vector3 startPosition = spikes[index].transform.position;

        Bullet projectile = Instantiate(bullet, startPosition, spikes[index].transform.rotation);

        projectile.owner = this;
        projectile.Launch(target, new Vector3(0f, 0.4f, 0f));
    }
}
