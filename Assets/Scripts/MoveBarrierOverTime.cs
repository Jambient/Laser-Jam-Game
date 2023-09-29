using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBarrierOverTime : MonoBehaviour
{
    public Vector3 newPosition;
    public float animationTime;

    private float elapsedTime = 0;

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        transform.position = Vector3.Lerp(initialPosition, newPosition, Mathf.Clamp(elapsedTime / animationTime, 0, 1));
    }
}
