using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject laserPrefab;

    [SerializeField] private GameObject emitters;
    [SerializeField] private GameObject receivers;
    [SerializeField] private GameObject lasers;

    private void Start()
    {
        InvokeRepeating("CalculateLasers", 1f, 50f);
    }

    private void CreateLaser(Color color, Vector3 pos1, Vector3 pos2)
    {
        Vector3[] positions = new Vector3[2];
        positions[0] = pos1;
        positions[1] = pos2;

        GameObject laser = Instantiate(laserPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));

        var lineRenderer = laser.GetComponent<LineRenderer>();
        lineRenderer.SetPositions(positions);

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0.0f), new GradientAlphaKey(1f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        laser.transform.SetParent(lasers.transform);
    }

    private void SimulateLaser(List<GameObject> active_recievers, Color color, Vector3 startPos, Vector3 startDir)
    {
        bool isFinished = false;
        Vector3 currentPos = startPos;
        Vector3 currentDir = startDir;

        RaycastHit2D[] hit;

        while (!isFinished)
        {
            hit = Physics2D.RaycastAll(currentPos, currentDir, 300);

            // it is probably garateed here that hit has at least 2 items
            // as it collides with the inital collider at currentPos and in an empty scenario the grid walls.
            // im just gonna hope no murphy's law gonna be happening here

            RaycastHit2D collisionData = hit[1];
            Vector3 collisionPos = collisionData.point;

            if (collisionData.collider.isTrigger)
            {
                string collisionTag = collisionData.transform.tag;
                if (collisionTag == "Mirror")
                {
                    collisionPos += currentDir * 0.4f;
                    CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));

                    if (currentDir != collisionData.transform.up)
                    {
                        currentDir = -collisionData.transform.up;
                    }
                    else
                    {
                        currentDir = -collisionData.transform.right;
                    }
                }
                if (collisionTag == "Receiver")
                {
                    collisionPos += currentDir * 0.5f;
                    CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));

                    isFinished = true;

                    if (collisionData.transform.gameObject.GetComponent<SpriteRenderer>().color == color)
                    {
                        active_recievers.Remove(collisionData.transform.gameObject);
                    }
                }
            }
            else
            {
                CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));
                isFinished = true;
            }

            currentPos = collisionPos;

        }
    }

    private void CalculateLasers()
    {
        // clear lasers
        foreach (Transform child in lasers.transform)
        {
            Destroy(child.gameObject);
        }

        // get receivers
        List<GameObject> active_recievers = new List<GameObject>();
        foreach (Transform child in receivers.transform)
        {
            active_recievers.Add(child.gameObject);
        }

        // loop through emitters and run laser simulation
        foreach (Transform emitter in emitters.transform)
        {
            SimulateLaser(active_recievers, emitter.gameObject.GetComponent<SpriteRenderer>().color, emitter.position, -emitter.up);
        }

        if (active_recievers.Count == 0)
        {
            Debug.Log("success!");
        } else
        {
            Debug.Log("failed!");
        }
    }
}
