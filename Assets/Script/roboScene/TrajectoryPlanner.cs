using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Geometry;
using RosMessageTypes.NiryoMoveit;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrajectoryPlanner : MonoBehaviour
{

    // Hardcoded variables
    const int k_NumRobotJoints = 6;
    const float k_JointAssignmentWait = 0.1f;
    const float k_PoseAssignmentWait = 0.5f;

    // Variables required for ROS communication
    [SerializeField]
    string m_RosServiceName = "niryo_moveit";
    public string RosServiceName { get => m_RosServiceName; set => m_RosServiceName = value; }

    [SerializeField]
    GameObject m_NiryoOne;
    public GameObject NiryoOne { get => m_NiryoOne; set => m_NiryoOne = value; }
    [SerializeField]
    GameObject m_Target;
    public GameObject Target { get => m_Target; set => m_Target = value; }
    [SerializeField]
    GameObject m_TargetPlacementDefault;
    public GameObject TargetPlacement { get => m_TargetPlacementDefault; set => m_TargetPlacementDefault = value; }
    [SerializeField]
    GameObject m_CatAPlacement; 
    public GameObject CatAPlacement { get => m_CatAPlacement; set => m_CatAPlacement = value; }
    [SerializeField]
    GameObject m_CatBPlacement;
    public GameObject CatBPlacement { get => m_CatBPlacement; set => m_CatBPlacement = value; }
    [SerializeField]
    Queue<KeyValuePair<GameObject, Vector3>> targetsQueue; // keyvalue pair of target object and its placement position.
    [SerializeField]
    GameObject targetPrefab;
    [SerializeField]
    Button trajectoryGenerationButton;
    [SerializeField]
    bool IsTrajectoryMotionPending;
    [SerializeField]
    UnityEvent<string> trajectoryGenerationStatus;
    [SerializeField]
    Material highlightMaterial;
    // Assures that the gripper is always positioned above the m_Target cube before grasping.
    readonly Quaternion m_PickOrientation = Quaternion.Euler(90, 90, 0);
    readonly Vector3 m_PickPoseOffset = Vector3.up * 0.1f;
    //readonly Vector3 m_PickPoseOffset = Vector3.up * 0.07f;

    // Articulation Bodies
    ArticulationBody[] m_JointArticulationBodies;
    ArticulationBody m_LeftGripper;
    ArticulationBody m_RightGripper;

    // ROS Connector
    ROSConnection m_Ros;

    /// <summary>
    ///     Find all robot joints in Awake() and add them to the jointArticulationBodies array.
    ///     Find left and right finger joints and assign them to their respective articulation body objects.
    /// </summary>
    void Start()
    {
        Debug.Log("Trajectory planner is running...");
        IsTrajectoryMotionPending = false;
        targetsQueue = new();
        // Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterRosService<MoverServiceRequest, MoverServiceResponse>(m_RosServiceName);

        m_JointArticulationBodies = new ArticulationBody[k_NumRobotJoints];

        var linkName = string.Empty;
        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            linkName += SourceDestinationPublisher.LinkNames[i];
            m_JointArticulationBodies[i] = m_NiryoOne.transform.Find(linkName).GetComponent<ArticulationBody>();
        }

        // Find left and right fingers
        var rightGripper = linkName + "/tool_link/gripper_base/servo_head/control_rod_right/right_gripper";
        var leftGripper = linkName + "/tool_link/gripper_base/servo_head/control_rod_left/left_gripper";

        m_RightGripper = m_NiryoOne.transform.Find(rightGripper).GetComponent<ArticulationBody>();
        m_LeftGripper = m_NiryoOne.transform.Find(leftGripper).GetComponent<ArticulationBody>();
    }

    /// <summary>
    ///     Returns the offset value adjusted according to the height of the target object.
    ///     The returned value will place the gripper at <seealso cref="m_PickPoseOffset"/>
    ///     from the top of the target object.
    /// </summary>
    private Vector3 GetPickPoseOffset(GameObject target)
    {
        var y = target.GetComponent<MeshRenderer>().bounds.size.y;
        var new_pose = new Vector3(m_PickPoseOffset.x, m_PickPoseOffset.y + y/2f, m_PickPoseOffset.z);
        Debug.Log("height of object: " + y);
        Debug.Log("base pose: " + m_PickPoseOffset);
        Debug.Log("new pose: " + new_pose);
        return new_pose;
    }

    private void Update()
    {
        trajectoryGenerationButton.interactable = !IsTrajectoryMotionPending && targetsQueue.Count > 0 ;
    }

    /// <summary>
    ///     Close the gripper
    /// </summary>
    void CloseGripper()
    {
        var leftDrive = m_LeftGripper.xDrive;
        var rightDrive = m_RightGripper.xDrive;

        leftDrive.target = -0.01f;
        rightDrive.target = 0.01f;

        m_LeftGripper.xDrive = leftDrive;
        m_RightGripper.xDrive = rightDrive;
    }

    /// <summary>
    ///     Open the gripper
    /// </summary>
    void OpenGripper()
    {
        var leftDrive = m_LeftGripper.xDrive;
        var rightDrive = m_RightGripper.xDrive;

        leftDrive.target = 0.01f;
        rightDrive.target = -0.01f;

        m_LeftGripper.xDrive = leftDrive;
        m_RightGripper.xDrive = rightDrive;
    }

    /// <summary>
    ///     Get the current values of the robot's joint angles.
    /// </summary>
    /// <returns>NiryoMoveitJoints</returns>
    NiryoMoveitJointsMsg CurrentJointConfig()
    {
        var joints = new NiryoMoveitJointsMsg();

        for (var i = 0; i < k_NumRobotJoints; i++)
        {
            joints.joints[i] = m_JointArticulationBodies[i].jointPosition[0];
        }

        return joints;
    }

    private bool IsTargetValidForPick(GameObject target)
    {
        var size = target.GetComponent<MeshRenderer>().bounds.size;
        return size.x < 0.08f && size.z < 0.08f ;
    }

    

    public void HighlightObjPickValidity(GameObject target)
    {
        // incomplete method -- for adding and removing a green colored highlight material.
        var objMesh = target.transform.Find("default");
        var mr = objMesh.GetComponent<MeshRenderer>();
        var mrs = new List<Material>(mr.materials);
        
        if (IsTargetValidForPick(target))
        {
            mrs.Add(highlightMaterial);
        }
        else
        {
            mrs.Remove(highlightMaterial);
        }
    }

    public void AddTargetToQueue(GameObject target, TargetType.Type t)
    {
        Debug.Log("Add target type: " + "helloooo");
        switch (t)
        {
            case TargetType.Type.CatA:
                Debug.Log("Add target type: " + "CatA");
                AddTargetToQueue(target, CatAPlacement.transform.position);
                break;
            case TargetType.Type.CatB:
                Debug.Log("Add target type: " + "CatB");
                AddTargetToQueue(target, CatBPlacement.transform.position);
                break;
        }
    }

    public void AddTargetToQueue(GameObject target, Vector3 target_placement_pos)
    {
        Debug.Log("Adding target to queue ...");
        targetsQueue.Enqueue(new KeyValuePair<GameObject, Vector3>(target, target_placement_pos));
    }

    public void AddTargetToQueue(GameObject target)
    {
        AddTargetToQueue(target, CatAPlacement.transform.position);
    }

    void SpawnTarget(Vector3 target_placement_pos)
    {
        var tar = Instantiate(targetPrefab);
        AddTargetToQueue(tar, target_placement_pos);
    }

    public void SpawnFruit()
    {
        SpawnTarget(CatAPlacement.transform.position);
    }

    public void SpawnVeggie()
    {
        SpawnTarget(CatBPlacement.transform.position);
    }

    public void PublishJointsForFirstTargetInQueue()
    {
        Debug.Log("targetsQueue.Count: " + targetsQueue.Count);
        if (targetsQueue.Count > 0 && !IsTrajectoryMotionPending)
        {
            var el = targetsQueue.Peek();
            PublishJoints(el.Key, el.Value);
            //if (el.Value.GetType().FullName == "System.Numerics.Vector3")
            //{
                
            //}
            //else if (el.Value == TargetType.Fruit)
            //{
            //    PublishJoints(targetsQueue.Peek().Key, FruitPlacement);
            //}
            //else if(el.Value == TargetType.Veggie)
            //{
            //    PublishJoints(targetsQueue.Peek().Key, VeggiePlacement);
            //}
        }
        else
        {
            Debug.Log("Target queue is empty");
        }
    }

    private void PublishJoints(GameObject target, Vector3 targetPosition)
    {
        var request = new MoverServiceRequest();
        request.joints_input = CurrentJointConfig();

        var currentPickPoseOffset = GetPickPoseOffset(target);
        // Pick Pose
        request.pick_pose = new PoseMsg
        {
            position = (target.transform.position + currentPickPoseOffset).To<FLU>(),

            // The hardcoded x/z angles assure that the gripper is always positioned above the target cube before grasping.
            orientation = Quaternion.Euler(90, target.transform.eulerAngles.y, 0).To<FLU>()
        };

        // Place Pose
        request.place_pose = new PoseMsg
        {
            position = (targetPosition + currentPickPoseOffset).To<FLU>(),
            orientation = m_PickOrientation.To<FLU>()
        };

        IsTrajectoryMotionPending = true;
        m_Ros.SendServiceMessage<MoverServiceResponse>(m_RosServiceName, request, TrajectoryResponse);
    }

    private void PublishJoints(GameObject target, GameObject targetPlacement)
    {
        var request = new MoverServiceRequest();
        request.joints_input = CurrentJointConfig();

        var currentPickPoseOffset = GetPickPoseOffset(target);
        // Pick Pose
        request.pick_pose = new PoseMsg
        {
            position = (target.transform.position + currentPickPoseOffset).To<FLU>(),

            // The hardcoded x/z angles assure that the gripper is always positioned above the target cube before grasping.
            orientation = Quaternion.Euler(90, target.transform.eulerAngles.y, 0).To<FLU>()
        };

        // Place Pose
        request.place_pose = new PoseMsg
        {
            position = (targetPlacement.transform.position + currentPickPoseOffset).To<FLU>(),
            orientation = m_PickOrientation.To<FLU>()
        };

        IsTrajectoryMotionPending = true;
        m_Ros.SendServiceMessage<MoverServiceResponse>(m_RosServiceName, request, TrajectoryResponse);
    }


    /// <summary>
    ///     Create a new MoverServiceRequest with the current values of the robot's joint angles,
    ///     the target cube's current position and rotation, and the targetPlacement position and rotation.
    ///     Call the MoverService using the ROSConnection and if a trajectory is successfully planned,
    ///     execute the trajectories in a coroutine.
    /// </summary>
    public void PublishJoints()
    {
        var request = new MoverServiceRequest();
        request.joints_input = CurrentJointConfig();

        // Pick Pose
        request.pick_pose = new PoseMsg
        {
            position = (m_Target.transform.position + m_PickPoseOffset).To<FLU>(),

            // The hardcoded x/z angles assure that the gripper is always positioned above the target cube before grasping.
            orientation = Quaternion.Euler(90, m_Target.transform.eulerAngles.y, 0).To<FLU>()
        };

        // Place Pose
        request.place_pose = new PoseMsg
        {
            position = (m_TargetPlacementDefault.transform.position + m_PickPoseOffset).To<FLU>(),
            orientation = m_PickOrientation.To<FLU>()
        };

        trajectoryGenerationStatus.Invoke("Communicating with ROS service ...");

        m_Ros.SendServiceMessage<MoverServiceResponse>(m_RosServiceName, request, TrajectoryResponse);
    }

    void TrajectoryResponse(MoverServiceResponse response)
    {
        trajectoryGenerationStatus.Invoke("Motion planning ongoing ...");
        if (response.trajectories.Length > 0)
        {
            Debug.Log("Trajectory returned.");
            targetsQueue.Dequeue();
            StartCoroutine(ExecuteTrajectories(response));
        }
        else
        {
            trajectoryGenerationStatus.Invoke("Motion not possible with current position of object ... \n Pick & place the object by hand ...");
            targetsQueue.Dequeue();
            Debug.LogError("No trajectory returned from MoverService.");
            IsTrajectoryMotionPending = false;
        }
    }

    /// <summary>
    ///     Execute the returned trajectories from the MoverService.
    ///     The expectation is that the MoverService will return four trajectory plans,
    ///     PreGrasp, Grasp, PickUp, and Place,
    ///     where each plan is an array of robot poses. A robot pose is the joint angle values
    ///     of the six robot joints.
    ///     Executing a single trajectory will iterate through every robot pose in the array while updating the
    ///     joint values on the robot.
    /// </summary>
    /// <param name="response"> MoverServiceResponse received from niryo_moveit mover service running in ROS</param>
    /// <returns></returns>
    IEnumerator ExecuteTrajectories(MoverServiceResponse response)
    {
        if (response.trajectories != null)
        {
            trajectoryGenerationStatus.Invoke("Executing the trajectories ...");
            IsTrajectoryMotionPending = true; // make it true after the motion is complete

            // For every trajectory plan returned
            for (var poseIndex = 0; poseIndex < response.trajectories.Length; poseIndex++)
            {
                // For every robot pose in trajectory plan
                foreach (var t in response.trajectories[poseIndex].joint_trajectory.points)
                {
                    var jointPositions = t.positions;
                    var result = jointPositions.Select(r => (float)r * Mathf.Rad2Deg).ToArray();

                    // Set the joint values for every joint
                    for (var joint = 0; joint < m_JointArticulationBodies.Length; joint++)
                    {
                        var joint1XDrive = m_JointArticulationBodies[joint].xDrive;
                        joint1XDrive.target = result[joint];
                        m_JointArticulationBodies[joint].xDrive = joint1XDrive;
                    }

                    // Wait for robot to achieve pose for all joint assignments
                    yield return new WaitForSeconds(k_JointAssignmentWait);
                }

                // Close the gripper if completed executing the trajectory for the Grasp pose
                if (poseIndex == (int)Poses.Grasp)
                {
                    CloseGripper();
                }

                // Wait for the robot to achieve the final pose from joint assignment
                yield return new WaitForSeconds(k_PoseAssignmentWait);
            }

            // All trajectories have been executed, open the gripper to place the target cube
            OpenGripper();
            IsTrajectoryMotionPending = false;
            trajectoryGenerationStatus.Invoke("Trajectories execution successful ...");
        }

        else
        {
            IsTrajectoryMotionPending = false;
        }
    }

    enum Poses
    {
        PreGrasp,
        Grasp,
        PickUp,
        Place
    }
}
