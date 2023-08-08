using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Linq;

public class AnimationDataLogger : MonoBehaviour
{
    public int startFrame;
    public int endFrame;

    private List<string[]> animationData;
    private string filePath;

    void Start()
    {
        // Set the file path using the current date and time stamp
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        filePath = Path.Combine("D:\\Usuarios\\kerke\\Escritorio\\DiLimbs", "NewRightHand_" + timeStamp + ".csv");

        // Initialize the animation data list
        animationData = new List<string[]>();

        // Log the file path
        Debug.Log("Output file path: " + filePath);
    }

    void Update()
    {
        // Check if the current frame is within the specified range
        if ((startFrame == 0 && endFrame == 0) || (Time.frameCount >= startFrame && Time.frameCount <= endFrame))
        {
            // Log the current frame
            Debug.Log("Processing frame: " + Time.frameCount);

            // Iterate through all descendants of the current game object recursively
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                // Skip the current game object (the script's attached node)
                if (child == transform)
                    continue;

                // Get the position and rotation relative to the script's attached node
                Vector3 relativePosition = transform.InverseTransformPoint(child.position);
                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * child.rotation;

                // Normalize the position and rotation values
                relativePosition = NormalizeVector(relativePosition);
                relativeRotation = NormalizeQuaternion(relativeRotation);

                // Create an array with the frame data
                string[] frameData = {
                    Time.frameCount.ToString(),
                    child.name,
                    relativePosition.x.ToString(CultureInfo.InvariantCulture),
                    relativePosition.y.ToString(CultureInfo.InvariantCulture),
                    relativePosition.z.ToString(CultureInfo.InvariantCulture),
                    relativeRotation.x.ToString(CultureInfo.InvariantCulture),
                    relativeRotation.y.ToString(CultureInfo.InvariantCulture),
                    relativeRotation.z.ToString(CultureInfo.InvariantCulture),
                    relativeRotation.w.ToString(CultureInfo.InvariantCulture)
                };

                // Add the frame data to the animation data list
                animationData.Add(frameData);
            }
        }
    }

    void OnApplicationQuit()
    {
        // Save the animation data to a CSV file
        SaveAnimationDataToFile();
    }

    void SaveAnimationDataToFile()
    {
        // Create a CSV file and write the animation data to it
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write the column names
            writer.WriteLine("Frame,FingerName,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW");

            // Write the frame data rows
            foreach (string[] frameData in animationData)
            {
                string line = string.Join(",", frameData);
                writer.WriteLine(line);
            }
        }

        // Log the save completion message
        Debug.Log("Animation data saved to: " + filePath);
    }

    Vector3 NormalizeVector(Vector3 vector)
    {
        // Normalize the vector relative to the script's attached node
        return vector - transform.localPosition;
    }

    Quaternion NormalizeQuaternion(Quaternion quaternion)
    {
        // Normalize the quaternion relative to the script's attached node
        Quaternion inverseRotation = Quaternion.Inverse(transform.localRotation);
        return inverseRotation * quaternion;
    }
}
