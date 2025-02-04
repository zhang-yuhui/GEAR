using UnityEngine;
using Helper;
using System.Collections.Generic;
using UnityEditor;
public class DataImporter
{   
    public MoleculeData LoadData(string filename){
        MoleculeData moleculeData = new MoleculeData();
        moleculeData.atoms = new Dictionary<int, AtomData>();
        TextAsset textAsset = Resources.Load<TextAsset>(filename);
        if(textAsset == null){
            Debug.LogError($"File {filename} not found in Resources folder.");
            return moleculeData;
        }
        string fileContent = textAsset.text;
        string[] lines = fileContent.Split('\n');
        int count = 0;
        int currCount = 0;
        int n = -2;
        int time = -1; // to start from 0 as it will be incremented before use
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
                time++;
            }else if(currCount != 1){
                string[] parts = raw.Split(new []{' ', '\t'}, System.StringSplitOptions.RemoveEmptyEntries);
                try{
                    int id = int.Parse(parts[^1]);
                    if(moleculeData.atoms.ContainsKey(id)){
                        moleculeData.atoms[id].positions.Add(new PositionData{
                            time = time,
                            position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]))
                        });
                    }else{
                        AtomData atomData = new AtomData{
                            id = id,
                            type =MetaData.ElementToAtomicNumber(parts[0]),
                            positions = new List<PositionData>(){
                                new PositionData{
                                    time = time,
                                    position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]))
                                }
                            }
                        };
                        moleculeData.atoms.Add(id, atomData);
                    }
                }catch(System.Exception e){
                    Debug.LogError($"in line{count}, wrong format for data input: {raw}");
                    Debug.LogError($"error: {e.Message}");
                }
            }
            count++;
            currCount++;
            
        }
        return moleculeData;
    }
}
