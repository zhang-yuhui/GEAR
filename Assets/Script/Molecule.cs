using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Helper;
public class Molecule : MonoBehaviour
{
    
    public struct DataSimple
    {
        public Vector3 position;
        public float time;
        
    }
    private int id = -1;
    private string type = "" ;
    private Renderer objectRenderer;
    private List<DataSimple> data;
    private Dictionary<int, SortedList<float, Data>> database;
    private MetaData metaData;
    private List<List<int>> neighborList;
    private float totalTime = 0f;
    private float elapsedTime = 0f;
    private string fileName = "CO2.txt";
    private int currentStep = 0;
    public float endTime = 1000000;
    public void setId(int x){
        if(x < 0){
            return;
        }
        id = x;
        ////Debug.Log($"setting id to {id}");
    }
    public void setType(string s){
        type = s;
    }
    public void setFileName(string s){
        fileName = s;
    }
    
    public void loadData(Vector3 position, float time){
        if(data == null || data.Count == 0)
            data = new List<DataSimple>();
        DataSimple tmp;
        tmp.position = position;
        tmp.time = time;
        data.Add(tmp);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(id < 0){
            Debug.LogError("Wrong id for this molecule!");
        }else if(type == ""){
            if(type == "X")
                Debug.Log("Loading X");
            Debug.LogWarning("no material, using default");
        }else{
            objectRenderer = GetComponent<Renderer>();
            string materialPath = Path.Combine("Material", type);
            Material material = Resources.Load<Material>(materialPath);
            if (material == null)
            {
                Debug.LogError($"no material called {type}");
            }else{
                objectRenderer.material = material;
            }
            if(data == null || data.Count == 0){
                Debug.LogError($"data not loaded for id {id}");
            }
            totalTime += data[0].time;
            MoleculeSpawner parent = GetComponentInParent<MoleculeSpawner>();
            if (parent != null)
            {
                database = parent.GetDatabase();
                metaData = parent.GetMetaData();
            }
            ////Debug.Log($"{data.Count} data loaded for id {id}");
            ////Debug.Log($"Molecule id{id} initialized");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(endTime < totalTime)
            return;
        totalTime += Time.deltaTime;
        if(currentStep >= data.Count - 1)
            return;
        if(totalTime >= data[currentStep + 1].time){
            currentStep ++;
        }
        if(currentStep == data.Count - 1){
            transform.position = data[currentStep].position;
            currentStep ++;
            Debug.Log($"Finished for molecule id{id}");
            return;
        }
        elapsedTime = totalTime - data[currentStep].time;
        float stepTime = data[currentStep + 1].time - data[currentStep].time;

        if(stepTime <= 0){ // No movement
            return; 
        }

        float progress = elapsedTime / stepTime;
        transform.position = Vector3.Lerp(data[currentStep].position, data[currentStep + 1].position, progress);
    }
}
