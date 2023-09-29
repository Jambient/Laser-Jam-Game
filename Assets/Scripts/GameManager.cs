using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public Tilemap floorTilemap;

    [SerializeField] private GameObject laserPrefab;

    [SerializeField] private GameObject emitters;
    [SerializeField] private GameObject receivers;
    [SerializeField] private GameObject lasers;

    private string currentActiveItem = "";
    private GameObject tempActiveObject;
    private TextMeshProUGUI buttonTextComponent;

    public GameObject levelCompleteFrame;

    private void Start()
    {
        Bounds tilemapBounds = floorTilemap.localBounds;
        List<Vector2> edgePoints = new List<Vector2>();
        edgePoints.Add(tilemapBounds.min);
        edgePoints.Add(tilemapBounds.max - new Vector3(0, tilemapBounds.extents.y * 2, 0));
        edgePoints.Add(tilemapBounds.max);
        edgePoints.Add(tilemapBounds.min + new Vector3(0, tilemapBounds.extents.y * 2, 0));
        edgePoints.Add(tilemapBounds.min);
        floorTilemap.GetComponent<EdgeCollider2D>().SetPoints(edgePoints);

        CalculateLasers();
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

        //laser.AddComponent<BoxCollider2D>();
        //laser.GetComponent<BoxCollider2D>().isTrigger = true;

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
            System.Array.Sort(hit, (x, y) => x.distance.CompareTo(y.distance));

            // it is probably garateed here that hit has at least 2 items
            // as it collides with the inital collider at currentPos and in an empty scenario the grid walls.
            // im just gonna hope no murphy's law gonna be happening here

            int bestIndex = 1;
            //Debug.Log(hit.Length);
            //for (int i = 1; i < hit.Length; i++)
            //{
            //    bestIndex = i;

            //    RaycastHit2D testingCollisionData = hit[i];
            //    if (testingCollisionData.collider.isTrigger && testingCollisionData.transform.tag == "Laser")
            //    {
            //        Gradient otherLaserGradient = testingCollisionData.transform.gameObject.GetComponent<LineRenderer>().colorGradient;
            //        if (color != otherLaserGradient.Evaluate(0))
            //        {
            //            break;
            //        }
            //    }
            //    {
            //        break;
            //    }
            //}

            RaycastHit2D collisionData = hit[bestIndex];
            Vector3 collisionPos = collisionData.point;

            Debug.Log(bestIndex);

            if (collisionData.collider.isTrigger)
            {
                string collisionTag = collisionData.transform.tag;

                Debug.Log(collisionTag);

                if (collisionTag == "Mirror")
                {
                    collisionPos += currentDir * 0.45f;
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
                if (collisionTag == "Splitter")
                {
                    collisionPos += currentDir * 0.45f;
                    CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));

                    CreateLaser(color, collisionPos, collisionPos + collisionData.transform.right * 0.48f);
                    CreateLaser(color, collisionPos, collisionPos - collisionData.transform.right * 0.48f);

                    SimulateLaser(active_recievers, color, collisionPos + collisionData.transform.right*0.48f, collisionData.transform.right);
                    SimulateLaser(active_recievers, color, collisionPos - collisionData.transform.right * 0.48f, -collisionData.transform.right);
                    SimulateLaser(active_recievers, color, collisionPos - collisionData.transform.up * 0.48f, -collisionData.transform.up);
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
                //if (collisionTag == "Laser")
                //{
                //    CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));
                //    isFinished = true;
                //}
                //if (collisionTag == "Laser")
                //{
                //    Gradient otherLaserGradient = collisionData.transform.gameObject.GetComponent<LineRenderer>().colorGradient;

                //    CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));

                //    if (color == otherLaserGradient.Evaluate(0)) {;
                //        Debug.Log("lasers are the same color");
                //        // lasers are the same colour
                //        //collisionPos += currentDir * 0.02f;
                //    } else
                //    {
                //        Debug.Log("lasers are different colors");
                //        // lasers are different colours
                //        isFinished = true;
                //    }
                //}
            }
            else
            {
                CreateLaser(color, currentPos, new Vector3(collisionPos.x, collisionPos.y, -3));
                isFinished = true;
            }

            currentPos = new Vector3(collisionPos.x, collisionPos.y, -3);

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

            levelCompleteFrame.SetActive(true);
            Invoke("LevelSelectButton", 2);
        } else
        {
            Debug.Log("failed!");
        }
    }

    private void Update()
    {
        if (currentActiveItem != "")
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                tempActiveObject.transform.Rotate(0, 0, -90);
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = floorTilemap.WorldToCell(mousePos);

            if (floorTilemap.cellBounds.Contains(cellPosition))
            {
                tempActiveObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
                tempActiveObject.transform.position = cellPosition + new Vector3(0.5f, 0.5f, 0);

                if (Input.GetMouseButtonDown(0))
                {
                    // place the active object
                    tempActiveObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

                    currentActiveItem = "";
                    tempActiveObject = null;

                    int currentCount = int.Parse(buttonTextComponent.GetParsedText());
                    buttonTextComponent.SetText((currentCount - 1).ToString());

                    CalculateLasers();
                }
            } else
            {
                tempActiveObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0f);
            }
        }
    }

    public void LevelSelectButton()
    {
        SceneManager.LoadScene(1);
    }

    public void ResetSceneButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SelectItem(string itemString)
    {
        GameObject pressedButton = EventSystem.current.currentSelectedGameObject;
        buttonTextComponent = pressedButton.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>();
        int currentCount = int.Parse(buttonTextComponent.GetParsedText());

        if (currentCount == 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return; 
        }
        if (tempActiveObject) { Destroy(tempActiveObject); }

        currentActiveItem = itemString;
        tempActiveObject = Instantiate(Resources.Load("Prefabs/" + itemString, typeof(GameObject))) as GameObject;

        tempActiveObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
    }
}
