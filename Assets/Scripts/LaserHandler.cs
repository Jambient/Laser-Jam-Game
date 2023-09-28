using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHandler : MonoBehaviour
{
    public Vector3[] positions;
    void Start()
    {
        gameObject.GetComponent<LineRenderer>().SetPositions(positions);
    }
}
