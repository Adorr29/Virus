using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Entity
{
    void Awake()
    {
        InitLife(120);
    }

    // Start is called before the first frame update
    new void Start()
    {
        if (isPreview)
            return;

        base.Start();
    }
}
