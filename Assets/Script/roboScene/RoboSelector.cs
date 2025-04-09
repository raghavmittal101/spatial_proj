using System.Collections.Generic;
using UnityEngine;

public class RoboSelector : MonoBehaviour
{
    public List<GameObject> robots;

    public void ActivateRobot(int roboIndex)
    {
        DisableAllRobots();
        if(roboIndex != 0)
            robots[roboIndex].SetActive(true);
    }

    private void DisableAllRobots()
    {
        foreach(var r in robots)
        {
            r.SetActive(false);
        }
    }
}
