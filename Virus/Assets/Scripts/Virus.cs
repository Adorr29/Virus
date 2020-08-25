using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Virus : Entity
{
    public ParticleSystem cloneParticle;
    public ParticleSystem transformParticle;

    public Virus[] virusList;

    public MoveBetweenTwoPoint energyObject;

    protected Dictionary<string, Virus> virusDictionary = new Dictionary<string, Virus>();

    public int energyGeneration = 0;
    public int regeneration = 0;

    protected int energy { get; private set; } = 0;
    public int energyMax = 100;
    public int maxEnergyTransfer = 5;

    float updateDelay = 0f;

    // AI variables
    protected int danger = 0;
    protected int needEnergy = 0;

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();

        foreach (Virus virus in virusList)
            virusDictionary.Add(virus.name, virus);
    }

    // Update is called once per frame
    protected new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        if (moving)
            return;

        updateDelay -= Time.deltaTime;

        if (updateDelay <= 0f)
        {
            updateDelay = 1f;

            if (danger > 0)
                danger--;

            SpearDanger();

            if (energyGeneration > 0)
                ReciveEnergy(energyGeneration);

            if (hp < hpMax && regeneration > 0)
                Regeneration();

            VirusUpdate();

            if (danger <= 5 && name != "Virus Energy")
                Transform(virusDictionary["Virus Energy"]);
            else if (danger >= 15 && name == "Virus Energy")
                InDanger();
            else
                Clone();

            if (energy > needEnergy)
                GiveEnergy();
        }
    }

    void SpearDanger()
    {
        IEnumerable<Virus> virusList = GameManager.Get().GetNearEntity(position).Where(e => e is Virus).Select(e => e as Virus);
        int spearDanger = danger - 1;

        foreach (Virus virus in virusList)
            if (virus.danger < spearDanger)
                virus.danger = spearDanger;
    }

    void InDanger()
    {
        IEnumerable<Tile> tiles = GameManager.Get().GetNearTile(position);
        IEnumerable<Tile> groundTiles = tiles.Where(t => t.name == "Ground" && t.entity == null);

        if (groundTiles.Count() > 0)
            Transform(virusDictionary["Virus Wall"]);
        else
        {
            IEnumerable<Entity> entitys = tiles.Where(t => t.entity != null).Select(t => t.entity);

            if (entitys.Where(e => e is VirusWall).Count() > 0 && entitys.Where(e => e is VirusAttack).Count() == 0)
                Transform(virusDictionary["Virus Attack"]);
        }
    }

    int ReciveEnergy(int amount)
    {
        // limite to energyMax
        amount = Mathf.Min(amount, energyMax - energy);

        energy += amount;

        return amount;
    }

    protected int UseEnergy(int amount)
    {
        // limite to energy
        amount = Mathf.Min(amount, energy);

        energy -= amount;

        return amount;
    }

    void Regeneration()
    {
        int regen = hpMax - hp;

        // limite to regeneration
        regen = Mathf.Min(regen, regeneration);

        // limite Heal to energy used
        Heal(UseEnergy(regen));
    }

    void TransferEnergy(Virus virus, int amount)
    {
        // limite to maxEnergyTransfer
        amount = Mathf.Min(amount, maxEnergyTransfer);

        // use only energy recived by virus
        int energyTransferred = UseEnergy(virus.ReciveEnergy(amount));

        // if energy have been transferred run animation
        if (energyTransferred > 0)
        {
            Vector3 offset = new Vector3(0f, 0.8f, 0f);
            MoveBetweenTwoPoint energyObj = Instantiate(energyObject);

            energyObj.transform.parent = virus.transform;
            energyObj.startPoint = transform.position + offset;
            energyObj.endPoint = virus.transform.position + offset;

            energyObj.transform.localScale = new Vector3(energyTransferred, energyTransferred, energyTransferred);
        }
    }

    void GiveEnergy()
    {
        IEnumerable<Virus> virusList = GameManager.Get().GetNearEntity(position).Where(e => e is Virus).Select(e => e as Virus);

        if (virusList.Count() == 0)
            return;

        Virus virus = virusList.OrderBy(v => v.energy - v.needEnergy).First();

        if (virus.energy < virus.needEnergy)
        {
            int giveEnegry = energy - needEnergy;

            // limit energy transfer to virus needEnergy
            giveEnegry = Mathf.Min(giveEnegry, virus.needEnergy - virus.energy);

            // limit energy transfer to maxEnergyTransfer
            giveEnegry = Mathf.Min(giveEnegry, maxEnergyTransfer);

            TransferEnergy(virus, giveEnegry);
        }
        else
            SmoothEnergy();
    }

    void SmoothEnergy()
    {
        IEnumerable<Virus> virusList = GameManager.Get().GetNearEntity(position).Where(e => (e as Virus)?.energy < energy).Select(e => e as Virus);

        if (virusList.Count() == 0)
            return;

        int averageEnergy = energy;

        foreach (Virus virus in virusList)
            averageEnergy += virus.energy;
        averageEnergy = Mathf.FloorToInt(averageEnergy / (float)(virusList.Count() + 1));

        foreach (Virus virus in virusList)
            TransferEnergy(virus, averageEnergy - virus.energy);
    }

    void Clone(Tile cloneTile = null)
    {
        if (cloneTile == null)
        {
            IEnumerable<Tile> tiles = GameManager.Get().GetNearTile(position).Where(t => t.name == "Ground" && t.entity == null);

            if (tiles.Count() == 0)
                return;

            cloneTile = tiles.ToList()[Random.Range(0, tiles.Count())];
        }

        if (energy < energyCost)
        {
            needEnergy = energyCost;
            return;
        }
        UseEnergy(energyCost);

        Virus clone = Instantiate(this);

        GameManager.Get().AddEntity(clone, cloneTile.position, position);

        GameManager.Get().PlayPaticle(cloneParticle, transform.position);
    }

    void Transform(Virus transform)
    {
        if (name == transform.name)
            return;

        int transformCost = transform.energyCost - energyCost;

        if (energy < transformCost)
        {
            needEnergy = transformCost;
            return;
        }

        Virus virus = Instantiate(transform);

        GameManager.Get().RemoveEntity(this);
        GameManager.Get().AddEntity(virus, position);

        virus.energy = energy;
        virus.updateDelay = updateDelay;

        // keep same percentage of hp
        virus.SetHp(Mathf.FloorToInt(hp / (float)hpMax * virus.hpMax));
        if (virus.hp < 1)
            virus.SetHp(1);

        // copy Virus AI vars
        virus.danger = danger;

        if (transformCost > 0)
            virus.UseEnergy(transformCost);
        else
            virus.ReciveEnergy(-transformCost);

        GameManager.Get().PlayPaticle(transformParticle, this.transform.position);

        Destroy(gameObject);
    }

    protected override void OnDamageTaked(Entity attacker, int damage)
    {
        danger += damage;

        // limite danger level to 30
        danger = Mathf.Min(danger, 30);

        // if damage is too high SpearDanger urgently
        if (damage >= hpMax)
            SpearDanger();
    }

    protected abstract void VirusUpdate();
}
