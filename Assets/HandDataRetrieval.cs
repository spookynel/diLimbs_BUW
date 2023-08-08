using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class HandDataRetrieval : MonoBehaviour
{
    private string filePath;
    private bool isRecording = false;
    private List<Transform> fingerNodes = new List<Transform>();
    public int startFrame = 0;
    public int endFrame = 100;

    private void Start()
    {
        // Generate the file path using the node name and current date and time
        string nodeName = gameObject.name;
        string fileName = nodeName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        filePath = Path.Combine("D:\\Usuarios\\kerke\\Escritorio\\DiLimbs", fileName);

        // Automatically find and populate the finger nodes based on the hierarchy
        FindFingerNodes(transform);

        // Write the column names to the CSV file
        WriteColumnNames();
    }

    private void Update()
    {
        if (isRecording)
        {
            // Retrieve angular and position information for each finger
            foreach (Transform fingerNode in fingerNodes)
            {
                Vector3 relativePosition = fingerNode.position - transform.position;
                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * fingerNode.rotation;

                // Save the finger sensor data to the CSV file
                SaveFingerData(fingerNode.name, relativePosition, relativeRotation);
            }
        }
    }

    private void FindFingerNodes(Transform parent)
    {
        // Check if the current parent has any children
        if (parent.childCount > 0)
        {
            // Iterate through each child
            foreach (Transform child in parent)
            {
                // Add the current child to the list
                fingerNodes.Add(child);

                // Recursively search for finger nodes in the child's hierarchy
                FindFingerNodes(child);
            }
        }
    }

    private void WriteColumnNames()
    {
        // Create the column names string
        string columnNames = "Frame,FingerName,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW";

        // Write the column names to the CSV file
        File.WriteAllText(filePath, columnNames + Environment.NewLine);
    }

    private void SaveFingerData(string fingerName, Vector3 position, Quaternion rotation)
    {
        // Create the CSV data string
        string csvData = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
            Time.frameCount, fingerName, position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w);

        // Append the data to the CSV file
        File.AppendAllText(filePath, csvData + Environment.NewLine);
    }

    private void OnApplicationQuit()
    {
        if (isRecording)
        {
            // Stop recording and export the file
            isRecording = false;
            ExportFile();
        }
    }

    private void ExportFile()
    {
        // Copy the temporary file to the final location
        string finalFilePath = Path.Combine("D:\\Usuarios\\kerke\\Escritorio\\DiLimbs", Path.GetFileName(filePath));
        File.Copy(filePath, finalFilePath, true);

        // Delete the temporary file
        File.Delete(filePath);

        Debug.Log("File exported: " + finalFilePath);
    }

    // Called when entering or exiting the Play mode in Unity Editor
    private void OnEnable()
    {
        // Start recording when entering Play mode
        isRecording = true;
    }

    // Called when exiting the Play mode in Unity Editor
    private void OnDisable()
    {
        if (isRecording)
        {
            // Stop recording and export the file
            isRecording = false;
            ExportFile();
        }
    }
}
