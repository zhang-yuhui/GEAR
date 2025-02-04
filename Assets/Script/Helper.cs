using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
namespace Helper{
    
    public struct Data
    {
        public Vector3 position;
        public float time;
        public int id;
        public string type;
        public Data(Vector3 position, float time, int id, string type){
            this.position = position;
            this.time = time;
            this.id = id;
            this.type = type;
        }
    }
    public struct PositionData{
        public float time; // if  set to -1, the atom does not exists at this time
        public Vector3 position;
    }
    public struct AtomData{
        public int id;
        public int type;
        public List<PositionData> positions;
    }
    public struct MoleculeData{
        public int frameCount;
        public Dictionary<int, AtomData> atoms;
    }
    public struct BondFrameData{
        public int frame;
        public Dictionary<(int, int), BondData> bonds;
    }
    public struct BondData{
        public int type1;
        public int type2;
    }
    public class MetaData{
        public Vector3 minPos;
        public Vector3 maxPos;
        public float minDist;
        public float maxDist;
        public float maxSearchDistance;
        public Dictionary<(int, int), cutoffConfig> cutoffs;
        public cutoffConfig defaultCutoff = new ();
        public class cutoffConfig{
            public float cutoff = -1f;
            public float tolerance = 0f;
            public cutoffConfig(float cutoff, float tolerance){
                if(tolerance < 0){
                    tolerance = 0;
                    Debug.LogWarning($"tolerance is negative: {tolerance}, set to 0");
                }
                
                if(cutoff < 0){
                    if (cutoff != -1f)
                        Debug.LogWarning($"cutoff is negative: {cutoff}, set to -1");
                    cutoff = -1;
                    tolerance = 0f;
                } else {
                    float minDist = cutoff - tolerance;
                    float maxDist = cutoff + tolerance;
                    if(minDist <= 0 || maxDist <= 0){
                        Debug.LogWarning($"minDist or maxDist is invalid: {minDist}, {maxDist}");
                        cutoff = -1f;
                        tolerance = 0f;
                    } 
                }

                this.cutoff = cutoff;
                this.tolerance = tolerance;

            }
            public cutoffConfig(){}
        }
        public static int  ElementToAtomicNumber(string elementSymbol)
        {
            // Create a dictionary to map element symbols to atomic numbers
            Dictionary<string, int> periodicTable = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase)
            {
                {"H", 1}, {"He", 2}, {"Li", 3}, {"Be", 4}, {"B", 5},
                {"C", 6}, {"N", 7}, {"O", 8}, {"F", 9}, {"Ne", 10},
                {"Na", 11}, {"Mg", 12}, {"Al", 13}, {"Si", 14}, {"P", 15},
                {"S", 16}, {"Cl", 17}, {"Ar", 18}, {"K", 19}, {"Ca", 20},
                {"Sc", 21}, {"Ti", 22}, {"V", 23}, {"Cr", 24}, {"Mn", 25},
                {"Fe", 26}, {"Co", 27}, {"Ni", 28}, {"Cu", 29}, {"Zn", 30},
                {"Ga", 31}, {"Ge", 32}, {"As", 33}, {"Se", 34}, {"Br", 35},
                {"Kr", 36}, {"Rb", 37}, {"Sr", 38}, {"Y", 39}, {"Zr", 40},
                {"Nb", 41}, {"Mo", 42}, {"Tc", 43}, {"Ru", 44}, {"Rh", 45},
                {"Pd", 46}, {"Ag", 47}, {"Cd", 48}, {"In", 49}, {"Sn", 50},
                {"Sb", 51}, {"Te", 52}, {"I", 53}, {"Xe", 54}, {"Cs", 55},
                {"Ba", 56}, {"La", 57}, {"Ce", 58}, {"Pr", 59}, {"Nd", 60},
                {"Pm", 61}, {"Sm", 62}, {"Eu", 63}, {"Gd", 64}, {"Tb", 65},
                {"Dy", 66}, {"Ho", 67}, {"Er", 68}, {"Tm", 69}, {"Yb", 70},
                {"Lu", 71}, {"Hf", 72}, {"Ta", 73}, {"W", 74}, {"Re", 75},
                {"Os", 76}, {"Ir", 77}, {"Pt", 78}, {"Au", 79}, {"Hg", 80},
                {"Tl", 81}, {"Pb", 82}, {"Bi", 83}, {"Po", 84}, {"At", 85},
                {"Rn", 86}, {"Fr", 87}, {"Ra", 88}, {"Ac", 89}, {"Th", 90},
                {"Pa", 91}, {"U", 92}, {"Np", 93}, {"Pu", 94}, {"Am", 95},
                {"Cm", 96}, {"Bk", 97}, {"Cf", 98}, {"Es", 99}, {"Fm", 100},
                {"Md", 101}, {"No", 102}, {"Lr", 103}, {"Rf", 104}, {"Db", 105},
                {"Sg", 106}, {"Bh", 107}, {"Hs", 108}, {"Mt", 109}, {"Ds", 110},
                {"Rg", 111}, {"Cn", 112}, {"Nh", 113}, {"Fl", 114}, {"Mc", 115},
                {"Lv", 116}, {"Ts", 117}, {"Og", 118}
            };

            // Try to get the atomic number from the dictionary
            if (periodicTable.TryGetValue(elementSymbol, out int atomicNumber))
            {
                return atomicNumber;
            }

            // If the element symbol is not found, return 0
            return 0;
        }
        public static string AtomicNumberToElement(int atomicNumber)
        {
            // Create a dictionary to map atomic numbers to element symbols
            Dictionary<int, string> periodicTable = new Dictionary<int, string>
            {
                {1, "H"}, {2, "He"}, {3, "Li"}, {4, "Be"}, {5, "B"},
                {6, "C"}, {7, "N"}, {8, "O"}, {9, "F"}, {10, "Ne"},
                {11, "Na"}, {12, "Mg"}, {13, "Al"}, {14, "Si"}, {15, "P"},
                {16, "S"}, {17, "Cl"}, {18, "Ar"}, {19, "K"}, {20, "Ca"},
                {21, "Sc"}, {22, "Ti"}, {23, "V"}, {24, "Cr"}, {25, "Mn"},
                {26, "Fe"}, {27, "Co"}, {28, "Ni"}, {29, "Cu"}, {30, "Zn"},
                {31, "Ga"}, {32, "Ge"}, {33, "As"}, {34, "Se"}, {35, "Br"},
                {36, "Kr"}, {37, "Rb"}, {38, "Sr"}, {39, "Y"}, {40, "Zr"},
                {41, "Nb"}, {42, "Mo"}, {43, "Tc"}, {44, "Ru"}, {45, "Rh"},
                {46, "Pd"}, {47, "Ag"}, {48, "Cd"}, {49, "In"}, {50, "Sn"},
                {51, "Sb"}, {52, "Te"}, {53, "I"}, {54, "Xe"}, {55, "Cs"},
                {56, "Ba"}, {57, "La"}, {58, "Ce"}, {59, "Pr"}, {60, "Nd"},
                {61, "Pm"}, {62, "Sm"}, {63, "Eu"}, {64, "Gd"}, {65, "Tb"},
                {66, "Dy"}, {67, "Ho"}, {68, "Er"}, {69, "Tm"}, {70, "Yb"},
                {71, "Lu"}, {72, "Hf"}, {73, "Ta"}, {74, "W"}, {75, "Re"},
                {76, "Os"}, {77, "Ir"}, {78, "Pt"}, {79, "Au"}, {80, "Hg"},
                {81, "Tl"}, {82, "Pb"}, {83, "Bi"}, {84, "Po"}, {85, "At"},
                {86, "Rn"}, {87, "Fr"}, {88, "Ra"}, {89, "Ac"}, {90, "Th"},
                {91, "Pa"}, {92, "U"}, {93, "Np"}, {94, "Pu"}, {95, "Am"},
                {96, "Cm"}, {97, "Bk"}, {98, "Cf"}, {99, "Es"}, {100, "Fm"},
                {101, "Md"}, {102, "No"}, {103, "Lr"}, {104, "Rf"}, {105, "Db"},
                {106, "Sg"}, {107, "Bh"}, {108, "Hs"}, {109, "Mt"}, {110, "Ds"},
                {111, "Rg"}, {112, "Cn"}, {113, "Nh"}, {114, "Fl"}, {115, "Mc"},
                {116, "Lv"}, {117, "Ts"}, {118, "Og"}
            };

            // Try to get the element symbol from the dictionary
            if (periodicTable.TryGetValue(atomicNumber, out string elementSymbol))
            {
                return elementSymbol;
            }

            // If the atomic number is not found, return "Unknown"
            return "X";
        }
    }
    
    public class RangeNeighborSearch{
        public RangeNeighborSearch(float maxSearchDistance, MetaData metaData){
            points = new ();
            atomTypes = new ();
            this.maxSearchDistance = maxSearchDistance;
            float offset = 0.1f;
            this.metaData = metaData;
            minPos = new float[3]{metaData.minPos.x - offset,
                                    metaData.minPos.y - offset,
                                    metaData.minPos.z - offset};
            maxPos = new float[3]{metaData.maxPos.x + offset,
                                    metaData.maxPos.y + offset,
                                    metaData.maxPos.z + offset};

            // check if the minPos is less than maxPos
            bool check = minPos[0] <= maxPos[0] && minPos[1] <= maxPos[1] && minPos[2] <= maxPos[2];

            if(maxSearchDistance <= 0 || !check){
                Debug.LogError("invalid argument for the RangeNeighborSearch");
                return;
            }
            binCount = new int[3]{(int)math.ceil((this.maxPos[0] - this.minPos[0]) / maxSearchDistance),
                                    (int)math.ceil((this.maxPos[1] - this.minPos[1]) / maxSearchDistance),
                                    (int)math.ceil((this.maxPos[2] - this.minPos[2]) / maxSearchDistance)};
            bins = new List<int>[binCount[0],binCount[1],binCount[2]];
        }
        public RangeNeighborSearch(Dictionary<int, Vector3> points, float maxSearchDistance, Vector3 minPos, Vector3 maxPos){
            //! Don't use this constructor
            this.points = new Dictionary<int, Vector3>(points);
            this.maxSearchDistance = maxSearchDistance;
            float offset = 0.1f;
            this.minPos = new float[3]{minPos.x - offset,
                                        minPos.y - offset,
                                        minPos.z - offset};
            this.maxPos = new float[3]{maxPos.x + offset,
                                        maxPos.y + offset,
                                        maxPos.z + offset};

            bool check = minPos.x <= maxPos.x || minPos.y <= maxPos.y ||minPos.z <= maxPos.z;
            if(points.Count == 0 || maxSearchDistance <= 0 || !check){
                Debug.LogError("invalid argument for the RangeNeighborSearch");
                return;
            }
            binCount = new int[3]{(int)(math.ceil(maxPos.x - minPos.x) / maxSearchDistance),
                                    (int)(math.ceil(maxPos.y - minPos.y) / maxSearchDistance),
                                    (int)(math.ceil(maxPos.z - minPos.z) / maxSearchDistance)};
            bins = new List<int>[binCount[0],binCount[1],binCount[2]];
            foreach(var point in points){
                var binIndex = GetBinIndex(point.Value);
                if(bins[binIndex[0],binIndex[1],binIndex[2]] == null)
                    bins[binIndex[0],binIndex[1],binIndex[2]] = new List<int>(){point.Key};
                bins[binIndex[0],binIndex[1],binIndex[2]].Add(point.Key);
            }
        }
        public List<int> SearchNeighbors(int id){
            if(!points.ContainsKey(id))
                return null;
            var position = points[id];
            // //Debug.Log($"searching for neighbors of {bins.Length} ");
            List<int> neighbors = new List<int>();
            var targetBin = GetBinIndex(position);
            //// float maxSq = maxDist*maxDist;
            //// float minSq = minDist*minDist;

            for(int x = math.max(0,targetBin[0] - 1); x <= math.min(targetBin[0] + 1, binCount[0] - 1); x++){
                for(int y = math.max(0,targetBin[1] - 1); y <= math.min(targetBin[1] + 1, binCount[1] - 1); y++){
                    for(int z = math.max(0,targetBin[2] - 1); z <= math.min(targetBin[2] + 1, binCount[2] - 1); z++){
                        if(bins[x,y,z] == null)
                            continue;
                        foreach(int index in bins[x,y,z]){
                            var offsetVector = points[index] - position;
                            var dist = offsetVector.magnitude;
                            ////Debug.Log($"{id} and neighbor: {index} has distance: {dist}");
                            int atom1 = atomTypes[id];
                            int atom2 = atomTypes[index];
                            if(formBond(atom1, atom2, dist) && index != id)
                            ////if(dist >= metaData.defaultCutoff.cutoff - metaData.defaultCutoff.tolerance && dist <= metaData.defaultCutoff.cutoff + metaData.defaultCutoff.tolerance)
                                neighbors.Add(index);
                        }
                    }
                }
            }
            return neighbors;
        }
        public void MovePoint(int index, Vector3 newPoint){
            if(!points.ContainsKey(index)){
                Debug.LogError($"does not found point with index; {index}");
                return;
            }
            Vector3 oldPoint;
            oldPoint = points[index];

            float[] tmp = { newPoint.x,  newPoint.y,  newPoint.z};
            for(int i = 0; i < 3; i++){
                if(tmp[i] < minPos[i] || tmp[i] > maxPos[i]){
                    Debug.LogError($"new position vector {newPoint} is out of range");
                    return;
                }
            }
            var oldIndex = GetBinIndex(oldPoint);
            var newIndex = GetBinIndex(newPoint);
            bool isSame = true;
            // check if the old index is the same as the new index
            for(int i = 0;i < 3; i++){
                if(oldIndex[i] != newIndex[i]){
                    isSame = false;
                    break;
                }
            }

            // if the old index is the same as the new index, just update the position
            if(isSame){
                points[index] = newPoint;
            } else {
                // else move the point from the old index to the new index
                points[index] = newPoint;
                bins[oldIndex[0], oldIndex[1], oldIndex[2]].Remove(index);
                if(bins[newIndex[0], newIndex[1], newIndex[2]] == null)
                    bins[newIndex[0], newIndex[1], newIndex[2]] = new List<int>(){index};
                else
                    bins[newIndex[0], newIndex[1], newIndex[2]].Add(index);
            }
        }
        public void AddPoint(int id, Vector3 position, int type){
            // if the point is in the points dictionary, move it to the new position
            if(points.ContainsKey(id)){
                MovePoint(id, position);
                return;
            }
            points.Add(id, position);
            atomTypes.Add(id, type);
            // if the point is not in the points dictionary, add it to the points dictionary
            int[] binIndex = GetBinIndex(position);
            ////Debug.Log($"binIndex: {binIndex[0]}, {binIndex[1]}, {binIndex[2]}");
            if(bins[binIndex[0], binIndex[1], binIndex[2]] == null)
                bins[binIndex[0], binIndex[1], binIndex[2]] = new List<int>(){id};
            else
                bins[binIndex[0], binIndex[1], binIndex[2]].Add(id);
        }

        public void RemovePoint(int id){
            ////Debug.Log("RemovePoint by id is not implemented yet");
            Vector3 position;
            if(points.TryGetValue(id, out position)){
                var index = GetBinIndex(position);
                bins[index[0], index[1], index[2]].Remove(id);
            } else {
                Debug.LogWarning($"trying to get a non exist point id: {id}");
            }
        }

        private bool formBond(int atom1, int atom2, float dist){
            if(metaData.cutoffs == null)
                return false;
            // check if the atoms are in the dictionary
            if(atom1 >atom2){
                (atom1, atom2) = (atom2, atom1);
            }
            if(metaData.cutoffs.TryGetValue((atom1, atom2), out var cutoff)){
                // if both are in the dictionary, check if the distance is within the range
                if(dist >= cutoff.cutoff - cutoff.tolerance && dist <= cutoff.cutoff + cutoff.tolerance)
                    return true;
            }else {
                if(dist >= metaData.defaultCutoff.cutoff - metaData.defaultCutoff.tolerance && dist <= metaData.defaultCutoff.cutoff + metaData.defaultCutoff.tolerance)
                return true;
            }
            return false;
        }

        private Dictionary<int, Vector3> points;
        private Dictionary<int, int> atomTypes;
        private float maxSearchDistance;
        private float[] minPos;
        private float[] maxPos;
        private MetaData metaData;
        private List<int>[,,] bins;
        private int[] binCount;
        private int[] GetBinIndex(Vector3 position){
            int[] binIndex = new int[3];
            float[] tmp = {position.x, position.y, position.z};
            for(int i = 0;i < 3; i++){
                binIndex[i] = (int)math.floor((tmp[i] - minPos[i])/maxSearchDistance);
            }
            return binIndex;
        }
    }
}
