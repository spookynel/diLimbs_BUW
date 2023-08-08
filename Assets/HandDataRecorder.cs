using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

public class HandDataRecorder : MonoBehaviour
{
    private const string SaveFolder = @"D:\Usuarios\kerke\Escritorio\DiLimbs\";

    private List<HandData> handDataList;
    private int frameCount;

    private void Awake()
    {
        handDataList = new List<HandData>();
        frameCount = 0;
    }

    private void Update()
    {
        if (frameCount % 10 == 0)
        {
            RetrieveHandData(transform);
            LogFrameCount();
        }

        frameCount++;
    }

    private void RetrieveHandData(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            RecursiveRetrieveData(child, child.name);
        }
    }

    private void RecursiveRetrieveData(Transform currentTransform, string fingerName)
    {
        var handData = new HandData
        {
            Frame = frameCount,
            FingerName = fingerName,
            Position = currentTransform.position,
            Rotation = currentTransform.rotation
        };
        handDataList.Add(handData);

        foreach (Transform child in currentTransform)
        {
            RecursiveRetrieveData(child, child.name);
        }
    }

    private void LogFrameCount()
    {
        Debug.Log("Current Frame: " + frameCount);
    }

    private void OnDisable()
    {
        SaveHandData();
    }

    private void SaveHandData()
    {
        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "NewHandCousine_" + timeStamp + ".csv";
        string filePath = Path.Combine(SaveFolder, fileName);

        StringBuilder csvContent = new StringBuilder();
        csvContent.AppendLine("Frame,FingerName,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW");

        foreach (var handData in handDataList)
        {
            string positionX = handData.Position.x.ToString("0.0000", CultureInfo.InvariantCulture);
            string positionY = handData.Position.y.ToString("0.0000", CultureInfo.InvariantCulture);
            string positionZ = handData.Position.z.ToString("0.0000", CultureInfo.InvariantCulture);
            string rotationX = handData.Rotation.x.ToString("0.0000", CultureInfo.InvariantCulture);
            string rotationY = handData.Rotation.y.ToString("0.0000", CultureInfo.InvariantCulture);
            string rotationZ = handData.Rotation.z.ToString("0.0000", CultureInfo.InvariantCulture);
            string rotationW = handData.Rotation.w.ToString("0.0000", CultureInfo.InvariantCulture);

            csvContent.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                handData.Frame,
                handData.FingerName,
                positionX,
                positionY,
                positionZ,
                rotationX,
                rotationY,
                rotationZ,
                rotationW));
        }

        File.WriteAllText(filePath, csvContent.ToString());

        Debug.Log("File saved successfully at: " + filePath);
    }
}

public struct HandData
{
    public int Frame;
    public string FingerName;
    public Vector3 Position;
    public Quaternion Rotation;
}
