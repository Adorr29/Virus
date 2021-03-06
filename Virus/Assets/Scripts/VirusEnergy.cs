﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirusEnergy : Virus
{
    void Awake()
    {
        InitLife(14);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
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
        needEnergy = 0;
    }
}
