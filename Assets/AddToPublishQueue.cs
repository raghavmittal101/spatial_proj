using UnityEngine;

public class AddToPublishQueue : MonoBehaviour
{
    public TrajectoryPlanner trajectoryPlanner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trajectoryPlanner.AddTargetToQueue(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
