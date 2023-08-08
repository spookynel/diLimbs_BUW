using System.Collections.Generic;
using UnityEngine;

public class AngularDataCollector : MonoBehaviour
{
    private List<SensorOrientation> sensorOrientationData = new List<SensorOrientation>();
    private bool isRecording = true; // Flag to indicate if recording is in progress
    private Transform torsoReference; // Reference transform for the torso frame of reference
    private float captureInterval = 1.0f; // Capture interval in seconds
    private float nextCaptureTime = 0.0f; // Time of the next capture

    // Start is called before the first frame update
    void Start()
    {
        // Assign the torso reference transform (e.g., manually position and orient it in the Unity editor)
        torsoReference = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording && Time.time >= nextCaptureTime)
        {
            // Capture sensor orientation data and frame information starting from the root transform
            CaptureSensorOrientationData(transform);

            // Calculate the time of the next capture
            nextCaptureTime = Time.time + captureInterval;
        }
    }

    private void CaptureSensorOrientationData(Transform currentTransform)
    {
        // Get the sensor name
        string sensorName = currentTransform.name;

        // Get the sensor orientation relative to the torso reference
        Quaternion relativeRotation = Quaternion.Inverse(torsoReference.rotation) * currentTransform.rotation;

        // Calculate the parameterized orientation using two angles (θ, ϕ)
        float theta = Mathf.Atan2(relativeRotation.y, relativeRotation.w);
        float phi = Mathf.Asin(-2.0f * (relativeRotation.x * relativeRotation.z - relativeRotation.y * relativeRotation.w));

        // Create a new SensorOrientation instance
        SensorOrientation sensorOrientation = new SensorOrientation
        {
            FrameIndex = Time.frameCount,
            SensorName = sensorName,
            Theta = theta,
            Phi = phi
        };

        // Add the sensor orientation data to the list
        sensorOrientationData.Add(sensorOrientation);

        // Recursively iterate through the child transforms
        foreach (Transform childTransform in currentTransform)
        {
            CaptureSensorOrientationData(childTransform);
        }
    }

    // Function to save the sensor orientation data to a file
    private void SaveSensorOrientationDataToFile(string filePath)
    {
        // Convert the sensorOrientationData list to CSV format
        string csvData = ConvertSensorOrientationDataToCSV();

        // Save the CSV data to a file at the specified path
        System.IO.File.WriteAllText(filePath, csvData);
    }

    private string ConvertSensorOrientationDataToCSV()
    {
        string csvData = "SensorName,FrameIndex,Theta,Phi\n";

        // Iterate through each SensorOrientation instance
        foreach (SensorOrientation sensorOrientation in sensorOrientationData)
        {
            // Add a line to the CSV data with the sensor name, frame index, and parameterized orientation values
            csvData += $"{sensorOrientation.SensorName};{sensorOrientation.FrameIndex};{sensorOrientation.Theta};{sensorOrientation.Phi}\n";
        }

        return csvData;
    }

    private void OnDisable()
    {
        StopRecordingAndSave();
    }

    private void OnApplicationQuit()
    {
        StopRecordingAndSave();
    }

    // Function to stop recording and save the sensor orientation data
    public void StopRecordingAndSave()
    {
        if (!isRecording) return;

        isRecording = false;

        // Generate a file name based on the highest parent of the game object
        string fileName = transform.root.name + ".csv";

        // Specify the file path
        string filePath = @"D:\Usuarios\kerke\Escritorio\DiLimbs\" + fileName;

        // Save the sensor orientation data to a file
        SaveSensorOrientationDataToFile(filePath);
    }
}

// Class to store sensor orientation data
public class SensorOrientation
{
    public int FrameIndex;
    public string SensorName;
    public float Theta;
    public float Phi;
}
