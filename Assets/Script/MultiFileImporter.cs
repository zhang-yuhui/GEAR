using System.Collections.Generic;
using UnityEngine;
using Helper;
public class MultiFileImporter
{
    public MoleculeData LoadData(string foldername)
    {
        MoleculeData moleculeData = new MoleculeData();
        moleculeData.atoms = new Dictionary<int, AtomData>();
        TextAsset [] textAssets = Resources.LoadAll<TextAsset>(foldername);
        System.Array.Sort(textAssets, (a, b) => string.Compare(a.name, b.name));
        int count = 0;
        int currCount = 0;
        int n = -2;
        int time = -1; // to start from 0 as it will be incremented before use
        foreach(var textAsset in textAssets){
            // Copy directly from DataImporter.cs
            string fileContent = textAsset.text;
            string[] lines = fileContent.Split('\n');
            
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
                        PositionData tmpPosition = new (){
                                time = time,
                                position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]))
                            };

                        // If this is the first one, initialize the atomData list
                        if(!moleculeData.atoms.ContainsKey(id)){
                             AtomData atomData = new AtomData{
                                id = id,
                                type = MetaData.ElementToAtomicNumber(parts[0]),
                                positions = new()
                            };
                            moleculeData.atoms.Add(id, atomData);
                        }
                        // If this skips times, add empty position data to the list to align the time
                        while(moleculeData.atoms[id].positions.Count < time){
                            moleculeData.atoms[id].positions.Add(new (){time = -1});
                        }
                        moleculeData.atoms[id].positions.Add(tmpPosition);
                    }catch(System.Exception e){
                        Debug.LogError($"in line{count}, wrong format for data input: {raw}");
                        Debug.LogError($"error: {e.Message}");
                    }
                }
                count++;
                currCount++;
            }
        }
        moleculeData.frameCount = time;
        foreach(var atom in moleculeData.atoms){
            moleculeData.frameCount = atom.Value.positions.Count;
        }
        Debug.Log($"time: {time}, frame count: {moleculeData.frameCount}");
        return moleculeData;
    }
}
