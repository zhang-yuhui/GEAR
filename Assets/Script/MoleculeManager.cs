using UnityEngine;
using Helper;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
public class MoleculeManager : MonoBehaviour
{
    public string fileName = "CO2";
    public string cutoffFileName = "bondCutoff";
    public string foldername = "Casioh";
    public MultiFileImporter multiFileImporter;
    public DataImporter dataImporter;
    private MoleculeData moleculeData;
    public Transform parent;
    private MetaData metaData;
    public TimeController timeController;
    public BondManager bondManager;
    public AtomManager atomManager;
    public BondCutoffLoader bondCutoffLoader;
    private Dictionary<int, int> currentPositions;
    private RangeNeighborSearch rangeNeighborSearch;
    private List<BondFrameData> bondData;
    public float maxSize = 1.0f;
    public float scale = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dataImporter = new ();
        bondCutoffLoader = new ();
        multiFileImporter = new ();
        //moleculeData = dataImporter.LoadData(fileName);
        moleculeData = multiFileImporter.LoadData(foldername);
        int frames = moleculeData.frameCount;

        metaData = bondCutoffLoader.loadMetaData(cutoffFileName);
        metaData.maxDist = 2.0f;
        metaData.minDist = 1.5f;

        foreach(var kvp in metaData.cutoffs){
            Debug.Log($"bond: {kvp.Key.Item1}-{kvp.Key.Item2}, cutoff: {kvp.Value.cutoff}, tolerance: {kvp.Value.tolerance}");
        }

        bondManager.parent = parent;
        atomManager.parent = parent;

        Debug.Log("Loading data");
        atomManager.moleculeData = moleculeData;
        
        // Find the max and min pos
        metaData.minPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        metaData.maxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach(var atom in moleculeData.atoms){
            foreach(var position in atom.Value.positions){
                metaData.minPos = Vector3.Min(metaData.minPos, position.position);
                metaData.maxPos = Vector3.Max(metaData.maxPos, position.position);
            }
        }
        /*parent.localScale = new Vector3((metaData.maxPos.x - metaData.minPos.x) * scale, 
                                        (metaData.maxPos.y - metaData.minPos.y) * scale,
                                        (metaData.maxPos.z - metaData.minPos.z) * scale);*/
        
        // Scale the pose accordingly
        parent.localScale = new Vector3(scale, scale, scale);
        Debug.Log($"minPos: {metaData.minPos}, maxPos: {metaData.maxPos}, scale: {parent.localScale}");

        // Pre-process all the bond information
        Debug.Log("Finished loading data, calculating all the bonds");
        rangeNeighborSearch = new RangeNeighborSearch(metaData.maxSearchDistance, metaData);
        bondData = new ();
        var positions = new Dictionary<int, Vector3>();
        var atomTypes = new Dictionary<int, int>();

        foreach (var (key, atom) in moleculeData.atoms)
            atomTypes.Add(atom.id, atom.type);

        for(int i = 0; i < frames; i++){
            BondFrameData bondFrameData = new();
            bondFrameData.bonds = new();

            // Extract all positions of current frame
            foreach (var (key, atom) in moleculeData.atoms){
                ////if(atom.positions[i].time == -1)
                positions.Add(atom.id, atom.positions[i].position);
            }

            // First update all the positions in neighbor search
            foreach(var (id, position) in positions){
                rangeNeighborSearch.AddPoint(id, position, atomTypes[id]);
            }

            // Then search all the neighbors
            foreach(var (id, position) in positions){
                var neighborList = rangeNeighborSearch.SearchNeighbors(id);
                foreach(var neighbor in neighborList){
                    
                    if(id < neighbor){ // Ensure that the pair is unique, the smaller id is always first
                        BondData data = new(){type1 = atomTypes[id], type2 = atomTypes[neighbor]};
                        bondFrameData.bonds.TryAdd((id, neighbor), data);
                    }
                    else{
                        BondData data = new(){type1 = atomTypes[neighbor], type2 = atomTypes[id]};
                        bondFrameData.bonds.TryAdd((neighbor, id), data);
                    }
                }
            }
            positions.Clear();
            bondData.Add(bondFrameData);
        }
        Debug.Log("Finished bond calculation");

        //timeController.OnTimeUpdated += UpdateMolecule;
        timeController.OnFrameUpdated += UpdateFrame;
        timeController.maxFrame = frames;

        currentPositions = new Dictionary<int, int>();
        foreach (var atom in moleculeData.atoms)
        {
            currentPositions.Add(atom.Key, 0);
        }
        Debug.Log("Finished initialization");
    }
    void OnDestroy()
    {
        //timeController.OnTimeUpdated -= UpdateMolecule;
        timeController.OnFrameUpdated -= UpdateFrame;

    }
    public void UpdateMolecule(float currentTime){
        var positions = new Dictionary<int, Vector3>();
        var atomTypes = new Dictionary<int, int>();
        foreach (var atom in moleculeData.atoms)
        {
            if(atom.Value.positions[0].time >= currentTime)// Skip if the molecule has not been created yet
                continue;
            var position = InterpolatePosition(atom.Value.id, currentTime);
            positions.Add(atom.Value.id, position);
            atomTypes.Add(atom.Value.id, atom.Value.type);
        }
        // Update the molecule
        atomManager.UpdatePositions(positions);
        // Update the bonds
        foreach (var position in positions)
        {
            rangeNeighborSearch.AddPoint(position.Key, position.Value, atomTypes[position.Key]);
        }
        HashSet<(int, int)> neighborPairs = new HashSet<(int, int)>();
        foreach (var position in positions)
        {
            List<int> neighborList = rangeNeighborSearch.SearchNeighbors(position.Key);
            if(neighborList != null){ // prevent null reference exception
                foreach (var neighbor in neighborList)
                {
                    if(position.Key < neighbor) // Ensure that the pair is unique, the smaller id is always first
                        neighborPairs.Add((position.Key, neighbor));
                    else
                        neighborPairs.Add((neighbor, position.Key));
                }
            }
        }
        
        ////if(neighborPairs.Count != 0)
            ////Debug.Log($"Number of bonds: {neighborPairs.Count}");
        bondManager.UpdateBonds(positions, neighborPairs);
    }
    public void UpdateFrame(int frame){
        Debug.Log($"Current frame: {frame}, total: {moleculeData.frameCount}");
        var positions = new Dictionary<int, Vector3>();

        foreach (var (key, atom) in moleculeData.atoms){
            ////if(atom.positions[i].time == -1)
            positions.Add(atom.id, atom.positions[frame].position);
        }
        Debug.Log($"Current frame: {frame}, total atoms: {positions.Count}");
        atomManager.UpdatePositions(positions);
        HashSet<(int, int)> allNeighbors = new();
        foreach(var (bondPairs, types)in bondData[frame].bonds){
            allNeighbors.Add(bondPairs);
        }
        bondManager.UpdateBonds(positions, allNeighbors);

    }
    

    private Vector3 InterpolatePosition(int id, float currentTime)
    {
        // Find two PositionData entries surrounding currentTime
        int index = currentPositions[id];
        var positions = moleculeData.atoms[id].positions;
        if(positions[^1].time <= currentTime) 
            return positions[^1].position;
        if(positions[index + 1].time <= currentTime){
            currentPositions[id] ++;
            index = currentPositions[id];
        }

        var before = positions[index];
        var after = positions[index + 1];

        // Linear interpolation
        float t = (currentTime - before.time) / (after.time - before.time);
        return Vector3.Lerp(before.position, after.position, t);
    }
}
