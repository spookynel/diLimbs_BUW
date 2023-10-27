using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class GaitAnalysisScript : MonoBehaviour //TODO: Change directory to save the csv files.
{
    string nameObj;
    string participant;

    private List<float> strideLengthsRight = new List<float>();
    private List<float> strideTimesRight = new List<float>();
    private List<float> strideLengthsLeft = new List<float>();
    private List<float> strideTimesLeft = new List<float>();
    private List<float> strideWidths = new List<float>();
    private List<float> stepLengths = new List<float>();
    private List<float> legLengths = new List<float>();   
    private List<float> velocitys = new List<float>();
    private List<float> accelerations = new List<float>();

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
  
    private float acceleration;

    private bool firstFrame = true;

    private List<int> rightFootGroundedStates = new List<int>();
    private List<int> leftFootGroundedStates = new List<int>();

    // Lists to store joint rotations 
    private List<Quaternion> rightFootRotations = new List<Quaternion>(); 
    private List<Quaternion> rightThighRotations = new List<Quaternion>();
    private List<Quaternion> leftThighRotations = new List<Quaternion>();
    private List<Quaternion> righthShinRotations = new List<Quaternion>();
    private List<Quaternion> leftShinRotations = new List<Quaternion>();
    private List<Quaternion> rightShoulderRotations = new List<Quaternion>();
    private List<Quaternion> leftShoulderRotations = new List<Quaternion>();
    private List<Quaternion> headRotations = new List<Quaternion>();

    private bool prevRightFootGround;
    private Vector3 rightFootPrevPos;
    private float strideRightLength;
    private float startStrideRightTime;
    private float strideRightTime;

    private bool prevLeftFootGround;
    private Vector3 leftFootPrevPos;
    private float strideLeftLength;
    private float startStrideLeftTime;
    private float strideLeftTime;

    private Rigidbody hipsRigidbody; // Add a Rigidbody reference for the "hips" node
    private Vector3 prevHipsPosition;
    private Vector3 prevHipsVelocity;
    private float hipsVelocityMagnitude; // Store the magnitude of the hips velocity
    private float hipsAccelerationMagnitude; // Store the magnitude of the hips acceleration




    void Start()
    {
        //Get the name of the game object
        nameObj = gameObject.name;
        if (transform.childCount > 0)
        {
            participant = transform.GetChild(0).name;
        }

        // Define the output file name based on the game object's name
        outputFileName = "D:/Usuarios/kerke/Escritorio/DiLimbs/DATA_ANALYSIS/csv_files/" + name + ".csv";
        

        // Record the start time for timestamping
        startTime = Time.time;

        //Initialize times for right strides
        startStrideRightTime = Time.time;
        startStrideLeftTime = Time.time;
        strideRightTime = 0.0f;
        strideLeftTime = 0.0f;

        //Initialize times for left strides
        prevRightFootGround = false;
        prevLeftFootGround = false;
        strideRightLength = 0.0f;
        strideLeftLength = 0.0f;


        // Find the limbs transforms
        rightFoot = transform.Find(participant+ "/Root/Hips/RightThigh/RightShin/RightFoot");
        leftFoot = transform.Find(participant + "/Root/Hips/LeftThigh/LeftShin/LeftFoot");
        rightThigh = transform.Find(participant + "/Root/Hips/RightThigh");
        leftThigh = transform.Find(participant + "/Root/Hips/LeftThigh");
        rightShin = transform.Find(participant + "/Root/Hips/RightThigh/RightShin");
        leftShin = transform.Find(participant + "/Root/Hips/LeftThigh/LeftShin");
        rightShoulder = transform.Find(participant + "/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder");
        leftShoulder = transform.Find(participant + "/Root/Hips/Spine1/Spine2/Spine3/Spine4/LeftShoulder");
        head = transform.Find(participant + "/Root/Hips/Spine1/Spine2/Spine3/Spine4/Neck/Head");
        hips = transform.Find(participant + "/Root/Hips");

        // Find the leg length
        if (rightFoot != null && rightThigh != null && leftFoot != null)
        {
            legLength = Vector3.Distance(rightThigh.position, rightFoot.position);
            rightFootPrevPos = rightFoot.position;
            leftFootPrevPos = leftFoot.position;
        }
        else
        {
            Debug.LogError("Could not find RightFoot or RightThigh or LeftFoot node.");
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
                Debug.LogError("Rigidbody component not found on the Hips node."); // Add a Rigidbody component to the Hips node in the Unity Editor!
            }

            prevHipsPosition = new Vector3(hips.position.x, 0.0f, hips.position.z); // Store the initial position on the horizontal plane
            prevHipsVelocity = Vector3.zero; // Store the initial velocity
        }
       
    }

    void Update()
    {
        // Check if all the limb transforms have been found
        if (leftFoot != null && rightFoot != null && rightThigh != null && leftThigh != null && rightShin != null && leftShin != null && rightShoulder != null && leftShoulder != null && head != null)
        {

            // Check if the right foot is grounded
            bool isRightFootGrounded = IsFootGrounded(rightFoot);

            // Check if the left foot is grounded
            bool isLeftFootGrounded = IsFootGrounded(leftFoot);

            // Store the grounded state for this frame
            rightFootGroundedStates.Add(isRightFootGrounded ? 1 : 0);
            leftFootGroundedStates.Add(isLeftFootGrounded ? 1 : 0);

            // Store the current rotation of the joints at each frame
            rightFootRotations.Add(rightFoot.rotation);
            rightThighRotations.Add(rightThigh.rotation);
            leftThighRotations.Add(leftThigh.rotation);
            righthShinRotations.Add(rightShin.rotation);
            leftShinRotations.Add(leftShin.rotation);
            rightShoulderRotations.Add(rightShoulder.rotation);
            leftShoulderRotations.Add(leftShoulder.rotation);
            headRotations.Add(head.rotation);

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
                prevLeftFootGround = isLeftFootGrounded ? true : false;
                rightFootPrevPos = rightFoot.position;
                leftFootPrevPos = leftFoot.position;
                strideRightTime = 0.0f;
                strideLeftTime = 0.0f;
                strideRightLength = 0.0f;
                strideLeftLength = 0.0f;
            }
            
            if (!firstFrame)
            {
                //Calculate right stride length
                if (!prevRightFootGround && isRightFootGrounded) //Right foot was in the air and is now grounded
                {
                    strideRightLength = Vector3.Distance(rightFoot.position, rightFootPrevPos);
                    prevRightFootGround = true;
                    strideRightTime = Time.time - startStrideRightTime;
                }
                if (prevRightFootGround && !isRightFootGrounded) //Right foot was grounded and is now in the air
                {
                    rightFootPrevPos = rightFoot.position;
                    prevRightFootGround = false;
                    startStrideRightTime = Time.time;
                }
                if (prevRightFootGround && isRightFootGrounded) //Right foot was grounded and is still grounded
                {
                    prevRightFootGround = true;
                }
                if (!prevRightFootGround && !isRightFootGrounded) //Right foot was in the air and is still in the air
                {
                    prevRightFootGround = false;
                }

                //Calculate left stride length
                if (!prevLeftFootGround && isLeftFootGrounded) //Left foot was in the air and is now grounded
                {
                    strideLeftLength = Vector3.Distance(leftFoot.position, leftFootPrevPos);
                    prevLeftFootGround = true;
                    strideLeftTime = Time.time - startStrideLeftTime;
                }
                if (prevLeftFootGround && !isLeftFootGrounded) //Left foot was grounded and is now in the air
                {
                    leftFootPrevPos = leftFoot.position;
                    prevLeftFootGround = false;
                    startStrideLeftTime = Time.time;
                }
                if (prevLeftFootGround && isLeftFootGrounded) //Left foot was grounded and is still grounded
                {
                    prevLeftFootGround = true;
                }
                if (!prevLeftFootGround && !isLeftFootGrounded) //Left foot was in the air and is still in the air
                {
                    prevLeftFootGround = false;
                }


            }

            if (hipsRigidbody != null)
            {
                // Calculate the velocity of the "hips" node on the horizontal plane (X and Z axes)
                Vector3 currentHipsPosition = new Vector3(hips.position.x, 0.0f, hips.position.z); // Project position on the horizontal plane
                Vector3 hipsVelocity = (currentHipsPosition - prevHipsPosition) / Time.deltaTime;

                // Store the magnitude of the hips velocity
                hipsVelocityMagnitude = hipsVelocity.magnitude;                

                // Calculate acceleration
                Vector3 hipsAcceleration = (hipsVelocity - prevHipsVelocity) / Time.deltaTime;

                // Store the magnitude of the hips acceleration
                hipsAccelerationMagnitude = hipsAcceleration.magnitude;

                // Store the previous position and velocity for the next frame
                prevHipsPosition = currentHipsPosition;
                prevHipsVelocity = hipsVelocity;

            }
            // Add the stride lengths to the lists
            strideLengthsRight.Add(strideRightLength);
            strideLengthsLeft.Add(strideLeftLength);

            // Add the stride times to the lists
            strideTimesRight.Add(strideRightTime);
            strideTimesLeft.Add(strideLeftTime);

            // Add the velocity and acceleration to the lists
            velocitys.Add(hipsVelocityMagnitude);
            accelerations.Add(hipsAccelerationMagnitude);

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

    void OnApplicationQuit()
    {
        int minCount = Mathf.Min(strideLengthsRight.Count, rightFootGroundedStates.Count);

        using (StreamWriter writer = new StreamWriter(outputFileName))
        {
            writer.WriteLine("Timestamp,LegLength,StepLength,StepLengthRatio,StrideWidth,StrideWidthRatio,StrideLengthRight,StrideLengthRightRatio,StrideTimeRight,StrideLengthLeft,StrideLengthLeftRatio,StrideTimeLeft," +
                "Velocity,VelocityRatio,Acceleration,AccelerationRatio,RightFootGround,LeftFootGround," +
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
                Quaternion rightFootRotation = i < rightFootRotations.Count ? rightFootRotations[i] : Quaternion.identity;
                Quaternion rightThighRotation = i < rightThighRotations.Count ? rightThighRotations[i] : Quaternion.identity;
                Quaternion leftThighRotation = i < leftThighRotations.Count ? leftThighRotations[i] : Quaternion.identity;
                Quaternion rightShinRotation = i < righthShinRotations.Count ? righthShinRotations[i] : Quaternion.identity;
                Quaternion leftShinRotation = i < leftShinRotations.Count ? leftShinRotations[i] : Quaternion.identity;
                Quaternion rightShoulderRotation = i < rightShoulderRotations.Count ? rightShoulderRotations[i] : Quaternion.identity;
                Quaternion leftShoulderRotation = i < leftShoulderRotations.Count ? leftShoulderRotations[i] : Quaternion.identity;
                Quaternion headRotation = i < headRotations.Count ? headRotations[i] : Quaternion.identity;

                writer.WriteLine($"{timestamp.ToString("F6", culture)},{legLengths[i].ToString("F6", culture)},{stepLengths[i].ToString("F6", culture)},{(stepLengths[i] / legLengths[i]).ToString("F6", culture)},{strideWidths[i].ToString("F6", culture)},{(strideWidths[i] / legLengths[i]).ToString("F6", culture)},{strideLengthsRight[i].ToString("F6", culture)},{(strideLengthsRight[i] / legLengths[i]).ToString("F6", culture)},{strideTimesRight[i].ToString("F6", culture)},{strideLengthsLeft[i].ToString("F6", culture)},{(strideLengthsLeft[i] / legLengths[i]).ToString("F6", culture)},{strideTimesLeft[i].ToString("F6", culture)}," +
                    $"{velocitys[i].ToString("F6", culture)},{(velocitys[i] / legLengths[i]).ToString("F6", culture)},{accelerations[i].ToString("F6", culture)},{(accelerations[i] / legLengths[i]).ToString("F6", culture)}," +
                    $"{rightFootGroundedStates[i]},{leftFootGroundedStates[i]}," +
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