using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

public class ASLDataCollector : MonoBehaviour
{
    private Animator animator;

    // The thumb has 4 joints, and the other fingers have 5 each.
    // Total joints per hand = 4 (for thumb) + 5 * 4 (for other fingers) = 24
    private Transform[] leftHandJoints = new Transform[24];
    private Transform[] rightHandJoints = new Transform[24];

    // The velocity and position arrays should also be updated to match the size
    private Vector3[] prevLeftHandPositions = new Vector3[24];
    private Vector3[] prevLeftHandVelocities = new Vector3[24];
    private Vector3[] prevRightHandPositions = new Vector3[24];
    private Vector3[] prevRightHandVelocities = new Vector3[24];


    private StreamWriter leftHandWriter;
    private StreamWriter rightHandWriter;
    private string leftFilePath;
    private string rightFilePath;
    private string filePath;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Assign left hand joints
        // Initialize left and right hand joints
        InitializeJoints(leftHandJoints, "Left");
        InitializeJoints(rightHandJoints, "Right");

        // Set the file path and create the file
        string directoryPath = @"D:\Usuarios\kerke\Escritorio\DiLimbs\Experiment2_ASL\output_files";
        leftFilePath = Path.Combine(directoryPath, gameObject.name + "_Left.csv");
        rightFilePath = Path.Combine(directoryPath, gameObject.name + "_Right.csv");

        leftHandWriter = new StreamWriter(leftFilePath, false, Encoding.UTF8);
        rightHandWriter = new StreamWriter(rightFilePath, false, Encoding.UTF8);
        leftHandWriter.WriteLine("Timestamp,Joint,PosX,PosY,PosZ,VelX,VelY,VelZ,AccelX,AccelY,AccelZ,JointAngle");
        rightHandWriter.WriteLine("Timestamp,Joint,PosX,PosY,PosZ,VelX,VelY,VelZ,AccelX,AccelY,AccelZ,JointAngle");


    }

    void Update()
    {
        // Update metrics for each joint in left and right hands
        UpdateHandMetrics(leftHandJoints, ref prevLeftHandPositions, ref prevLeftHandVelocities, "Left", leftHandWriter);
        UpdateHandMetrics(rightHandJoints, ref prevRightHandPositions, ref prevRightHandVelocities, "Right", rightHandWriter);

    }


    private void InitializeJoints(Transform[] handJoints, string handPrefix)
    {
        string basePath = $"P1/Root/Hips/Spine1/Spine2/Spine3/Spine4/{handPrefix}Shoulder/{handPrefix}Arm/{handPrefix}ForeArm/{handPrefix}Hand";

        int jointIndex = 0; // Start at the beginning of the array

        // Loop through each finger and find the joints
        for (int i = 1; i <= 5; i++)
        {
            
            string[] jointNames = i == 1 ? new string[] { "Metacarpal", "Proximal", "Distal", "Tip" } :
                                          new string[] { "Metacarpal", "Proximal", "Medial", "Distal", "Tip" };



            foreach (var jointName in jointNames)
            {
                if (jointIndex >= handJoints.Length)
                {
                    Debug.LogError($"Trying to access out of bounds index: {jointIndex}. Array length is {handJoints.Length}.");
                    return; // Exit the function to prevent any further out of bounds errors
                }

                string fullJointName = $"{handPrefix}Finger{i}{jointName}";
                Transform jointTransform = FindDeepChild(transform, fullJointName);

                if (jointTransform != null)
                {
                    handJoints[jointIndex++] = jointTransform; // Assign and then increment index
                    Debug.Log($"Found joint at {jointTransform} (UnityEngine.Transform), index is {jointIndex}");
                }
                else
                {
                    Debug.LogError($"Joint not found: {fullJointName}");
                    jointIndex++; // Increment index even if joint not found
                }
            }
        }
    }



    // Recursive method to find child with given name
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform found = FindDeepChild(child, name);
            if (found != null)
                return found;
        }
        return null;
    }


    private string GetJointName(int fingerIndex, int jointIndex)
    {
        string jointName = "";

        if (fingerIndex == 1) // Thumb
        {
            switch (jointIndex)
            {
                case 0: jointName = "Metacarpal"; break;
                case 1: jointName = "Proximal"; break;
                case 2: jointName = "Distal"; break;
                case 3: jointName = "Tip"; break;
            }
        }
        else
        {
            switch (jointIndex % 5)
            {
                case 0: jointName = "Metacarpal"; break;
                case 1: jointName = "Proximal"; break;
                case 2: jointName = "Medial"; break;
                case 3: jointName = "Distal"; break;
                case 4: jointName = "Tip"; break;
            }
        }

        return $"{jointName}";
    }



    private void UpdateHandMetrics(Transform[] handJoints, ref Vector3[] prevPositions, ref Vector3[] prevVelocities, string handPrefix, StreamWriter writer)
    {
        for (int i = 0; i < handJoints.Length; i++)
        {
            if (handJoints[i] == null)
                continue;

            // Calculate the finger index and joint name
            int fingerIndex, jointPositionIndex;
            if (i < 4) // Thumb has 4 joints
            {
                fingerIndex = 1;
                jointPositionIndex = i; // Direct mapping for thumb joints
            }
            else // Other fingers
            {
                fingerIndex = ((i - 4) / 5) + 2; // Adjusting index for fingers 2 to 5
                jointPositionIndex = (i - 4) % 5; // 5 joints per finger for fingers 2 to 5
            }

            Vector3 currentPosition = handJoints[i].position;
            Vector3 currentVelocity = (currentPosition - prevPositions[i]) / Time.deltaTime;
            Vector3 currentAcceleration = (currentVelocity - prevVelocities[i]) / Time.deltaTime;

            prevPositions[i] = currentPosition;
            prevVelocities[i] = currentVelocity;

            string jointName = GetJointName(fingerIndex, jointPositionIndex);
            float jointAngle = i > 0 ? CalculateJointAngle(handJoints, i - 1, i) : 0.0f;

            // Write data to CSV
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},{1}Finger{2}{3},{4:F2},{5:F2},{6:F2},{7:F2},{8:F2},{9:F2},{10:F2},{11:F2},{12:F2},{13:F2}",
                Time.time, handPrefix, fingerIndex, jointName, currentPosition.x, currentPosition.y, currentPosition.z,
                currentVelocity.x, currentVelocity.y, currentVelocity.z,
                currentAcceleration.x, currentAcceleration.y, currentAcceleration.z, jointAngle);

            writer.WriteLine(line);
        }
    }


    private float CalculateJointAngle(Transform[] handJoints, int jointIndexA, int jointIndexB)
    {
        if (jointIndexA < handJoints.Length && jointIndexB < handJoints.Length && handJoints[jointIndexA] != null && handJoints[jointIndexB] != null)
        {
            Vector3 vectorA = handJoints[jointIndexA].position - handJoints[jointIndexA].parent.position;
            Vector3 vectorB = handJoints[jointIndexB].position - handJoints[jointIndexA].position;
            return Vector3.Angle(vectorA, vectorB);
        }
        return 0.0f;
    }


    void OnDestroy()
    {
        if (leftHandWriter != null)
        {
            leftHandWriter.Close();
            Debug.Log($"Left hand output file saved successfully at: {leftFilePath}");
        }
        else
        {
            Debug.LogWarning("Left hand writer was not initialized, so no file was saved.");
        }

        if (rightHandWriter != null)
        {
            rightHandWriter.Close();
            Debug.Log($"Right hand output file saved successfully at: {rightFilePath}");
        }
        else
        {
            Debug.LogWarning("Right hand writer was not initialized, so no file was saved.");
        }
    }
}
