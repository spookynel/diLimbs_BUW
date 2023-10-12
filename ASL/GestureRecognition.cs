using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GestureRecognition : MonoBehaviour
{
    public string gestureDataPath = @"C:\Users\anabe\My project\Assets\ASL_avg";
    public float similarityThreshold = 0.9f;
    public int recognitionInterval = 30;

    private Dictionary<string, List<float[]>> gestureData = new Dictionary<string, List<float[]>>();
    private int frameCount = 0;
    private List<float[]> currentGestureData = new List<float[]>();
    private string hierarchyName;

    private string[] fingerHierarchy = new string[]
    {
            "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger1Metacarpal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger1Metacarpal/RightFinger1Proximal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger1Metacarpal/RightFinger1Proximal/RightFinger1Distal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger1Metacarpal/RightFinger1Proximal/RightFinger1Distal/RightFinger1Tip",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger2Metacarpal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger2Metacarpal/RightFinger2Proximal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger2Metacarpal/RightFinger2Proximal/RightFinger2Medial",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger2Metacarpal/RightFinger2Proximal/RightFinger2Medial/RightFinger2Distal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger2Metacarpal/RightFinger2Proximal/RightFinger2Medial/RightFinger2Distal/RightFinger2Tip",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger3Metacarpal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger3Metacarpal/RightFinger3Proximal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger3Metacarpal/RightFinger3Proximal/RightFinger3Medial",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger3Metacarpal/RightFinger3Proximal/RightFinger3Medial/RightFinger3Distal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger3Metacarpal/RightFinger3Proximal/RightFinger3Medial/RightFinger3Distal/RightFinger3Tip",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger4Metacarpal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger4Metacarpal/RightFinger4Proximal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger4Metacarpal/RightFinger4Proximal/RightFinger4Medial",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger4Metacarpal/RightFinger4Proximal/RightFinger4Medial/RightFinger4Distal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger4Metacarpal/RightFinger4Proximal/RightFinger4Medial/RightFinger4Distal/RightFinger4Tip",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger5Metacarpal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger5Metacarpal/RightFinger5Proximal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger5Metacarpal/RightFinger5Proximal/RightFinger5Medial",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger5Metacarpal/RightFinger5Proximal/RightFinger5Medial/RightFinger5Distal",
    "Pooja/Pooja/Root/Hips/Spine1/Spine2/Spine3/Spine4/RightShoulder/RightArm/RightForeArm/RightHand/RightFinger5Metacarpal/RightFinger5Proximal/RightFinger5Medial/RightFinger5Distal/RightFinger5Tip"

    };

    void Start()
    {
        LoadGestureData();
        hierarchyName = GetHierarchyName(transform);
    }

    void Update()
    {
        frameCount++;

        if (frameCount % recognitionInterval == 0)
        {
            RecognizeGesture();
        }
    }

    void LoadGestureData()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(gestureDataPath);
        FileInfo[] files = dirInfo.GetFiles("*.csv");

        foreach (FileInfo file in files)
        {
            if (file.Extension.Equals(".csv"))
            {
                string gestureName = Path.GetFileNameWithoutExtension(file.Name);
                string[] fileNameParts = gestureName.Split('_');
                string key = fileNameParts[0]; // Extract the gesture name

                List<float[]> gestureFrames = new List<float[]>();

                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    // Skip the header line
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(',');

                        if (values.Length < 8)
                        {
                            Debug.LogError($"Invalid line format in file {file.Name}: {line}");
                            continue;
                        }

                        float[] frameData = new float[7];
                        for (int i = 0; i < 7; i++)
                        {
                            if (float.TryParse(values[i + 1], out float value))
                            {
                                frameData[i] = value;
                            }
                            else
                            {
                                Debug.LogError($"Error parsing value at index {i + 1} in file {file.Name}");
                                break;
                            }
                        }

                        gestureFrames.Add(frameData);
                    }
                }

                gestureData[key] = gestureFrames;
            }
        }
    }

    void RecognizeGesture()
    {
        currentGestureData = ExtractGestureData();
        Debug.Log("Hierarchy Name: " + hierarchyName);
        Debug.Log("Game Object's Children:");
        PrintChildrenRecursive(transform);

        Debug.Log("Available Gesture Keys:");
        foreach (string key in gestureData.Keys)
        {
            Debug.Log(key);
        }

        float minDistance = float.MaxValue;
        string recognizedGesture = "";

        foreach (KeyValuePair<string, List<float[]>> gesture in gestureData)
        {
            List<float[]> referenceGestureData = gesture.Value;
            float distance = CalculateDTWDistance(currentGestureData, referenceGestureData);

            if (distance < minDistance)
            {
                minDistance = distance;
                recognizedGesture = gesture.Key;
            }
        }

        if (minDistance <= similarityThreshold)
        {
            Debug.Log("Closest Recognized Gesture: " + recognizedGesture);
        }
        else
        {
            Debug.Log("No gesture recognized.");
        }
    }

    List<float[]> ExtractGestureData()
    {
        List<float[]> gestureFrames = new List<float[]>();

        foreach (string fingerPath in fingerHierarchy)
        {
            Transform fingerTransform = transform.Find(fingerPath);
            if (fingerTransform != null)
            {
                float[] frameData = new float[7];
                frameData[0] = fingerTransform.position.x;
                frameData[1] = fingerTransform.position.y;
                frameData[2] = fingerTransform.position.z;
                frameData[3] = fingerTransform.rotation.x;
                frameData[4] = fingerTransform.rotation.y;
                frameData[5] = fingerTransform.rotation.z;
                frameData[6] = fingerTransform.rotation.w;

                gestureFrames.Add(frameData);
            }
            else
            {
                Debug.LogWarning("Finger transform not found: " + fingerPath);
            }
        }

        return gestureFrames;
    }

    float CalculateDTWDistance(List<float[]> referenceFrames, List<float[]> testFrames)
    {
        int n = referenceFrames.Count;
        int m = testFrames.Count;
        float[,] dtw = new float[n + 1, m + 1];

        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= m; j++)
            {
                dtw[i, j] = float.MaxValue;
            }
        }

        dtw[0, 0] = 0;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                float cost = CalculateDistance(referenceFrames[i - 1], testFrames[j - 1]);
                dtw[i, j] = cost + Mathf.Min(dtw[i - 1, j], dtw[i, j - 1], dtw[i - 1, j - 1]);
            }
        }

        return dtw[n, m];
    }

    float CalculateDistance(float[] frame1, float[] frame2)
    {
        float distance = 0;

        for (int i = 0; i < 7; i++)
        {
            distance += Mathf.Pow(frame1[i] - frame2[i], 2);
        }

        return Mathf.Sqrt(distance);
    }

    string GetHierarchyName(Transform obj)
    {
        string name = obj.name;
        Transform parent = obj.parent;

        while (parent != null && parent != transform)
        {
            name = parent.name + "-" + name;
            parent = parent.parent;
        }

        return name;
    }

    void PrintChildrenRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Debug.Log(child.name + ": " + child.position + " " + child.rotation);
            PrintChildrenRecursive(child);
        }
    }
}
