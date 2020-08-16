using UnityEngine;

public class MoveBetweenTwoPoint : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;

    public float moveDuration = 1f;

    float elapseTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPoint;
    }

    // Update is called once per frame
    void Update()
    {
        elapseTime += Time.deltaTime;

        if (elapseTime < moveDuration)
            transform.position = Vector3.Lerp(startPoint, endPoint, elapseTime / moveDuration);
        else
            Destroy(gameObject);
    }
}
