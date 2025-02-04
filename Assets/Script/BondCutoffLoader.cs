using UnityEngine;
using Helper;
using System.Collections.Generic;

public class BondCutoffLoader
{
    public string fileName = "bondCutoff";
    public MetaData metaData;

    [System.Serializable]
    private class BondCutoffRaw
    {
        public string bond;
        public float cutoff;
        public float tolerance;
    }
    [System.Serializable]
    private class BondCutoffList
    {
        public BondCutoffRaw [] bondCutoffs;
    }
    public MetaData loadMetaData(string fileName)
    {
        fileName ??= this.fileName;
        MetaData metaData = new MetaData();

        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile == null)
        {
            Debug.LogError($"No file called {fileName}");
            return new MetaData();
        }
        
        BondCutoffList bondCutoffList = JsonUtility.FromJson<BondCutoffList>("{\"bondCutoffs\":" + jsonFile.text + "}");
        
        if (bondCutoffList.bondCutoffs == null){
            Debug.LogError($"No bond cutoff list found in {fileName}");
            return new MetaData();
        }
        var bondCutoffs = bondCutoffList.bondCutoffs;

        // handles default value
        if(bondCutoffs[0].bond != "default"){
            Debug.LogWarning($"No default bond cutoff found in {fileName}, default value will be used, default value must be in the first line");
        } else{
            //? JsonUtility does not support nullable objects
            //// metaData.defaultCutoff = new (bondCutoffs[0].cutoff ?? -1f, bondCutoffs[0].tolerance ?? 0f);
            metaData.defaultCutoff = new (bondCutoffs[0].cutoff, bondCutoffs[0].tolerance);
            Debug.Log($"Default bond cutoff found: {metaData.defaultCutoff.cutoff}, {metaData.defaultCutoff.tolerance}");
        }

        foreach (var bondCutoff in bondCutoffs)
        {
            if(bondCutoff.bond.ToLower() == "default"){
                Debug.LogWarning($"Default bond cutoff found, but it is not the first line, this value won't be used");
                continue;
            }
            // split the bond string into two atoms
            string[] atoms = bondCutoff.bond.Split('-');
            if (atoms.Length != 2){
                Debug.LogError($"Invalid bond format: {bondCutoff.bond}. Expected format: 'A-B'");
                continue;
            }
            int atom1 = MetaData.ElementToAtomicNumber( atoms[0].Trim());
            int atom2 = MetaData.ElementToAtomicNumber( atoms[1].Trim());

            // sort the atoms to avoid duplicate entries
            if (atom1 > atom2){
                int temp = atom1;
                atom1 = atom2;
                atom2 = temp;
            }

            // add the bond cutoff to the dictionary
            var key = (atom1 , atom2);
            metaData.cutoffs ??= new Dictionary<(int, int), MetaData.cutoffConfig>();
            if(metaData.cutoffs.ContainsKey(key)){
                Debug.LogWarning($"Duplicate bond cutoff found for {atom1}-{atom2}. Overriding the previous one.");
            }
            //? JsonUtility does not support nullable objects
            ////MetaData.cutoffConfig cutoffConfig = new (bondCutoff.cutoff ?? metaData.defaultCutoff.cutoff, bondCutoff.tolerance ?? metaData.defaultCutoff.tolerance);
            MetaData.cutoffConfig cutoffConfig = new (bondCutoff.cutoff, bondCutoff.tolerance);
            
            metaData.cutoffs[key] = cutoffConfig;
        }

        // find the max search distance
        float maxDist = 0;
        foreach (var cutoff in metaData.cutoffs)
        {
            maxDist = Mathf.Max(maxDist, cutoff.Value.cutoff + cutoff.Value.tolerance);
        }
        metaData.maxSearchDistance = maxDist;
        return metaData;
    }
}
