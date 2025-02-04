using System.IO; // For file reading
using System.Linq;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public string fileName = "data.txt"; // Name of the file in the project folder
    public Transform parent; // Optional: Assign a parent to the created objects

    // Start is called before the first frame update
    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, "Resources/"+fileName); // File path in the project folder

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                CreateObjectFromLine(line);
            }
        }
        else
        {
            Debug.LogError($"File not found: {filePath}");
        }
    }

    void CreateObjectFromLine(string line)
    {
        // Assuming each line contains "ObjectType x y z"
        string[] parts = line.Split(' ');

        if (parts.Length >= 4)
        {
            string objectType = parts[0];
            float x = float.Parse(parts[1]);
            float y = float.Parse(parts[2]);
            float z = float.Parse(parts[3]);
            Vector3 position = new Vector3(x, y, z);

            GameObject prefab = GetPrefabByName(objectType);

            if (prefab != null)
            {
                GameObject newObject = Instantiate(prefab, position, Quaternion.identity);
                if (parent != null)
                {
                    newObject.transform.parent = parent; // Set parent if assigned
                }
            }
            else
            {
                Debug.LogWarning($"Prefab not found for type: {objectType}");
            }
        }
        else
        {
            Debug.LogError($"Invalid line format: {line}");
        }
    }

    GameObject GetPrefabByName(string name)
    {
        // Find a prefab by name from the Resources folder or other means
        // You need to implement a way to load or match prefab objects.
        return Resources.Load<GameObject>("Prefab/" + name); // Assumes prefab is in a "Resources" folder
    }
}
