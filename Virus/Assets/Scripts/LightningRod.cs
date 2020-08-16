using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningRod : Entity
{
    public ParticleSystem generateParticle;

    public int enegryGenerate;
    public float generateTime;

    float generateDelay = 0f;

    void Awake()
    {
        InitLife(30);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        generateDelay = generateTime;
    }

    // Update is called once per frame
    new void Update()
    {
        if (isPreview)
            return;

        base.Update();

        if (buildProgress < 1f)
            return;

        generateDelay -= Time.deltaTime;

        if (generateDelay <= 0f)
        {
            generateDelay = generateTime;

            Player.Get().energy += enegryGenerate;

            GameManager.Get().PlayPaticle(generateParticle, transform.Find("GeneratePosition").position);
        }
    }
}
