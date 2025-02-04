using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Helper;
public class AtomManager : MonoBehaviour
{
    private Dictionary<int, GameObject> atoms;
    public MoleculeData moleculeData;
    private Renderer objectRenderer;
    public Transform parent;
    public float atomRadius = 0.3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        atoms = new Dictionary<int, GameObject>();
    }
    public void UpdatePositions(Dictionary<int, Vector3> positions){
        foreach (var position in positions)
        {
            if(atoms.ContainsKey(position.Key)){
                atoms[position.Key].transform.localPosition = position.Value;
            }else{
                GameObject prefab = Resources.Load<GameObject>(Path.Combine("Prefab","Atom"));
                if (prefab == null)
                {
                    Debug.LogError($"No prefab called Molecule");
                    return;
                }
                GameObject atom = Instantiate(prefab,position.Value, Quaternion.identity);
                atom.transform.SetParent(parent);
                atom.transform.localScale = new Vector3(atomRadius, atomRadius, atomRadius);
                objectRenderer = atom.GetComponent<Renderer>();
                var type = moleculeData.atoms[position.Key].type;
                string materialPath = Path.Combine("Material", MetaData.AtomicNumberToElement(type));
                Material material = Resources.Load<Material>(materialPath);
                if (material == null)
                {
                    Debug.LogError($"no material called {type}");
                }else{
                    objectRenderer.material = material;
                }
                atoms.Add(position.Key, atom);
            }
        }
    }
}
