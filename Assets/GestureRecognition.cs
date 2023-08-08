using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GestureRecognition : MonoBehaviour
{
    public string gestureFolderPath = "C:/Users/anabe/My project/Assets/ASL_avg";
    public float recognitionThreshold = 0.8f;
    public int recognitionInterval = 30;

    private List<string> fingerNames;
    private List<float[]> referenceGestures;

    private int frameCount = 0;

    private void Start()
    {
        // Read the finger names from the first CSV file
        string firstGestureFile = Path.Combine(gestureFolderPath, "0_avg.csv");
        fingerNames = ReadFingerNames(firstGestureFile);

        // Load all reference gestures
        LoadReferenceGestures();
    }

    private void Update()
    {
        frameCount++;
        if (frameCount >= recognitionInterval)
        {
            frameCount = 0;

            // Get the current gesture vector from the live input
            float[] currentGesture = GetCurrentGesture();

            // Find the most similar reference gesture
            string recognizedGesture = RecognizeGesture(currentGesture);

            // Log the recognized gesture and similarity percentage
            Debug.Log("Recognized Gesture: " + recognizedGesture);

            if (recognizedGesture != "None")
            {
                float similarityPercentage = CompareGestures(currentGesture, GetReferenceGesture(recognizedGesture));
                Debug.Log("Similarity Percentage: " + similarityPercentage * 100 + "%");
            }
        }
    }

    private List<string> ReadFingerNames(string filePath)
    {
        List<string> names = new List<string>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(',');
                if (columns.Length >= 8 && columns[0] == "FingerName")
                {
                    for (int i = 1; i <= 24; i++)
                    {
                        Debug.Log("aaaa "+columns[i]);
                        names.Add(columns[i]);
                    }
                    break;
                }
            }
        }

        return names;
    }

    private void LoadReferenceGestures()
    {
        referenceGestures = new List<float[]>();

        foreach (string gestureName in fingerNames)
        {
            string gestureFilePath = Path.Combine(gestureFolderPath, gestureName + "_avg.csv");
            float[] gestureData = ReadGestureData(gestureFilePath);
            referenceGestures.Add(gestureData);
        }
    }

    private float[] ReadGestureData(string filePath)
    {
        List<float> data = new List<float>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(',');
                if (columns.Length >= 8 && columns[0] == "FingerName")
                {
                    for (int i = 1; i <= 24; i++)
                    {
                        float positionX = float.Parse(columns[i + 1]);
                        float positionY = float.Parse(columns[i + 2]);
                        float positionZ = float.Parse(columns[i + 3]);
                        float rotationX = float.Parse(columns[i + 4]);
                        float rotationY = float.Parse(columns[i + 5]);
                        float rotationZ = float.Parse(columns[i + 6]);
                        float rotationW = float.Parse(columns[i + 7]);

                        data.Add(positionX);
                        data.Add(positionY);
                        data.Add(positionZ);
                        data.Add(rotationX);
                        data.Add(rotationY);
                        data.Add(rotationZ);
                        data.Add(rotationW);

                        i += 7;
                    }
                    break;
                }
            }
        }

        return data.ToArray();
    }

    private float[] GetCurrentGesture()
    {
        List<float> data = new List<float>();

        foreach (string fingerName in fingerNames)
        {
            GameObject fingerObject = GameObject.Find(fingerName);
            if (fingerObject != null)
            {
                Transform fingerNode = fingerObject.transform;
                Quaternion rotation = fingerNode.rotation;
                Vector3 position = fingerNode.position;

                data.Add(position.x);
                data.Add(position.y);
                data.Add(position.z);
                data.Add(rotation.x);
                data.Add(rotation.y);
                data.Add(rotation.z);
                data.Add(rotation.w);
            }
        }

        return data.ToArray();
    }

    private string RecognizeGesture(float[] currentGesture)
    {
        float highestSimilarity = 0f;
        string recognizedGesture = "None";

        for (int i = 0; i < referenceGestures.Count; i++)
        {
            float[] referenceGesture = referenceGestures[i];
            float similarity = CompareGestures(currentGesture, referenceGesture);

            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                recognizedGesture = fingerNames[i];
            }
        }

        if (highestSimilarity < recognitionThreshold)
        {
            recognizedGesture = "None";
        }

        return recognizedGesture;
    }

    private float CompareGestures(float[] gestureA, float[] gestureB)
    {
        if (gestureA.Length != gestureB.Length)
        {
            Debug.LogError("Gesture dimension mismatch!");
            return 0f;
        }

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < gestureA.Length; i++)
        {
            dotProduct += gestureA[i] * gestureB[i];
            magnitudeA += gestureA[i] * gestureA[i];
            magnitudeB += gestureB[i] * gestureB[i];
        }

        magnitudeA = Mathf.Sqrt(magnitudeA);
        magnitudeB = Mathf.Sqrt(magnitudeB);

        if (magnitudeA == 0f || magnitudeB == 0f)
        {
            return 0f;
        }

        return dotProduct / (magnitudeA * magnitudeB);
    }

    private float[] GetReferenceGesture(string gestureName)
    {
        int index = fingerNames.IndexOf(gestureName);
        if (index != -1)
        {
            return referenceGestures[index];
        }

        return null;
    }
}
