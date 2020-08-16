using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinMaxValue
{
    public float min;
    public float max;

    public MinMaxValue()
    {
        min = 0;
        max = 0;
    }

    public MinMaxValue(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float borderSize = 10f;

    public float zoomSpeed = 10f;
    public MinMaxValue zoomLimite = new MinMaxValue(5, 50);

    public float lerpSpeed = 15f;

    Vector3 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        ZoomCamera();

        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
    }

    private void MoveCamera()
    {
        if (Input.mousePosition.x <= borderSize)
            targetPosition.x -= moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y <= borderSize)
            targetPosition.z -= moveSpeed * Time.deltaTime;
        if (Input.mousePosition.x >= Screen.width - borderSize)
            targetPosition.x += moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y >= Screen.height - borderSize)
            targetPosition.z += moveSpeed * Time.deltaTime;
    }

    private void ZoomCamera()
    {
        float targetPositionY = targetPosition.y - zoomSpeed * Input.GetAxis("Mouse ScrollWheel");

        targetPosition.y = Mathf.Clamp(targetPositionY, zoomLimite.min, zoomLimite.max);
    }
}
