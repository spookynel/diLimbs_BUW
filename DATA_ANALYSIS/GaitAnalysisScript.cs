using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class GaitAnalysisScript : MonoBehaviour
{
    private List<float> strideRightLengths = new List<float>();
    private List<float> strideLeftLengths = new List<float>();
    private List<float> strideRightTimes = new List<float>();
    private List<float> strideLeftTimes = new List<float>();
    private List<float> stepLengths = new List<float>();
    private List<float> legLengths = new List<float>();
    private List<float> rightFootGroundedStates = new List<float>();
    private List<float> leftFootGroundedStates = new List<float>();

    private float legLength;
    private float startTime;
    private string outputFileName;

    private Transform rightFoot;
    private Transform leftFoot;
    private Transform rightThigh;
    private Transform hips;
    private Transform rightToeTip;
    private Transform leftToeTip;

    private Vector3 prevRightFootPosition;
    private float prevTimestamp;

    private float maxVelocity;
    private float maxAcceleration;
    private float prevVelocity;
    private float acceleration;

    private bool firstFrame = true;

    void Start()
    {
        // Define the output file name based on the game object's name
        outputFileName = "D:/Usuarios/kerke/Escritorio/DiLimbs/DATA_ANALYSIS/csv_files/" + name + ".csv";

        // Record the start time for timestamping
        startTime = Time.time;

        // Find the right leg transforms
        rightFoot = transform.Find("P09/Root/Hips/RightThigh/RightShin/RightFoot");
        leftFoot = transform.Find("P09/Root/Hips/LeftThigh/LeftShin/LeftFoot");
        rightThigh = transform.Find("P09/Root/Hips/RightThigh");

        if (rightFoot != null && rightThigh != null)
        {
            legLength = Vector3.Distance(rightThigh.position, rightFoot.position);
        }
        else
        {
            Debug.LogError("Could not find RightFoot or RightThigh node.");
        }

        // Find hips transform for velocity and acceleration
        hips = transform.Find("P09/Root/Hips");

        if (hips == null)
        {
            Debug.LogError("Could not find Hips node.");
        }

        // Find toe tip transforms for raycasting
        rightToeTip = transform.Find("P09/Root/Hips/RightThigh/RightShin/RightFoot/RightToe/RightToeTip");
        leftToeTip = transform.Find("P09/Root/Hips/LeftThigh/LeftShin/LeftFoot/LeftToe/LeftToeTip");
    }

    void Update()
    {
        if (leftFoot != null && rightFoot != null)
        {
            // Check if the right foot is grounded
            bool isRightFootGrounded = IsFootGrounded(rightFoot);

            // Check if the left foot is grounded
            bool isLeftFootGrounded = IsFootGrounded(leftFoot);

            // Store the grounded state for this frame
            rightFootGroundedStates.Add(isRightFootGrounded ? 1 : 0);
            leftFootGroundedStates.Add(isLeftFootGrounded ? 1 : 0);

            // Calculate stride lengths and times for the right and left legs
            CalculateStrideLengthAndTime(isRightFootGrounded, isLeftFootGrounded);

            // Calculate step length
            float stepLength = Vector3.Distance(rightFoot.position, leftFoot.position);
            stepLengths.Add(stepLength);

            // Calculate leg length
            legLengths.Add(legLength);

            // Calculate velocity and acceleration
            if (!firstFrame && strideRightTimes.Count > 1)
            {
                float velocity = strideRightLengths.Count > 1 ? strideRightLengths[strideRightLengths.Count - 1] / strideRightTimes[strideRightTimes.Count - 1] : 0f;
                acceleration = (velocity - prevVelocity) / strideRightTimes[strideRightTimes.Count - 1];
                if (acceleration > maxAcceleration)
                {
                    maxAcceleration = acceleration;
                }

                if (velocity > maxVelocity)
                {
                    maxVelocity = velocity;
                }
            }
            prevVelocity = strideRightLengths.Count > 1 ? strideRightLengths[strideRightLengths.Count - 1] / strideRightTimes[strideRightTimes.Count - 1] : 0f;
            firstFrame = false;
        }
    }


    bool IsFootGrounded(Transform foot)
    {
        RaycastHit hit;
        LayerMask groundLayer = LayerMask.GetMask("Ground"); // Assuming "Ground" is the layer of your "Plane" GameObject
        if (Physics.Raycast(foot.position, Vector3.down, out hit, 0.1f, groundLayer))
        {
            //Debug.Log("Foot grounded: " + foot.name);
            return true; // Foot is in contact with the ground
        }
        else
        {
            //Debug.Log("Foot not grounded: " + foot.name);
        }
        return false; // Foot is not in contact with the ground
    }

    void CalculateStrideLengthAndTime(bool isRightFootGrounded, bool isLeftFootGrounded)
    {
        if (isRightFootGrounded)
        {
            // Record the right foot position when it's grounded
            prevRightFootPosition = rightFoot.position;
        }
        else if (prevRightFootPosition != Vector3.zero)
        {
            // Calculate the stride length and time when the right foot is lifted
            float strideLength = Vector3.Distance(rightFoot.position, prevRightFootPosition);
            float timestamp = Time.time - startTime;
            float strideTime = firstFrame ? 0 : timestamp - prevTimestamp;
            strideRightLengths.Add(strideLength);
            strideRightTimes.Add(strideTime);
            prevTimestamp = timestamp;
            prevRightFootPosition = Vector3.zero;
        }

        if (isLeftFootGrounded)
        {
            // Record the left foot position when it's grounded
            prevRightFootPosition = leftFoot.position;
        }
        else if (prevRightFootPosition != Vector3.zero)
        {
            // Calculate the stride length and time when the left foot is lifted
            float strideLength = Vector3.Distance(leftFoot.position, prevRightFootPosition);
            float timestamp = Time.time - startTime;
            float strideTime = firstFrame ? 0 : timestamp - prevTimestamp;
            strideLeftLengths.Add(strideLength);
            strideLeftTimes.Add(strideTime);
            prevTimestamp = timestamp;
            prevRightFootPosition = Vector3.zero;
        }
    }

    void OnApplicationQuit()
    {
        // Save the data to a CSV file
        using (StreamWriter writer = new StreamWriter(outputFileName))
        {
            // Write column headers
            writer.WriteLine("Timestamp,LegLength,StrideRightLength,StrideLeftLength,StrideRightTime,StrideLeftTime,StepLength,RightFootGround,LeftFootGround");

            CultureInfo culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < strideRightLengths.Count; i++)
            {
                float timestamp = i * (1.0f / 30.0f); // 30 FPS frame rate
                writer.WriteLine($"{timestamp.ToString("F6", culture)},{legLengths[i].ToString("F6", culture)},{strideRightLengths[i].ToString("F6", culture)},{strideLeftLengths[i].ToString("F6", culture)},{strideRightTimes[i].ToString("F6", culture)},{strideLeftTimes[i].ToString("F6", culture)},{stepLengths[i].ToString("F6", culture)},{rightFootGroundedStates[i]},{leftFootGroundedStates[i]}");
            }
        }
    }
}
