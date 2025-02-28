using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles drawing functionality in VR using Oculus hands.
/// This script allows the user to toggle drawing mode, select stroke types (line or sphere),
/// and save or clear their sketches. It also keeps track of drawing progress.
/// </summary>
public class Draw : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject brush; // Prefab for the brush used for drawing lines.
    [SerializeField] OVRHand righthand; // Reference to the right Oculus hand.
    [SerializeField] OVRHand lefthand; // Reference to the left Oculus hand.
    [SerializeField] OVRSkeleton righthandSkeleton; // Skeleton for the right hand.
    [SerializeField] OVRSkeleton lefthandSkeleton; // Skeleton for the left hand.
    [SerializeField] GameObject currrentSphereMarker; // Prefab for the sphere marker.
    [SerializeField] Button retrievalButton;
    [SerializeField] GameObject retrievalStatusPanel;
    [SerializeField] TMPro.TMP_Text statusTextField;
    [SerializeField] GameObject prefab;
    [SerializeField] Transform objPlaceholder;


    [Header("Drawing Settings")]
    public bool isDrawingModeEnabled; // Indicates if drawing mode is active.
    public bool unsavedProgressExists; // Indicates if there are unsaved changes in the sketch.

    public enum StrokeType
    {
        line,   // Line stroke type.
        sphere  // Sphere stroke type.
    }
    private StrokeType currentStrokeType; // Stores the current stroke type.

    private LineRenderer currentLineRenderer; // Current LineRenderer for line drawing.
    private Transform handIndexTipTransform; // Transform of the index finger tip.
    private bool lineStartingPointExists; // Tracks if the starting point for a line exists.
    private Vector3 lastPos; // Last position to avoid redundant object spawning.

    private List<List<Vector3>> completeSketchPosList; // Stores the entire sketch as a collection of strokes.
    private List<Vector3> partialSketchPosList; // Stores positions for the current stroke.
    private List<GameObject> spawnedSketchObjects; // Stores all spawned objects for clearing or saving.

    /// <summary>
    /// Toggles the drawing mode on or off.
    /// </summary>
    /// <param name="isDrawingModeEnabled">Boolean to enable or disable drawing mode.</param>
    public void ToggleDrawingMode(bool isDrawingModeEnabled)
    {
        this.isDrawingModeEnabled = isDrawingModeEnabled;

    }

    /// <summary>
    /// Selects the type of stroke to draw (line or sphere).
    /// </summary>
    /// <param name="selectedIndex">0 for line, 1 for sphere.</param>
    public void SelectStrokeType(int selectedIndex)
    {
        currentStrokeType = selectedIndex switch
        {
            0 => StrokeType.line,
            1 => StrokeType.sphere,
            _ => StrokeType.sphere,
        };
    }

    /// <summary>
    /// Draws a continuous line based on the position of the hand's index finger tip.
    /// </summary>
    /// <param name="handIndexTipTransform">Transform of the hand's index finger tip.</param>
    void DrawLine(Transform handIndexTipTransform)
    {
        Vector3 markerPosition = handIndexTipTransform.transform.position;
        if (!lineStartingPointExists)
        {
            GameObject brushInstance = Instantiate(brush);
            spawnedSketchObjects.Add(brushInstance);
            currentLineRenderer = brushInstance.GetComponent<LineRenderer>();
            currentLineRenderer.SetPosition(0, markerPosition);
            currentLineRenderer.SetPosition(1, markerPosition);
            lastPos = markerPosition;
            partialSketchPosList.Add(lastPos);
            lineStartingPointExists = true;
        }
        else
        {
            if (lastPos != markerPosition)
            {

                currentLineRenderer.positionCount++;
                int positionIndex = currentLineRenderer.positionCount - 1;
                currentLineRenderer.SetPosition(positionIndex, markerPosition);
                lastPos = markerPosition;
                partialSketchPosList.Add(lastPos);
            }
        }
    }

    Color GenerateStrokeColor(Vector3 brushPosition)
    {
        return Color.HSVToRGB(
            Mathf.Sqrt(Mathf.Pow(brushPosition.x, 2))/10f,
            Mathf.Sqrt(Mathf.Pow(brushPosition.y, 2))/10f,
            Mathf.Sqrt(Mathf.Pow(brushPosition.z, 2))/10f
            );
    }

    /// <summary>
    /// Spawns a sphere marker at the position of the hand's index finger tip.
    /// </summary>
    /// <param name="handIndexTipTransform">Transform of the hand's index finger tip.</param>
    void SpawnSphere(Transform handIndexTipTransform)
    {
        Vector3 markerPosition = handIndexTipTransform.transform.position;
        Quaternion markerRotation = handIndexTipTransform.transform.rotation;
        if (lastPos != markerPosition)
        {
            var sphereMark = Instantiate(currrentSphereMarker, markerPosition, markerRotation);
            var renderer = sphereMark.GetComponent<Renderer>();            
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = GenerateStrokeColor(markerPosition);
            spawnedSketchObjects.Add(sphereMark);
            lastPos = markerPosition;
            partialSketchPosList.Add(lastPos);
        }
    }

    /// <summary>
    /// Saves the sketch data and clears the current sketch from the scene.
    /// </summary>
    public void SaveAndClearSketch()
    {
        SaveSketch();
        //StartCoroutine(Waiter());
        ClearSketch();
    }

    /// <summary>
    /// Clears all drawn objects and resets sketch data.
    /// </summary>
    public void ClearSketch()
    {
        foreach (var obj in spawnedSketchObjects)
        {
            Destroy(obj);
        }
        spawnedSketchObjects.Clear();
        partialSketchPosList.Clear();
        completeSketchPosList.Clear();
    }

    /// <summary>
    /// Saves the current sketch data to a CSV file.
    /// </summary>
    public void SaveSketch()
    {
        System.DateTime currentDateTime = System.DateTime.Now;
        string filename = currentDateTime.ToString("dd-MM-YYYY_HH-mm-ss");
        string filePath = Path.Combine(Application.persistentDataPath, $"my_data_{filename}.csv");
        Debug.Log($"----------Saving sketch.........  : {completeSketchPosList.Count}");
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine("sketch_id,x,y,z");
            int a = 0;
            if (completeSketchPosList.Count > 0)
            {
                foreach (var i in completeSketchPosList)
                {
                    foreach (var j in i)
                    {
                        Debug.Log($"--------------- { a},{ j.x:F3},{ j.y:F3},{ j.z:F3} ---------------");
                        writer.WriteLine($"{a},{j.x:F3},{j.y:F3},{j.z:F3}");

                    }
                    a++;
                }
            }
        }
    }

    public void RetrieveObj()
    {
        // spawn a new object here. Get it from prefabs for demo purposes.

        StartCoroutine(Waiter());
    }

    private void Start()
    {
        partialSketchPosList = new();
        completeSketchPosList = new();
        spawnedSketchObjects = new();
        lastPos = Vector3.zero;
        lineStartingPointExists = false;
        isDrawingModeEnabled = false;
        unsavedProgressExists = false;
        retrievalButton.onClick.AddListener(RetrieveObj);
    }

    private void Update()
    {

        if (isDrawingModeEnabled && lefthand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            foreach (var b in righthandSkeleton.Bones)
            {

                if (b.Id == OVRSkeleton.BoneId.Hand_Middle2) // there is a mismatch in indexing of fingers. Hand_Middle2 is mapped to index finger tip.
                {

                    handIndexTipTransform = b.Transform;
                    break;
                }
            }

            switch (currentStrokeType)
            {
                case StrokeType.line:
                    DrawLine(handIndexTipTransform);
                    break;

                case StrokeType.sphere:
                    SpawnSphere(handIndexTipTransform);
                    break;

                default:
                    SpawnSphere(handIndexTipTransform);
                    break;
            }
            unsavedProgressExists = true;
        }
        else
        {
            lineStartingPointExists = false; // in line drawing mode, to ensure next time when user starts drawing, a new line is generated.
            if (unsavedProgressExists)
            {
                completeSketchPosList.Add(partialSketchPosList);
                unsavedProgressExists = false;
                partialSketchPosList = new List<Vector3>();
            }
        }
    }

    IEnumerator Waiter()
    {
        retrievalStatusPanel.SetActive(true);
        statusTextField.text = "Retrieving the object...";
        yield return new WaitForSeconds(3f);
        //retrievalStatusPanel.SetActive(false);
        Debug.Log("Here is your object from sketch.");
        retrievalStatusPanel.SetActive(false);
        var g = Instantiate(prefab, objPlaceholder);

    }
}