using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusWall : Virus
{
    void Awake()
    {
        InitLife(73);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }

    protected override void VirusUpdate()
    {
        // energy needed for regeneration
        needEnergy = hpMax - hp;
    }
}
