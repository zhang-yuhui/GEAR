using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Helper;
using Unity.Mathematics;
public class MoleculeSpawner : MonoBehaviour
{
    public string fileName = "pentane";
    public Transform parent;
    private Dictionary<int, SortedList<float, Data>> database;
    public Dictionary<int, SortedList<float, Data>> GetDatabase(){
        return database;
    }
    private MetaData metaData;
    public MetaData GetMetaData(){
        return metaData;
    }
    private SortedDictionary<float, HashSet<int>> spawnTable;
    private List<List<int>> neighborList;
    private float time = -1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float t = Time.time;
        TextAsset textAsset =Resources.Load<TextAsset>(fileName); // File path in the project folder

        if (textAsset != null)
        {
            string fileContent = textAsset.text;
            string[] lines = fileContent.Split('\n');
            database = new Dictionary<int, SortedList<float, Data>>();
            int count = 0;
            int currCount = 0;
            int n = -2;
            int time = 0;
            List<Data> data = new List<Data>();
            foreach(var line in lines){
                string raw = line.Trim();
                if(raw == "")
                    continue;
                if(n + 2 == currCount){
                    try{
                        n = int.Parse(raw);
                    }catch (System.Exception){
                        Debug.LogError($"in line{count}, wrong format for n {raw}");
                    }
                    currCount = 0;
                    if(data.Count != 0){
                        time ++;
                        foreach (var point in data)
                        {
                            if(database.ContainsKey(point.id)){
                                database[point.id].Add(point.time, point);
                            }else{
                                SortedList<float, Data> tmp = new SortedList<float, Data> {{point.time, point}};
                                database.Add(point.id, tmp);
                            }
                        }
                        data.Clear();
                    }
                }else if(currCount != 1){
                    string[] parts = raw.Split(new []{' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                    try{
                        data.Add(new Data(
                            new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])),
                            time,
                            int.Parse(parts[parts.Length - 1]),
                            parts[0]));
                    }catch(System.Exception){
                        Debug.LogError($"in line{count}, wrong format for data input: {raw}");
                    }
                }
                count++;
                currCount++;
            }
            Debug.Log($"Successfully finished reading the data in {Time.time - t}s, start spawning object...");
            spawnTable = new SortedDictionary<float, HashSet<int>>();
            foreach(var kvp in database){
                ////Debug.Log($"searching value{kvp.Value[0].time}");
                if(!spawnTable.ContainsKey(kvp.Value.Values[0].time)){
                    ////Debug.Log($"searching value{kvp.Key}");
                    spawnTable.Add(kvp.Value.Values[0].time, new HashSet<int>{kvp.Key});
                } else {
                    spawnTable[kvp.Value.Values[0].time].Add(kvp.Key);
                }
            }
            metaData.minDist = 1.0f;
            metaData.maxDist = 3.0f;
            metaData.maxPos = metaData.minPos = database.First().Value.Values[0].position;
            foreach(var list in database){
                foreach(var kvp in list.Value){
                    Vector3 tmp = kvp.Value.position;
                    metaData.maxPos = math.max(metaData.maxPos, tmp);
                    metaData.minPos = math.min(metaData.minPos, tmp);
                }
            }
            
            Debug.Log($"max:{metaData.maxPos}, min:{metaData.minPos}");
        }else{
            Debug.LogError($"File not found: {fileName}");
        }
    }
    
    void Update()
    {
        if(time == -1f)
            time = 0f;
        else
            time += Time.deltaTime;
        if(spawnTable.Count() == 0)
            return;
        if(time >= spawnTable.First().Key){
            var spownGroup = spawnTable.First().Value;
            spawnTable.Remove(spawnTable.First().Key);
            foreach(var id in spownGroup){
                SpawnObject(id);
            }
        }
    }

    void SpawnObject(int id){
        if(!database.ContainsKey(id)){
            Debug.LogError($"Does not found molecule id {id}");
            return;
        }
        GameObject prefab = Resources.Load<GameObject>(Path.Combine("Prefab","Molecule"));
        if (prefab == null)
        {
            Debug.LogError($"No prefab called Molecule");
            return;
        }
        var movement = database[id];
        GameObject moleculeObject = Instantiate(prefab,movement.Values[0].position, Quaternion.identity);
        if (parent != null){
            moleculeObject.transform.parent = parent;
            
        }
        Molecule molecule = moleculeObject.GetComponent<Molecule>();
        if (molecule != null)
        {
            molecule.setId(id);
            molecule.setType(movement.Values[0].type);
            molecule.setFileName(fileName);
            foreach(var data in movement){
                molecule.loadData(data.Value.position, data.Value.time);
            }
            
        } else {
            Debug.LogError("does not get moleculeObject");
        }
        
    }
    void CreatObject(string line, int id, string[] file){
        // Deal with multiple white spaces
        string[] parts = line.Split(new []{' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4)
        {
            Debug.LogError($"wrong input format!{line}");
            return;
        }
        GameObject prefab = Resources.Load<GameObject>(Path.Combine("Prefab","Molecule"));
        if (prefab == null)
        {
            Debug.LogError($"No prefab called Molecule");
            return;
        }

        Vector3 position = new Vector3(float.Parse(parts[1]),float.Parse(parts[2]),float.Parse(parts[3]));
        GameObject molecule = Instantiate(prefab,position, Quaternion.identity);
        if (parent != null)
        {
            molecule.transform.SetParent(transform, false);
        }
        Molecule moleculeObject = molecule.GetComponent<Molecule>();
        if (moleculeObject != null)
        {
            moleculeObject.setId(id);
            moleculeObject.setType(parts[0]);
            moleculeObject.setFileName(fileName);
            float time = 0.0f;
            while(id < file.Length){
                parts = file[id].Split(new []{' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                position = new Vector3(float.Parse(parts[1]),float.Parse(parts[2]),float.Parse(parts[3]));
                moleculeObject.loadData(position, time);
                time += 1.0f;
                ///id += n + 2;
            }
            
        } else {
            Debug.LogError("does not get moleculeObject");
        }
        ////Debug.Log($"object {id} created!");
    }
    //// Update is called once per frame
}
