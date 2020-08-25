using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem ExlosionParticle;

    [HideInInspector]
    public Entity owner;
    [HideInInspector]
    public Entity target;
    int targetLayer;

    public int damage = 1;
    public bool damageNearEntity = false;

    public bool curvedTrajectory = false;
    public float altitudeMultiplicator = 1f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + rb.velocity);
    }

    public void Launch(Entity target)
    {
        Launch(target, Vector3.zero);
    }

    public void Launch(Entity target, Vector3 offset)
    {
        this.target = target;
        targetLayer = target.gameObject.layer;

        if (curvedTrajectory)
            LaunchCurved(target, offset);
        else
            LaunchLinear(target, offset);
    }

    void LaunchCurved(Entity target, Vector3 offset)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.transform.position + offset;
        float gravity = Physics.gravity.y;

        float displacementY = endPosition.y - startPosition.y;
        Vector3 displacementXZ = new Vector3(endPosition.x - startPosition.x, 0, endPosition.z - startPosition.z);

        float h = altitudeMultiplicator * displacementXZ.magnitude;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        rb.velocity = velocityXZ + velocityY;
    }

    void LaunchLinear(Entity target, Vector3 offset)
    {
        rb.useGravity = false;

        float speed = 5f;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.transform.position + offset;
        Vector3 direction = endPosition - startPosition;

        rb.velocity = direction.normalized * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject gameObject = GameManager.Get().AscendToTheBaseObject(other.gameObject);

        if (gameObject == null)
            return;

        Tile tile = gameObject.GetComponent<Tile>();
        Entity entity = gameObject.GetComponent<Entity>();

        // check if entity is on the good layer
        if (entity?.gameObject.layer != targetLayer)
            entity = null;

        if (tile != null)
            DoDamage(tile.position);
        else if (entity != null)
            DoDamage(entity.position);

        if (tile != null || entity != null)
        {
            GameManager.Get().PlayPaticle(ExlosionParticle, transform.position);
            Destroy(this.gameObject);
        }
    }

    void DoDamage(Vector2Int damagePosition)
    {
        Entity entity = GameManager.Get().GetEntity(damagePosition);

        if (entity?.gameObject.layer == targetLayer)
            entity.TakeDamage(owner, damage);

        if (damageNearEntity)
        {
            IEnumerable<Entity> nearEntitys = GameManager.Get().GetNearEntity(damagePosition);

            foreach (Entity nearEntity in nearEntitys)
                if (nearEntity.gameObject.layer == targetLayer)
                    nearEntity.TakeDamage(owner, damage / 2);
        }
    }
}
