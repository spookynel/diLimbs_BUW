using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class GaitAnalysisScript : MonoBehaviour
{
    private List<float> strideLengths = new List<float>();
    private List<float> strideWidths = new List<float>();
    private List<float> stepLengths = new List<float>();
    private List<float> legLengths = new List<float>();
    private List<float> strideTimes = new List<float>();
    private List<float> velocitys = new List<float>();

    private float legLength;
    private float startTime;
    private string outputFileName;

    private Transform leftFoot;
    private Transform rightFoot;
    private Transform rightThigh;
    private Transform hips;
    private Transform rightToeTip;
    private Transform leftToeTip;
    private Transform leftThigh;
    private Transform rightShin;
    private Transform leftShin;
    private Transform rightShoulder;
    private Transform leftShoulder;
    private Transform head;

    private Vector3 prevRightFootPosition;
    //private float prevTimestamp;

    //private float maxVelocity;
    //private float maxAcceleration;
    private float prevVelocity;
    private float acceleration;

    private bool firstFrame = true;

    //private bool rightFootGrounded = true;
    //private bool leftFootGrounded = true;
    private List<int> rightFootGroundedStates = new List<int>();
    private List<int> leftFootGroundedStates = new List<int>();

    private Vector3 rightFootGroundedPosition; // Stores the position of the right foot when it first touches the ground
    private bool rightFootWasGrounded; // Keeps track of whether the right foot was grounded in the previous frame

    private List<Vector3> rightFootPositions = new List<Vector3>(); // List to store the positions of the right foot

    private List<Quaternion> rightFootRotations = new List<Quaternion>(); // List to store the rotations of the right foot---
    private List<Quaternion> rightThighRotations = new List<Quaternion>();
    private List<Quaternion> leftThighRotations = new List<Quaternion>();
    private List<Quaternion> righthShinRotations = new List<Quaternion>();
    private List<Quaternion> leftShinRotations = new List<Quaternion>();
    private List<Quaternion> rightShoulderRotations = new List<Quaternion>();
    private List<Quaternion> leftShoulderRotations = new List<Quaternion>();
    private List<Quaternion> headRotations = new List<Quaternion>();

    private bool prevRightFootGround;
    private Vector3 rightFootPrevPos;
    private float strideLength;
    private float startStrideTime;
    private float strideTime;

    private Rigidbody hipsRigidbody; // Add a Rigidbody reference for the "hips" node
    private Vector3 prevHipsPosition;
    private float hipsVelocityMagnitude; // Store the magnitude of the hips velocity



    void Start()
    {
        // Define the output file name based on the game object's name
        outputFileName = "D:/Usuarios/kerke/Escritorio/DiLimbs/DATA_ANALYSIS/csv_files/" + name + ".csv";

        // Record the start time for timestamping
        startTime = Time.time;
        startStrideTime = Time.time;
        strideTime = 0.0f;

        prevRightFootGround = false;
        strideLength = 0.0f;

        // Ensure both feet are grounded at the start
        //rightFootGrounded = true;
        //leftFootGrounded = true;

        // Find the right leg transforms
        rightFoot = transform.Find("P09/Root/Hips/RightThigh/RightShin/RightFoot");
        leftFoot = transform.Find("P09/Root/Hips/LeftThigh/LeftShin/LeftFoot");
        rightThigh = transform.Find("P09/Root/Hips/RightThigh");
        leftThigh = transform.Find("P09/Root/Hips/LeftThigh");
        rightShin = transform.Find("P09/Root/Hips/RightThigh/RightShin");
        leftShin = transform.Find("P09/Root/Hips/LeftThigh/LeftShin");
        rightShoulder = transform.Find("P09/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder");
        leftShoulder = transform.Find("P09/Root/Hips/Spine1/Spine2/Spine3/Spine4/LeftShoulder");
        head = transform.Find("P09/Root/Hips/Spine1/Spine2/Spine3/Spine4/Neck/Head");
        hips = transform.Find("P09/Root/Hips");


        if (rightFoot != null && rightThigh != null)
        {
            legLength = Vector3.Distance(rightThigh.position, rightFoot.position);
            rightFootPrevPos = rightFoot.position;
        }
        else
        {
            Debug.LogError("Could not find RightFoot or RightThigh node.");
        }

        // Find hips transform for velocity and acceleration       

        if (hips == null)
        {
            Debug.LogError("Could not find Hips node.");
        }
        else
        {
            // Get the Rigidbody component attached to the hips node
            hipsRigidbody = hips.GetComponent<Rigidbody>();

            if (hipsRigidbody == null)
            {
                Debug.LogError("Rigidbody component not found on the Hips node.");
            }

            prevHipsPosition = new Vector3(hips.position.x, 0.0f, hips.position.z); // Store the initial position on the horizontal plane
        }

        // Find toe tip transforms for raycasting
        //rightToeTip = transform.Find("P09/Root/Hips/RightThigh/RightShin/RightFoot/RightToe/RightToeTip");
        //leftToeTip = transform.Find("P09/Root/Hips/LeftThigh/LeftShin/LeftFoot/LeftToe/LeftToeTip");

    }

    void Update()
    {
        if (leftFoot != null && rightFoot != null && rightThigh != null && leftThigh != null && rightShin != null && leftShin != null && rightShoulder != null && leftShoulder != null && head != null)
        {

            // Check if the right foot is grounded
            bool isRightFootGrounded = IsFootGrounded(rightFoot);

            // Check if the left foot is grounded
            bool isLeftFootGrounded = IsFootGrounded(leftFoot);

            // Store the grounded state for this frame
            rightFootGroundedStates.Add(isRightFootGrounded ? 1 : 0);
            leftFootGroundedStates.Add(isLeftFootGrounded ? 1 : 0);

            // Store the current rotation of the right foot at each frame---
            rightFootRotations.Add(rightFoot.rotation);
            rightThighRotations.Add(rightThigh.rotation);
            leftThighRotations.Add(leftThigh.rotation);
            righthShinRotations.Add(rightShin.rotation);
            leftShinRotations.Add(leftShin.rotation);
            rightShoulderRotations.Add(rightShoulder.rotation);
            leftShoulderRotations.Add(leftShoulder.rotation);
            headRotations.Add(head.rotation);


            // Calculate stride length
            //float strideLength = Vector3.Distance(rightFoot.position, leftFoot.position);
            //strideLengths.Add(strideLength);
            //float currentStrideLength = Vector3.Distance(rightFoot.position, rightFootGroundedPosition);

            // Calculate stride time
            //float timestamp = Time.time - startTime;
            //float strideTime = firstFrame ? 0 : timestamp - prevTimestamp;
            //strideTimes.Add(strideTime);
            //prevTimestamp = timestamp;

            // Calculate step length
            float stepLength = Vector3.Distance(rightFoot.position, leftFoot.position);
            stepLengths.Add(stepLength);

            // Calculate leg length
            legLengths.Add(legLength);

            // Calculate stride width (lateral distance)
            float strideWidth = Mathf.Abs(rightFoot.position.x - leftFoot.position.x);
            strideWidths.Add(strideWidth);

           
            if (firstFrame)
            {
                prevRightFootGround = isRightFootGrounded ? true : false;
                rightFootPrevPos = rightFoot.position;
                strideTime = 0.0f;
                strideLength = 0.0f;
            }

            
            if (!firstFrame)
            {
                // Calculate velocity and acceleration
                /*float velocity = strideLength / strideTime;
                acceleration = (velocity - prevVelocity) / strideTime;
                if (acceleration > maxAcceleration)
                {
                    maxAcceleration = acceleration;
                }

                if (velocity > maxVelocity)
                {
                    maxVelocity = velocity;
                }
                */

                //Calculate stride length
                if (!prevRightFootGround && isRightFootGrounded) //Right foot was in the air and is now grounded
                {
                    strideLength = Vector3.Distance(rightFoot.position, rightFootPrevPos);
                    prevRightFootGround = true;
                    strideTime = Time.time - startStrideTime;
                    //Debug.Log("0 1");
                }
                if (prevRightFootGround && !isRightFootGrounded) //Right foot was grounded and is now in the air
                {
                    rightFootPrevPos = rightFoot.position;
                    //strideLength = Vector3.Distance(rightFoot.position, rightFootPrevPos);
                    prevRightFootGround = false;
                    startStrideTime = Time.time;
                    //Debug.Log("1 0");
                }
                if (prevRightFootGround && isRightFootGrounded) //Right foot was grounded and is still grounded
                {
                    //rightFootPrevPos = rightFoot.position;
                    prevRightFootGround = true;
                    //Debug.Log("1 1");
                }
                if (!prevRightFootGround && !isRightFootGrounded) //Right foot was in the air and is still in the air
                {
                    //strideLength = Vector3.Distance(rightFoot.position, rightFootPrevPos);
                    prevRightFootGround = false;
                    //Debug.Log("0 0");
                }

                
            }

            if (hipsRigidbody != null)
            {
                // Calculate the velocity of the "hips" node on the horizontal plane (X and Z axes)
                Vector3 currentHipsPosition = new Vector3(hips.position.x, 0.0f, hips.position.z); // Project position on the horizontal plane
                Vector3 hipsVelocity = (currentHipsPosition - prevHipsPosition) / Time.deltaTime;

                // Store the magnitude of the hips velocity
                hipsVelocityMagnitude = hipsVelocity.magnitude;

                // Store the previous position for the next frame
                prevHipsPosition = currentHipsPosition;

            }

            prevVelocity = strideLength / strideTime;
            firstFrame = false;
            //rightFootPositions.Add(rightFoot.position); // Store the current position of the right foot at each frame
            strideLengths.Add(strideLength);
            strideTimes.Add(strideTime);
            velocitys.Add(hipsVelocityMagnitude);
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

    void OnApplicationQuit()
    {
        int minCount = Mathf.Min(strideLengths.Count, rightFootGroundedStates.Count);

        using (StreamWriter writer = new StreamWriter(outputFileName))
        {
            writer.WriteLine("Timestamp,LegLength,StrideLength,StrideTime,StepLength,StrideWidth,StrideLengthRatio,StrideWidthRatio,StepLengthRatio,Velocity,RightFootGround,LeftFootGround," +
                /*"MaxVelocity,Acceleration,MaxAcceleration,VelocityRatio,AccelerationRatio," +*/
                /*"RightFootGround,LeftFootGround,RightFootX,RightFootY,RightFootZ," +*/
                "RightThighRotationX,RightThighRotationY,RightThighRotationZ,RightThighRotationW," +
                "LeftThighRotationX,LeftThighRotationY,LeftThighRotationZ,LeftThighRotationW," +
                "RightShinRotationX,RightShinRotationY,RightShinRotationZ,RightShinRotationW," +
                "LeftShinRotationX,LeftShinRotationY,LeftShinRotationZ,LeftShinRotationW," +
                "RightShoulderRotationX,RightShoulderRotationY,RightShoulderRotationZ,RightShoulderRotationW," +
                "LeftShoulderRotationX,LeftShoulderRotationY,LeftShoulderRotationZ,LeftShoulderRotationW," +
                "HeadRotationX,HeadRotationY,HeadRotationZ,HeadRotationW");

            CultureInfo culture = CultureInfo.InvariantCulture;

            for (int i = 0; i < minCount; i++)
            {
                float timestamp = i * (1.0f / 30.0f);
                Vector3 rightFootPosition = i < rightFootPositions.Count ? rightFootPositions[i] : Vector3.zero;
                Quaternion rightFootRotation = i < rightFootRotations.Count ? rightFootRotations[i] : Quaternion.identity;
                Quaternion rightThighRotation = i < rightThighRotations.Count ? rightThighRotations[i] : Quaternion.identity;
                Quaternion leftThighRotation = i < leftThighRotations.Count ? leftThighRotations[i] : Quaternion.identity;
                Quaternion rightShinRotation = i < righthShinRotations.Count ? righthShinRotations[i] : Quaternion.identity;
                Quaternion leftShinRotation = i < leftShinRotations.Count ? leftShinRotations[i] : Quaternion.identity;
                Quaternion rightShoulderRotation = i < rightShoulderRotations.Count ? rightShoulderRotations[i] : Quaternion.identity;
                Quaternion leftShoulderRotation = i < leftShoulderRotations.Count ? leftShoulderRotations[i] : Quaternion.identity;
                Quaternion headRotation = i < headRotations.Count ? headRotations[i] : Quaternion.identity;

                writer.WriteLine($"{timestamp.ToString("F6", culture)},{legLengths[i].ToString("F6", culture)},{strideLengths[i].ToString("F6", culture)},{strideTimes[i].ToString("F6", culture)},{stepLengths[i].ToString("F6", culture)},{strideWidths[i].ToString("F6", culture)},{(strideLengths[i] / legLengths[i]).ToString("F6", culture)},{(strideWidths[i] / legLengths[i]).ToString("F6", culture)},{(stepLengths[i] / legLengths[i]).ToString("F6", culture)}," +
                    $"{velocitys[i].ToString("F6", culture)}," +
                    /*$"{maxVelocity.ToString("F6", culture)},{acceleration.ToString("F6", culture)},{maxAcceleration.ToString("F6", culture)},{(maxVelocity / legLengths[i]).ToString("F6", culture)},{(maxAcceleration / legLengths[i]).ToString("F6", culture)}," +*/
                    $"{rightFootGroundedStates[i]},{leftFootGroundedStates[i]}," +
                    /*$"{rightFootPosition.x.ToString("F6", culture)},{rightFootPosition.y.ToString("F6", culture)},{rightFootPosition.z.ToString("F6", culture)}," +*/
                    $"{rightThighRotation.x.ToString("F6", culture)},{rightThighRotation.y.ToString("F6", culture)},{rightThighRotation.z.ToString("F6", culture)},{rightThighRotation.w.ToString("F6", culture)}," +
                    $"{leftThighRotation.x.ToString("F6", culture)},{leftThighRotation.y.ToString("F6", culture)},{leftThighRotation.z.ToString("F6", culture)},{leftThighRotation.w.ToString("F6", culture)}," +
                    $"{rightShinRotation.x.ToString("F6", culture)},{rightShinRotation.y.ToString("F6", culture)},{rightShinRotation.z.ToString("F6", culture)},{rightShinRotation.w.ToString("F6", culture)}," +
                    $"{leftShinRotation.x.ToString("F6", culture)},{leftShinRotation.y.ToString("F6", culture)},{leftShinRotation.z.ToString("F6", culture)},{leftShinRotation.w.ToString("F6", culture)}," +
                    $"{rightShoulderRotation.x.ToString("F6", culture)},{rightShoulderRotation.y.ToString("F6", culture)},{rightShoulderRotation.z.ToString("F6", culture)},{rightShoulderRotation.w.ToString("F6", culture)}," +
                    $"{leftShoulderRotation.x.ToString("F6", culture)},{leftShoulderRotation.y.ToString("F6", culture)},{leftShoulderRotation.z.ToString("F6", culture)},{leftShoulderRotation.w.ToString("F6", culture)}," +
                    $"{headRotation.x.ToString("F6", culture)},{headRotation.y.ToString("F6", culture)},{headRotation.z.ToString("F6", culture)},{headRotation.w.ToString("F6", culture)}");
            } 
        }
    }
}